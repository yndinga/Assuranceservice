using System.Text.Json.Serialization;
using AssuranceService.Api.ExceptionHandling;
using AssuranceService.Application;
using AssuranceService.Application.Assurances.Commands;
using AssuranceService.Application.Assurances.Queries;
using AssuranceService.Application.Avenants.Commands;
using AssuranceService.Application.Avenants.Queries;
using AssuranceService.Application.DTOs;
using AssuranceService.Application.Primes.Commands;
using AssuranceService.Application.Garanties.Commands;
using AssuranceService.Application.Garanties.Queries;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Domain.Constants;
using AssuranceService.Infrastructure;
using AssuranceService.Infrastructure.Data;
using AssuranceService.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);

// Ã‰viter les cycles JSON (ex. TauxDeChange.Devise.TauxDeChanges.Devise...)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Gestion centralisÃ©e des erreurs â€” ordre important : du plus spÃ©cifique au plus gÃ©nÃ©ral
builder.Services.AddExceptionHandler<ValidationExceptionHandler>(); // FluentValidation â†’ 400
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();     // ArgumentException, InvalidOperationException, etc.
builder.Services.AddProblemDetails();

// Ajouter Health Checks
builder.Services.AddHealthChecks();

// Ajouter Consul
builder.Services.AddConsulServices(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Assurance Service API", 
        Version = "v1",
        Description = "A microservice for managing assurances"
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// ðŸ”„ Au dÃ©marrage : crÃ©ation de la base MS_ASSURANCE si elle n'existe pas, puis crÃ©ation/mise Ã  jour des tables (migrations)
await app.ApplyMigrationsAtStartupAsync();

// Swagger toujours disponible
app.UseSwagger();
app.UseSwaggerUI();

// Enregistrer le service auprÃ¨s de Consul
app.UseConsulServiceDiscovery();

// Mapper le endpoint Health Check
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { message = "Le service assurance fonctionne", version = "v1.1.0", status = "OK" }))
    .WithName("ServiceStatus")
    .WithTags("Health");

// API v1 endpoints avec prÃ©fixe /api/v1/
var api = app.MapGroup("/api/v1");

// Assurances endpoints
api.MapPost("/assurances", async (CreateAssuranceCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/api/v1/assurances/{id}", new
    {
        id,
        importateurNom = cmd.ImportateurNom,
        importateurNIU = cmd.ImportateurNIU,
        dateDebut = cmd.DateDebut,
        dateFin = cmd.DateFin,
        typeContrat = cmd.TypeContrat,
        duree = cmd.Duree,
        modeDeTransport = cmd.ModeDeTransport,
        assureur = cmd.Assureur,
        intermediaire = cmd.Intermediaire,
        typePartenaire = cmd.TypePartenaire,
        garantie = cmd.Garantie,
        ocre = cmd.OCRE,
        statut = cmd.Statut,
        designation = cmd.Designation,
        nature = cmd.Nature,
        specificites = cmd.Specificites,
        conditionnement = cmd.Conditionnement,
        description = cmd.Description,
        valeurFCFA = cmd.ValeurFCFA,
        valeurDevise = cmd.ValeurDevise,
        devise = cmd.Devise,
        masseBrute = cmd.MasseBrute,
        uniteStatistique = cmd.UniteStatistique,
        marque = cmd.Marque,
        nomTransporteur = cmd.NomTransporteur,
        nomNavire = cmd.NomNavire,
        typeNavire = cmd.TypeNavire,
        transit = cmd.Transit,
        paysProvenance = cmd.PaysProvenance,
        paysDestination = cmd.PaysDestination,
        portEmbarquement = cmd.PortEmbarquement,
        portDebarquement = cmd.PortDebarquement,
        aeroportEmbarquement = cmd.AeroportEmbarquement,
        aeroportDebarquement = cmd.AeroportDebarquement,
        routeNationale = cmd.RouteNationale,
        numeroBL = cmd.NumeroBL,
        numeroLTA = cmd.NumeroLTA,
        numeroLV = cmd.NumeroLV
    });
});

api.MapPost("/assurances/from-dossier/{dossierId:guid}", async (Guid dossierId, CreateAssuranceFromDossierRequest body, IMediator mediator) =>
{
    var assurance = await mediator.Send(new CreateAssuranceFromDossierCommand(
        dossierId,
        body.TypeContrat,
        body.Garantie,
        body.Assureur,
        body.Intermediaire,
        body.TypePartenaire,
        body.Duree,
        body.NomTransporteur,
        body.Transit,
        body.Nature,
        body.Specificites,
        body.Conditionnement,
        body.DescriptionConditionnement,
        body.Marque,
        body.UniteStatistique,
        body.NumeroBL,
        body.NomNavire,
        body.TypeNavire,
        body.NumeroLTA,
        body.NumeroLV));
    return Results.Created($"/api/v1/assurances/{assurance.ID}", assurance);
});

api.MapPost("/assurances/from-dossiers", async (CreateAssuranceFromDossiersRequest body, IMediator mediator) =>
{
    var assurance = await mediator.Send(new CreateAssuranceFromDossiersCommand(
        body.DossierIds,
        body.Lignes.Select(line => new AssuranceLigneSelection(
            line.DossierId,
            line.LigneDIId,
            line.Quantite,
            line.MasseBrute,
            line.MasseNette,
            line.Volume,
            line.ValeurDevise,
            line.ValeurXAF)).ToArray(),
        body.TypeContrat,
        body.Garantie,
        body.Assureur,
        body.Intermediaire,
        body.TypePartenaire,
        body.Duree,
        body.NomTransporteur,
        body.Transit,
        body.Nature,
        body.Specificites,
        body.Conditionnement,
        body.DescriptionConditionnement,
        body.Marque,
        body.UniteStatistique,
        body.NumeroBL,
        body.NomNavire,
        body.TypeNavire,
        body.NumeroLTA,
        body.NumeroLV));

    return Results.Created($"/api/v1/assurances/{assurance.ID}", assurance);
})
    .WithName("CreateAssuranceFromDossiers")
    .WithTags("Assurances");

api.MapGet("/assurances/dossiers-eligibles", async (
    string? search,
    IDeclarationDossierClient declarationClient,
    IAssuranceRepository assuranceRepository,
    ICurrentUserService currentUser,
    CancellationToken cancellationToken) =>
{
    var organisation = NormalizeCode(currentUser.OrganisationCode);
    if (string.IsNullOrWhiteSpace(organisation))
        return Results.BadRequest(new { message = "Le code de l'organisation connectée est obligatoire." });

    var term = search?.Trim();
    var dossiers = await declarationClient.GetDossiersAsync(cancellationToken);
    var accessibleDossiers = dossiers
        .Where(dossier => dossier.Etat == 50)
        .Where(dossier => string.Equals(dossier.Regime?.Trim(), "DI", StringComparison.OrdinalIgnoreCase))
        .Where(dossier => string.Equals(NormalizeCode(dossier.OCRE), organisation, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(NormalizeCode(dossier.Transitaire), organisation, StringComparison.OrdinalIgnoreCase))
        .ToArray();

    // La liste DI est volontairement légère et peut ne pas contenir les commandes/lignes.
    // Recharger les détails avant de calculer les reliquats et de rechercher par facture.
    var detailedDossiers = await Task.WhenAll(accessibleDossiers.Select(dossier =>
        declarationClient.GetDossierAsync(dossier.Id, cancellationToken)));
    var eligible = detailedDossiers
        .Where(dossier => dossier is not null)
        .Select(dossier => dossier!)
        .Where(dossier => string.IsNullOrWhiteSpace(term)
                          || (dossier.NoDossier?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                          || (dossier.ImportateurNom?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                          || (dossier.ImportateurNIU?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                          || (dossier.Transitaire?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                          || dossier.Commandes.Any(commande =>
                              commande.NoFacture?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
        .OrderByDescending(dossier => dossier.NoDossier)
        .ToArray();

    var sourceLineIds = eligible
        .SelectMany(dossier => dossier.Commandes)
        .SelectMany(commande => commande.LignesCommandes)
        .Select(ligne => ligne.Id)
        .Distinct()
        .ToArray();
    var consumptions = await assuranceRepository.GetLineConsumptionsAsync(
        sourceLineIds,
        cancellationToken: cancellationToken);

    var withRemainingAmounts = eligible
        .Select(dossier => dossier with
        {
            Commandes = dossier.Commandes
                .Select(commande => commande with
                {
                    LignesCommandes = commande.LignesCommandes
                        .Select(ligne =>
                        {
                            var consumed = consumptions.GetValueOrDefault(ligne.Id)
                                ?? new AssuranceLineConsumption(0m, 0m, 0m, 0m, 0m, 0m);
                            return ligne with
                            {
                                Quantite = RemainingAmount(ligne.Quantite, consumed.Quantite),
                                MasseBrute = RemainingAmount(ligne.MasseBrute, consumed.MasseBrute),
                                MasseNette = RemainingAmount(ligne.MasseNette, consumed.MasseNette),
                                Volume = RemainingAmount(ligne.Volume, consumed.Volume),
                                ValeurDevise = RemainingAmount(ligne.ValeurDevise, consumed.ValeurDevise),
                                ValeurFCFA = RemainingAmount(ligne.ValeurFCFA, consumed.ValeurXAF)
                            };
                        })
                        .Where(HasInsurableRemainingAmount)
                        .ToArray()
                })
                .Where(commande => commande.LignesCommandes.Count > 0)
                .ToArray()
        })
        .Where(dossier => dossier.Commandes.Count > 0)
        .ToArray();

    return Results.Ok(withRemainingAmounts);
})
    .WithName("GetEligibleDossiersForAssurance")
    .WithTags("Assurances");

api.MapGet("/assurances", async (ICurrentUserService currentUser, IMediator mediator, string? search) =>
{
    // IdentitÃ© de l'organisation connectÃ©e : fournie par la Gateway (X-Organisation-Code / X-Organisation-Type), pas saisie par l'utilisateur
    var result = await mediator.Send(new GetAllAssurancesQuery(
        Search: search,
        OrganisationCode: string.IsNullOrWhiteSpace(currentUser.OrganisationCode) ? null : currentUser.OrganisationCode,
        OrganisationType: string.IsNullOrWhiteSpace(currentUser.OrganisationType) ? null : currentUser.OrganisationType));
    return Results.Ok(result);
});

api.MapGet("/assurances/assurancelist", async (
    ICurrentUserService currentUser,
    AssuranceDbContext db,
    string? search,
    CancellationToken cancellationToken) =>
{
    var organisation = NormalizeCode(currentUser.OrganisationCode);
    if (string.IsNullOrWhiteSpace(organisation))
    {
        return Results.BadRequest(new { message = "Le code SEG de l'organisation connectee est obligatoire." });
    }

    var query = db.VisaAssurances
        .AsNoTracking()
        .Include(visa => visa.Assurance)
            .ThenInclude(assurance => assurance!.Garantie)
        .Include(visa => visa.Assurance)
            .ThenInclude(assurance => assurance!.Voyage)
                .ThenInclude(voyage => voyage!.Maritime)
        .Include(visa => visa.Assurance)
            .ThenInclude(assurance => assurance!.Voyage)
                .ThenInclude(voyage => voyage!.Fluvial)
        .Include(visa => visa.Assurance)
            .ThenInclude(assurance => assurance!.Voyage)
                .ThenInclude(voyage => voyage!.Aerien)
        .Include(visa => visa.Assurance)
            .ThenInclude(assurance => assurance!.Voyage)
                .ThenInclude(voyage => voyage!.Routier)
        .Include(visa => visa.Assurance)
            .ThenInclude(assurance => assurance!.Primes)
        .Where(visa =>
            visa.Assurance != null &&
            visa.Organisation.Trim().ToUpper() == organisation);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        query = query.Where(visa =>
            visa.Assurance != null &&
            (
                (visa.Assurance.NoPolice ?? string.Empty).Contains(term) ||
                (visa.Assurance.NumeroCert ?? string.Empty).Contains(term) ||
                (visa.Assurance.NoFacture ?? string.Empty).Contains(term) ||
                (visa.Assurance.ImportateurNom ?? string.Empty).Contains(term) ||
                (visa.Assurance.ImportateurNIU ?? string.Empty).Contains(term) ||
                (visa.Assurance.ModeDeTransport ?? string.Empty).Contains(term)
            ));
    }

    var visas = await query
        .OrderByDescending(visa => visa.Assurance!.CreerLe)
        .ToListAsync(cancellationToken);

    var result = visas.Select(visa => new AssuranceVisaListItem(
        visa.Assurance!.ToDto(),
        visa.Id,
        visa.TypePartenaire,
        visa.Organisation,
        visa.VisaOK,
        !visa.VisaOK && (StatutAssuranceCodes.IsVisaDemande(visa.Assurance!.Etat) || StatutAssuranceCodes.IsModificationSoumise(visa.Assurance.Etat)),
        visa.Statut,
        visa.CreerLe,
        visa.ModifierLe));

    return Results.Ok(result);
});

api.MapGet("/assurances/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var assurance = await mediator.Send(new GetAssuranceByIdQuery(id));
    return assurance is null ? Results.NotFound() : Results.Ok(assurance);
});

api.MapPut("/assurances/{id:guid}", async (Guid id, UpdateAssuranceCommand cmd, IMediator mediator) =>
{
    var command = cmd with { Id = id };
    await mediator.Send(command);
    return Results.NoContent();
});

// Action distincte : l'intermÃ©diaire choisit la maison d'assurance (avant soumission ; l'assureur pourra accepter/refuser Ã  la signature)
api.MapPost("/assurances/{id:guid}/choisir-assureur", async (Guid id, ChoisirAssureurRequest body, IMediator mediator) =>
{
    await mediator.Send(new ChoisirAssureurCommand { AssuranceId = id, Assureur = body.Assureur });
    return Results.NoContent();
});

// Signature : assureur ou intermediaire. Decisions acceptees : 50/13 pour valider, 66/14 pour demander modification. Assurance en 79 ou 68.
api.MapPost("/assurances/{id:guid}/signer", async (Guid id, SignerAssuranceRequest body, IMediator mediator) =>
{
    await mediator.Send(new SignerAssuranceCommand
    {
        AssuranceId = id,
        TypePartenaire = body.TypePartenaire,
        Organisation = body.Organisation,
        Decision = body.Decision,
        VisaContent = body.VisaContent
    });
    return Results.NoContent();
});

api.MapDelete("/assurances/{id:guid}", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new DeleteAssuranceCommand(id));
    return Results.NoContent();
});

api.MapPost("/assurances/{id:guid}/start-process", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new StartAssuranceProcessCommand { AssuranceId = id });
    return Results.Ok(new { message = "Assurance process started", assuranceId = id });
});

// Ã‰tape 4: Soumettre l'assurance (gÃ©nÃ¨re NoPolice, NumeroCert, calcule la prime)
api.MapPost("/assurances/submit", async (SubmitAssuranceCommand cmd, IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(cmd);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Avenants endpoints (corrections sur une police dÃ©jÃ  validÃ©e/payÃ©e/approuvÃ©e)
api.MapPost("/assurances/{assuranceId:guid}/avenants", async (Guid assuranceId, EnregistrerAvenantBody body, IMediator mediator) =>
{
    var result = await mediator.Send(new EnregistrerAvenantCommand
    {
        AssuranceId = assuranceId,
        Type = body.Type,
        Motif = body.Motif,
        Assurance = body.Assurance
    });
    return Results.Created($"/api/v1/assurances/{assuranceId}/avenants/{result.AvenantId}", result);
}).WithTags("Avenants");

api.MapGet("/assurances/{assuranceId:guid}/avenants", async (Guid assuranceId, IMediator mediator) =>
{
    var avenants = await mediator.Send(new GetAvenantsByAssuranceIdQuery(assuranceId));
    return Results.Ok(avenants);
}).WithTags("Avenants");

api.MapGet("/assurances/{assuranceId:guid}/avenants/{avenantId:guid}", async (Guid assuranceId, Guid avenantId, IMediator mediator) =>
{
    var avenant = await mediator.Send(new GetAvenantByIdQuery(assuranceId, avenantId));
    return avenant is null ? Results.NotFound() : Results.Ok(avenant);
}).WithTags("Avenants");

api.MapGet("/assurances/{assuranceId:guid}/historiques", async (Guid assuranceId, IMediator mediator) =>
{
    var historiques = await mediator.Send(new GetHistoriquesByAssuranceIdQuery(assuranceId));
    return Results.Ok(historiques);
}).WithTags("Avenants");

// Primes endpoints
api.MapPost("/primes", async (CreatePrimeCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/api/v1/primes/{id}", new { id });
});

// Garanties endpoints (CRUD)
api.MapGet("/garanties", async (IMediator mediator) =>
{
    var garanties = await mediator.Send(new GetAllGarantiesQuery());
    return Results.Ok(garanties);
});

api.MapGet("/garanties/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var garantie = await mediator.Send(new GetGarantieByIdQuery(id));
    return garantie is null ? Results.NotFound() : Results.Ok(garantie);
});

api.MapPost("/garanties", async (CreateGarantieCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/garanties/{id}", new { id });
});

api.MapPut("/garanties/{id:guid}", async (Guid id, UpdateGarantieCommand cmd, IMediator mediator) =>
{
    var command = cmd with { Id = id };
    var updated = await mediator.Send(command);
    return updated ? Results.NoContent() : Results.NotFound();
});

api.MapDelete("/garanties/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var deleted = await mediator.Send(new DeleteGarantieCommand(id));
    return deleted ? Results.NoContent() : Results.NotFound();
});

// Documents (fichiers stockÃ©s dans MinIO)
api.MapPost("/assurances/{assuranceId:guid}/documents", async (Guid assuranceId, HttpContext httpContext, IObjectStorageService storage, IDocumentRepository docRepo, IConfiguration config, AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var assurance = await db.Assurances.FindAsync(assuranceId);
    if (assurance == null) return Results.NotFound();
    var file = httpContext.Request.Form.Files.FirstOrDefault();
    if (file == null || file.Length == 0) return Results.BadRequest(new { error = "Aucun fichier fourni." });
    var requestedType = httpContext.Request.Form["typeDocument"].FirstOrDefault()?.Trim().ToUpperInvariant();
    var typeDocument = string.IsNullOrWhiteSpace(requestedType) ? "PIECE_ASSURANCE" : requestedType;
    if (typeDocument.Length > 50 || typeDocument.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
        return Results.BadRequest(new { error = "Le type de document est invalide." });
    var bucketName = config["MinIO:BucketName"] ?? "assurances";
    await storage.EnsureBucketExistsAsync(bucketName);
    var objectKey = $"assurances/{assuranceId}/{Guid.NewGuid():N}_{file.FileName}";
    await using var stream = file.OpenReadStream();
    await storage.UploadAsync(bucketName, objectKey, stream, file.ContentType);
    var accessUrl = await storage.GetPresignedUrlAsync(bucketName, objectKey, 3600, false);
    var document = new Document
    {
        Id = Guid.NewGuid(),
        AssuranceId = assuranceId,
        TypeDocument = typeDocument,
        Description = file.FileName,
        DocumentUrl = objectKey,
        ContentType = file.ContentType,
        Taille = file.Length,
        CreerLe = DateTime.UtcNow
    };
    await docRepo.AddAsync(document);
    return Results.Created($"/api/v1/assurances/{assuranceId}/documents/{document.Id}", new { id = document.Id, document.TypeDocument, fileName = document.Description, accessUrl = accessUrl });
}).WithTags("Documents").DisableAntiforgery();

api.MapGet("/assurances/{assuranceId:guid}/documents", async (Guid assuranceId, IDocumentRepository docRepo, IObjectStorageService storage, IConfiguration config) =>
{
    var list = await docRepo.GetByAssuranceIdAsync(assuranceId);
    var bucketName = config["MinIO:BucketName"] ?? "assurances";
    var dtos = new List<object>();
    foreach (var d in list)
    {
        var url = await storage.GetPresignedUrlAsync(bucketName, d.DocumentUrl, 3600, false);
        dtos.Add(new { d.Id, d.TypeDocument, d.Description, d.ContentType, d.Taille, d.CreerLe, accessUrl = url });
    }
    return Results.Ok(dtos);
}).WithTags("Documents");

api.MapGet("/assurances/{assuranceId:guid}/documents/{documentId:guid}/download", async (Guid assuranceId, Guid documentId, IDocumentRepository docRepo, IObjectStorageService storage, IConfiguration config) =>
{
    var doc = await docRepo.GetByIdAsync(documentId);
    if (doc == null || doc.AssuranceId != assuranceId) return Results.NotFound();
    var bucketName = config["MinIO:BucketName"] ?? "assurances";
    var stream = await storage.GetObjectAsync(bucketName, doc.DocumentUrl);
    var contentType = string.IsNullOrWhiteSpace(doc.ContentType)
        ? "application/octet-stream"
        : doc.ContentType;
    var fileName = string.IsNullOrWhiteSpace(doc.Description)
        ? "document"
        : doc.Description;

    return Results.File(stream, contentType, fileName);
}).WithTags("Documents");

api.MapDelete("/assurances/{assuranceId:guid}/documents/{documentId:guid}", async (Guid assuranceId, Guid documentId, IDocumentRepository docRepo, IObjectStorageService storage, IConfiguration config) =>
{
    var doc = await docRepo.GetByIdAsync(documentId);
    if (doc == null || doc.AssuranceId != assuranceId) return Results.NotFound();
    var bucketName = config["MinIO:BucketName"] ?? "assurances";
    await storage.DeleteAsync(bucketName, doc.DocumentUrl);
    await docRepo.DeleteAsync(documentId);
    return Results.NoContent();
}).WithTags("Documents");

// ===== API RÃ©fÃ©rentiel =====
// GET /api/v1/referentiel/{path} (pays, departements, devises, modules, statuts, etc.)
static int ClampReferentielTake(int take)
{
    return Math.Clamp(take <= 0 ? 10 : take, 1, 50);
}

static IQueryable<T> ApplyReferentielSearch<T>(IQueryable<T> query, string? search)
{
    var term = search?.Trim();
    if (string.IsNullOrWhiteSpace(term))
    {
        return query;
    }

    var parameter = Expression.Parameter(typeof(T), "item");
    Expression? body = null;

    foreach (var propertyName in new[] { "Code", "Nom", "Libelle", "Description", "Type", "Module" })
    {
        var propertyInfo = typeof(T).GetProperty(propertyName);
        if (propertyInfo?.PropertyType != typeof(string))
        {
            continue;
        }

        var property = Expression.Property(parameter, propertyInfo);
        var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
        var contains = Expression.Call(property, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(term));
        var condition = Expression.AndAlso(notNull, contains);
        body = body is null ? condition : Expression.OrElse(body, condition);
    }

    return body is null ? query : query.Where(Expression.Lambda<Func<T, bool>>(body, parameter));
}

var referentielHandlers = new Func<AssuranceService.Infrastructure.Data.AssuranceDbContext, string?, int, Task<IResult>>[]
{
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Pays.Where(p => p.Actif).OrderBy(p => p.Nom), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Departements.Where(d => d.Actif).OrderBy(d => d.Ordre).ThenBy(d => d.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Devises.Where(d => d.Actif).OrderBy(d => d.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.TypeTransports.Where(t => t.Actif).OrderBy(t => t.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Statuts.OrderBy(s => s.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Aeroports.Where(a => a.Actif).OrderBy(a => a.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Ports.Where(p => p.Actif).OrderBy(p => p.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Corridors.Where(c => c.Actif).OrderBy(c => c.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Routes.Where(r => r.Actif).OrderBy(r => r.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Troncons.Where(t => t.Actif).OrderBy(t => t.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await db.TauxDeChanges.AsNoTracking().Where(t => t.Actif).OrderBy(t => t.ValideDe).Take(ClampReferentielTake(take)).Select(t => new { t.Id, t.DeviseId, DeviseCode = t.Devise != null ? t.Devise.Code : null, t.Source, t.Taux, t.ValideDe, t.Actif, t.CreerLe, t.ModifierLe }).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.UniteStatistiques.Where(u => u.Actif).OrderBy(u => u.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.Specificites.AsNoTracking().OrderBy(s => s.Nom), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.RoutesNationales.Where(r => r.Actif).OrderBy(r => r.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
    async (db, search, take) => Results.Ok(await ApplyReferentielSearch(db.TypePartenaires.Where(t => t.Actif).OrderBy(t => t.Code), search).Take(ClampReferentielTake(take)).ToListAsync()),
};

var referentielRoutes = new[] { ("pays", "GetReferentielPays"), ("departements", "GetReferentielDepartements"), ("devises", "GetReferentielDevises"), ("type-transports", "GetReferentielTypeTransports"), ("statuts", "GetReferentielStatuts"), ("aeroports", "GetReferentielAeroports"), ("ports", "GetReferentielPorts"), ("corridors", "GetReferentielCorridors"), ("routes", "GetReferentielRoutes"), ("troncons", "GetReferentielTroncons"), ("taux-de-change", "GetReferentielTauxDeChange"), ("unites-statistiques", "GetReferentielUnitesStatistiques"), ("specificites", "GetReferentielSpecificites"), ("routes-nationales", "GetReferentielRoutesNationales"), ("type-partenaires", "GetReferentielTypePartenaires") };

for (var i = 0; i < referentielRoutes.Length; i++)
{
    var (path, name) = referentielRoutes[i];
    var h = referentielHandlers[i];
    api.MapGet($"/referentiel/{path}", (AssuranceService.Infrastructure.Data.AssuranceDbContext db, string? search, int take = 10) => h(db, search, take)).WithName(name).WithTags("Referentiel");
}

// Alias legacy : modules â†’ type-transports
api.MapGet("/referentiel/modules", (AssuranceService.Infrastructure.Data.AssuranceDbContext db, string? search, int take = 10) => referentielHandlers[3](db, search, take)).WithName("GetReferentielModules").WithTags("Referentiel");

static async Task<List<Guid>> ResolvePaysIdsAsync(AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance)
{
    if (paysId.HasValue) return new List<Guid> { paysId.Value };
    if (string.IsNullOrWhiteSpace(paysProvenance)) return new List<Guid>();

    if (Guid.TryParse(paysProvenance, out var parsed)) return new List<Guid> { parsed };

    var normalized = paysProvenance.Trim().ToUpperInvariant();
    return await db.Pays
        .Where(p => p.Actif && (
            (p.Code != null && p.Code.ToUpper() == normalized) ||
            (p.Nom != null && p.Nom.ToUpper() == normalized)))
        .Select(p => p.Id)
        .ToListAsync();
}

// RÃ©fÃ©rentiel filtrÃ© par pays (paysProvenance -> ports/aÃ©roports/fleuves/corridors)
api.MapGet("/referentiel/ports/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysId (ou paysProvenance) requis." });

    var data = await db.Ports
        .Where(x => x.Actif && x.PaysId.HasValue && paysIds.Contains(x.PaysId.Value))
        .OrderBy(x => x.Nom)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetPortsByPays").WithTags("Referentiel");

api.MapGet("/referentiel/aeroports/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysId (ou paysProvenance) requis." });

    var data = await db.Aeroports
        .Where(x => x.Actif && x.PaysId.HasValue && paysIds.Contains(x.PaysId.Value))
        .OrderBy(x => x.Nom)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetAeroportsByPays").WithTags("Referentiel");

api.MapGet("/referentiel/fleuves/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysId (ou paysProvenance) requis." });

    var data = await db.Ports
        .Where(x => x.Actif &&
                    x.PaysId.HasValue &&
                    paysIds.Contains(x.PaysId.Value) &&
                    (x.Type == "FL" || x.Module == "FL"))
        .OrderBy(x => x.Nom)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetFleuvesByPays").WithTags("Referentiel");

api.MapGet("/referentiel/corridors/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysId (ou paysProvenance) requis." });

    var data = await db.Corridors
        .Where(x => x.Actif && x.PaysId.HasValue && paysIds.Contains(x.PaysId.Value))
        .OrderBy(x => x.Nom)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetCorridorsByPays").WithTags("Referentiel");

// Alias legacy pour coller au front historique Laravel
api.MapGet("/referentiel/getPorts", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysProvenance requis." });
    var data = await db.Ports.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value)).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

api.MapGet("/referentiel/getAeroports", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysProvenance requis." });
    var data = await db.Aeroports.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value)).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

api.MapGet("/referentiel/getFleuves", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysProvenance requis." });
    var data = await db.Ports.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value) && (x.Type == "FL" || x.Module == "FL")).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

api.MapGet("/referentiel/getCorridors", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "ParamÃ¨tre paysProvenance requis." });
    var data = await db.Corridors.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value)).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

await app.RunAsync();

static string NormalizeCode(string? value)
{
    return value?.Trim().ToUpperInvariant() ?? string.Empty;
}

static decimal? RemainingAmount(decimal? sourceAmount, decimal consumedAmount)
{
    return sourceAmount.HasValue
        ? Math.Max(0m, sourceAmount.Value - consumedAmount)
        : null;
}

static bool HasInsurableRemainingAmount(DeclarationLigneCommandeSource line)
{
    return new[]
    {
        line.Quantite,
        line.MasseBrute,
        line.MasseNette,
        line.Volume,
        line.ValeurDevise,
        line.ValeurFCFA
    }.Any(value => value > 0m);
}

public sealed record AssuranceVisaListItem(
    AssuranceDto Assurance,
    Guid VisaId,
    string TypePartenaire,
    string Organisation,
    bool VisaOK,
    bool PeutSigner,
    string StatutVisa,
    DateTime CreerLe,
    DateTime? ModifierLe);

public sealed record CreateAssuranceFromDossierRequest(
    string? TypeContrat,
    string? Garantie,
    string? Assureur,
    string? Intermediaire,
    string? TypePartenaire,
    string? Duree,
    string? NomTransporteur,
    string? Transit,
    string? Nature,
    string? Specificites,
    string? Conditionnement,
    string? DescriptionConditionnement,
    string? Marque,
    string? UniteStatistique,
    string? NumeroBL,
    string? NomNavire,
    string? TypeNavire,
    string? NumeroLTA,
    string? NumeroLV);

public sealed record AssuranceLigneSelectionRequest(
    Guid DossierId,
    Guid LigneDIId,
    decimal? Quantite,
    decimal? MasseBrute,
    decimal? MasseNette,
    decimal? Volume,
    decimal? ValeurDevise,
    decimal? ValeurXAF);

public sealed record CreateAssuranceFromDossiersRequest(
    IReadOnlyCollection<Guid> DossierIds,
    IReadOnlyCollection<AssuranceLigneSelectionRequest> Lignes,
    string? TypeContrat,
    string? Garantie,
    string? Assureur,
    string? Intermediaire,
    string? TypePartenaire,
    string? Duree,
    string? NomTransporteur,
    string? Transit,
    string? Nature,
    string? Specificites,
    string? Conditionnement,
    string? DescriptionConditionnement,
    string? Marque,
    string? UniteStatistique,
    string? NumeroBL,
    string? NomNavire,
    string? TypeNavire,
    string? NumeroLTA,
    string? NumeroLV);
