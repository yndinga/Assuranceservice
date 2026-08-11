using AssuranceService.Application.Assurances.Commands;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using Moq;
using Xunit;

namespace AssuranceService.Application.Tests;

public sealed class CreateAssuranceFromDossiersHandlerTests
{
    [Fact]
    public async Task Handle_RejectsDossiersWithDifferentPorts()
    {
        var first = CreateDossier("DI-001", "CG", "PNR", 10m);
        var second = CreateDossier("DI-002", "CG", "BZV", 10m);
        var fixture = CreateFixture(first, second);
        var command = CreateCommand(first, second);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Handler.Handle(command, CancellationToken.None));

        Assert.Contains("incompatible", error.Message, StringComparison.OrdinalIgnoreCase);
        fixture.AssuranceRepository.Verify(repository => repository.CreateAsync(It.IsAny<Assurance>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CopiesSelectedLineAndMarksPartialCoverage()
    {
        var dossier = CreateDossier("DI-001", "CG", "PNR", 10m);
        var fixture = CreateFixture(dossier);
        var sourceLine = dossier.Commandes.Single().LignesCommandes.Single();
        var selection = new AssuranceLigneSelection(
            dossier.Id,
            sourceLine.Id,
            Quantite: 4m,
            MasseBrute: 40m,
            MasseNette: 32m,
            Volume: 2m,
            ValeurDevise: 400m,
            ValeurXAF: 240_000m);
        var command = CreateCommand([dossier], [selection]);

        var result = await fixture.Handler.Handle(command, CancellationToken.None);

        var line = Assert.Single(result.Lignes);
        Assert.Equal(sourceLine.Id, line.SourceLigneDIId);
        Assert.Equal(4m, line.Quantite);
        Assert.True(line.EstPartielle);
        var declaration = Assert.Single(result.Declarations);
        Assert.Equal("CG", declaration.PaysEmbarquementCode);
        Assert.Equal("PNR", declaration.PortEmbarquementCode);
    }

    [Fact]
    public async Task Handle_AcceptsMultipleDossiersWithSameCountryAndPort()
    {
        var first = CreateDossier("DI-001", "CG", "PNR", 10m);
        var second = CreateDossier("DI-002", "CG", "PNR", 6m);
        var fixture = CreateFixture(first, second);

        var result = await fixture.Handler.Handle(CreateCommand(first, second), CancellationToken.None);

        Assert.Equal(2, result.Declarations.Count);
        Assert.Equal(2, result.Lignes.Count);
        Assert.All(result.Declarations, declaration =>
        {
            Assert.Equal("CG", declaration.PaysEmbarquementCode);
            Assert.Equal("PNR", declaration.PortEmbarquementCode);
        });
        fixture.InvoiceImporter.Verify(importer => importer.ImportAsync(
            result.ID,
            It.Is<IReadOnlyCollection<DeclarationInvoiceSelection>>(items =>
                items.Count == 2 && items.All(item => item.CommandeIds.Count == 1)),
            "tester",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_RejectsZeroForAnExplicitInsuredValue()
    {
        var dossier = CreateDossier("DI-001", "CG", "PNR", 10m);
        var line = dossier.Commandes.Single().LignesCommandes.Single();
        var command = CreateCommand(
            [dossier],
            [new AssuranceLigneSelection(dossier.Id, line.Id, 0m, null, null, null, null, null)]);

        var result = new CreateAssuranceFromDossiersValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("strictement positive", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_ExposesTypedDurationAndCalculatedTotals()
    {
        var dossier = CreateDossier("DI-001", "CG", "PNR", 10m);
        var fixture = CreateFixture(dossier);

        var result = await fixture.Handler.Handle(CreateCommand(dossier), CancellationToken.None);

        Assert.Equal(30, result.DureeJours);
        Assert.Equal(10m, result.Lignes.Sum(line => line.Quantite));
        Assert.Equal(600_000m, result.ValeurXAFTotale);
        Assert.Equal(5m, result.VolumeTotal);
    }

    private static TestFixture CreateFixture(params DeclarationDossierSource[] dossiers)
    {
        var declarationClient = new Mock<IDeclarationDossierClient>();
        foreach (var dossier in dossiers)
        {
            declarationClient
                .Setup(client => client.GetDossierAsync(dossier.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dossier);
        }

        var assuranceRepository = new Mock<IAssuranceRepository>();
        Assurance? saved = null;
        assuranceRepository
            .Setup(repository => repository.GetLineConsumptionsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, AssuranceLineConsumption>());
        assuranceRepository.Setup(repository => repository.CreateAsync(It.IsAny<Assurance>()))
            .ReturnsAsync((Assurance assurance) =>
            {
                saved = assurance;
                return assurance;
            });
        assuranceRepository.Setup(repository => repository.SaveChangesAsync()).Returns(Task.CompletedTask);
        assuranceRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => saved);

        var transportRepository = new Mock<ITransportDetailsRepository>();
        transportRepository.Setup(repository => repository.AddVoyageAsync(It.IsAny<Voyage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Voyage voyage, CancellationToken _) =>
            {
                voyage.Id = Guid.NewGuid();
                return voyage;
            });
        transportRepository.Setup(repository => repository.AddMaritimeAsync(It.IsAny<Maritime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var primeRepository = new Mock<IPrimeRepository>();
        primeRepository.Setup(repository => repository.CreateAsync(It.IsAny<Prime>()))
            .ReturnsAsync((Prime prime) => prime);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(service => service.UserName).Returns("tester");
        currentUser.SetupGet(service => service.OrganisationCode).Returns("IMP-001");
        currentUser.SetupGet(service => service.OrganisationType).Returns("IMPORTATEUR");

        var invoiceImporter = new Mock<IDeclarationInvoiceImporter>();
        invoiceImporter
            .Setup(importer => importer.ImportAsync(
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<DeclarationInvoiceSelection>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAssuranceFromDossiersHandler(
            declarationClient.Object,
            assuranceRepository.Object,
            transportRepository.Object,
            primeRepository.Object,
            Mock.Of<IPrimeCalculatorService>(),
            currentUser.Object,
            invoiceImporter.Object);

        return new TestFixture(handler, assuranceRepository, invoiceImporter);
    }

    private static CreateAssuranceFromDossiersCommand CreateCommand(params DeclarationDossierSource[] dossiers)
    {
        var selections = dossiers.Select(dossier =>
        {
            var line = dossier.Commandes.Single().LignesCommandes.Single();
            return new AssuranceLigneSelection(dossier.Id, line.Id, null, null, null, null, null, null);
        }).ToArray();
        return CreateCommand(dossiers, selections);
    }

    private static CreateAssuranceFromDossiersCommand CreateCommand(
        IReadOnlyCollection<DeclarationDossierSource> dossiers,
        IReadOnlyCollection<AssuranceLigneSelection> selections) =>
        new(
            dossiers.Select(dossier => dossier.Id).ToArray(),
            selections,
            TypeContrat: "FACULTES",
            Garantie: "GARANTIE",
            Assureur: "ASS-001",
            Intermediaire: null,
            TypePartenaire: "ASSUREUR",
            Duree: "30",
            NomTransporteur: null,
            LieuSejour: null,
            DureeSejour: null,
            Nature: null,
            Specificites: null,
            Conditionnement: null,
            DescriptionConditionnement: null,
            Marque: null,
            UniteStatistique: null,
            NumeroBL: null,
            NomNavire: null,
            TypeNavire: null,
            NumeroLTA: null,
            NumeroLV: null);

    private static DeclarationDossierSource CreateDossier(
        string number,
        string country,
        string port,
        decimal quantity)
    {
        var dossierId = Guid.NewGuid();
        var commandeId = Guid.NewGuid();
        return new DeclarationDossierSource
        {
            Id = dossierId,
            NoDossier = number,
            Etat = 50,
            OCRE = "IMP-001",
            ImportateurNom = "Importateur Test",
            ImportateurNIU = "NIU-001",
            ModeDeTransport = "MA",
            Commandes =
            [
                new DeclarationCommandeSource
                {
                    Id = commandeId,
                    DossierId = dossierId,
                    NoFacture = $"FAC-{number}",
                    Intitule = "Marchandise test",
                    Devise = "EUR",
                    PaysProvenance = country,
                    PaysEmbarquement = country,
                    PaysDestination = "CG",
                    PortEmbarquement = port,
                    PortDebarquement = "PNR",
                    LignesCommandes =
                    [
                        new DeclarationLigneCommandeSource
                        {
                            Id = Guid.NewGuid(),
                            CommandeId = commandeId,
                            NoLigne = 1,
                            PositionTarifaire = "01010101010",
                            Designation = "Marchandise test",
                            Quantite = quantity,
                            MasseBrute = 100m,
                            MasseNette = 80m,
                            Volume = 5m,
                            PrixUnitaire = 100m,
                            ValeurDevise = 1_000m,
                            ValeurFCFA = 600_000m,
                            UniteStatistique = "KGM",
                            PaysOrigine = country
                        }
                    ]
                }
            ]
        };
    }

    private sealed record TestFixture(
        CreateAssuranceFromDossiersHandler Handler,
        Mock<IAssuranceRepository> AssuranceRepository,
        Mock<IDeclarationInvoiceImporter> InvoiceImporter);
}
