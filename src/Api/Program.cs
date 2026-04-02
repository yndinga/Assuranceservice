using System.Text.Json.Serialization;
using AssuranceService.Api.ExceptionHandling;
using AssuranceService.Application;
using AssuranceService.Application.Assurances.Commands;
using AssuranceService.Application.Assurances.Queries;
using AssuranceService.Application.Primes.Commands;
using AssuranceService.Application.Garanties.Commands;
using AssuranceService.Application.Garanties.Queries;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure;
using AssuranceService.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Éviter les cycles JSON (ex. TauxDeChange.Devise.TauxDeChanges.Devise...)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Gestion centralisée des erreurs — ordre important : du plus spécifique au plus général
builder.Services.AddExceptionHandler<ValidationExceptionHandler>(); // FluentValidation → 400
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

// 🔄 Au démarrage : création de la base MS_ASSURANCE si elle n'existe pas, puis création/mise à jour des tables (migrations)
await app.ApplyMigrationsAtStartupAsync();

// Swagger toujours disponible
app.UseSwagger();
app.UseSwaggerUI();

// Enregistrer le service auprès de Consul
app.UseConsulServiceDiscovery();

// Mapper le endpoint Health Check
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { message = "Le service assurance fonctionne", version = "v1.1.0", status = "OK" }))
    .WithName("ServiceStatus")
    .WithTags("Health");

// API v1 endpoints avec préfixe /api/v1/
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
        module = cmd.Module,
        assureurId = cmd.AssureurId,
        intermediaireId = cmd.IntermediaireId,
        garantieId = cmd.GarantieId,
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
        lieuSejour = cmd.LieuSejour,
        dureeSejour = cmd.DureeSejour,
        paysProvenance = cmd.PaysProvenance,
        paysDestination = cmd.PaysDestination,
        portEmbarquement = cmd.PortEmbarquement,
        portDebarquement = cmd.PortDebarquement,
        aeroportEmbarquement = cmd.AeroportEmbarquement,
        aeroportDebarquement = cmd.AeroportDebarquement,
        routeNationale = cmd.RouteNationale
    });
});

api.MapGet("/assurances", async (HttpContext httpContext, IMediator mediator, string? search) =>
{
    // Identité utilisateur connecté : fournie par le système dans les en-têtes (pas saisis par l'utilisateur)
    var ocre = httpContext.Request.Headers["X-User-OCRE"].FirstOrDefault();
    var intermediaireIdHeader = httpContext.Request.Headers["X-User-IntermediaireId"].FirstOrDefault();
    var assureurIdHeader = httpContext.Request.Headers["X-User-AssureurId"].FirstOrDefault();
    Guid? intermediaireId = Guid.TryParse(intermediaireIdHeader, out var iid) ? iid : null;
    Guid? assureurId = Guid.TryParse(assureurIdHeader, out var aid) ? aid : null;

    var result = await mediator.Send(new GetAllAssurancesQuery(
        Search: search,
        Ocre: string.IsNullOrWhiteSpace(ocre) ? null : ocre,
        IntermediaireId: intermediaireId,
        AssureurId: assureurId));
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

// Action distincte : l'intermédiaire choisit la maison d'assurance (avant soumission ; l'assureur pourra accepter/refuser à la signature)
api.MapPost("/assurances/{id:guid}/choisir-assureur", async (Guid id, ChoisirAssureurRequest body, IMediator mediator) =>
{
    await mediator.Send(new ChoisirAssureurCommand { AssuranceId = id, AssureurId = body.AssureurId });
    return Results.NoContent();
});

// Signature : assureur ou intermédiaire — décision Validé (13) ou Modification demandée (14). Assurance doit être en Visa demandé (11) ou Modification soumise (12).
api.MapPost("/assurances/{id:guid}/signer", async (Guid id, SignerAssuranceRequest body, IMediator mediator) =>
{
    await mediator.Send(new SignerAssuranceCommand
    {
        AssuranceId = id,
        SignataireId = body.SignataireId,
        Decision = body.Decision
        // VisaContent : rempli côté serveur quand le signataire sera connecté (pas depuis le formulaire)
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

// Étape 4: Soumettre l'assurance (génère NoPolice, NumeroCert, calcule la prime)
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

// Documents (fichiers stockés dans MinIO)
api.MapPost("/assurances/{assuranceId:guid}/documents", async (Guid assuranceId, HttpContext httpContext, IObjectStorageService storage, IDocumentRepository docRepo, IConfiguration config, AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var assurance = await db.Assurances.FindAsync(assuranceId);
    if (assurance == null) return Results.NotFound();
    var file = httpContext.Request.Form.Files.FirstOrDefault();
    if (file == null || file.Length == 0) return Results.BadRequest(new { error = "Aucun fichier fourni." });
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
        Description = file.FileName,
        DocumentUrl = objectKey,
        ContentType = file.ContentType,
        Taille = file.Length,
        CreerLe = DateTime.UtcNow
    };
    await docRepo.AddAsync(document);
    return Results.Created($"/api/v1/assurances/{assuranceId}/documents/{document.Id}", new { id = document.Id, fileName = document.Description, accessUrl = accessUrl });
}).WithTags("Documents").DisableAntiforgery();

api.MapGet("/assurances/{assuranceId:guid}/documents", async (Guid assuranceId, IDocumentRepository docRepo, IObjectStorageService storage, IConfiguration config) =>
{
    var list = await docRepo.GetByAssuranceIdAsync(assuranceId);
    var bucketName = config["MinIO:BucketName"] ?? "assurances";
    var dtos = new List<object>();
    foreach (var d in list)
    {
        var url = await storage.GetPresignedUrlAsync(bucketName, d.DocumentUrl, 3600, false);
        dtos.Add(new { d.Id, d.Description, d.ContentType, d.Taille, d.CreerLe, accessUrl = url });
    }
    return Results.Ok(dtos);
}).WithTags("Documents");

api.MapGet("/assurances/{assuranceId:guid}/documents/{documentId:guid}/download", async (Guid assuranceId, Guid documentId, IDocumentRepository docRepo, IObjectStorageService storage, IConfiguration config) =>
{
    var doc = await docRepo.GetByIdAsync(documentId);
    if (doc == null || doc.AssuranceId != assuranceId) return Results.NotFound();
    var bucketName = config["MinIO:BucketName"] ?? "assurances";
    var url = await storage.GetPresignedUrlAsync(bucketName, doc.DocumentUrl, 600, false);
    return Results.Redirect(url);
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

// ===== API Référentiel =====
// GET /api/v1/referentiel/{path} (pays, departements, devises, modules, statuts, etc.)
var referentielHandlers = new Func<AssuranceService.Infrastructure.Data.AssuranceDbContext, Task<IResult>>[]
{
    async (db) => Results.Ok(await db.Pays.Where(p => p.Actif).OrderBy(p => p.Nom).ToListAsync()),
    async (db) => Results.Ok(await db.Departements.Where(d => d.Actif).OrderBy(d => d.Ordre).ThenBy(d => d.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Devises.Where(d => d.Actif).OrderBy(d => d.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Modules.Where(m => m.Actif).OrderBy(m => m.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Statuts.OrderBy(s => s.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Aeroports.Where(a => a.Actif).OrderBy(a => a.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Ports.Where(p => p.Actif).OrderBy(p => p.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Corridors.Where(c => c.Actif).OrderBy(c => c.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Routes.Where(r => r.Actif).OrderBy(r => r.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Troncons.Where(t => t.Actif).OrderBy(t => t.Code).ToListAsync()),
    async (db) => Results.Ok(await db.TauxDeChanges.AsNoTracking().Where(t => t.Actif).OrderBy(t => t.ValideDe).Select(t => new { t.Id, t.DeviseId, DeviseCode = t.Devise != null ? t.Devise.Code : null, t.Source, t.Taux, t.ValideDe, t.Actif, t.CreerLe, t.ModifierLe }).ToListAsync()),
    async (db) => Results.Ok(await db.UniteStatistiques.Where(u => u.Actif).OrderBy(u => u.Code).ToListAsync()),
    async (db) => Results.Ok(await db.Specificites.AsNoTracking().OrderBy(s => s.Nom).ToListAsync()),
    async (db) => Results.Ok(await db.TypeTransports.AsNoTracking().OrderBy(t => t.Nom).ToListAsync()),
};

var referentielRoutes = new[] { ("pays", "GetReferentielPays"), ("departements", "GetReferentielDepartements"), ("devises", "GetReferentielDevises"), ("modules", "GetReferentielModules"), ("statuts", "GetReferentielStatuts"), ("aeroports", "GetReferentielAeroports"), ("ports", "GetReferentielPorts"), ("corridors", "GetReferentielCorridors"), ("routes", "GetReferentielRoutes"), ("troncons", "GetReferentielTroncons"), ("taux-de-change", "GetReferentielTauxDeChange"), ("unites-statistiques", "GetReferentielUnitesStatistiques"), ("specificites", "GetReferentielSpecificites"), ("type-transports", "GetReferentielTypeTransports") };

for (var i = 0; i < referentielRoutes.Length; i++)
{
    var (path, name) = referentielRoutes[i];
    var h = referentielHandlers[i];
    api.MapGet($"/referentiel/{path}", (AssuranceService.Infrastructure.Data.AssuranceDbContext db) => h(db)).WithName(name).WithTags("Referentiel");
}

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

// Référentiel filtré par pays (paysProvenance -> ports/aéroports/fleuves/corridors)
api.MapGet("/referentiel/ports/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "Paramètre paysId (ou paysProvenance) requis." });

    var data = await db.Ports
        .Where(x => x.Actif && x.PaysId.HasValue && paysIds.Contains(x.PaysId.Value))
        .OrderBy(x => x.Nom)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetPortsByPays").WithTags("Referentiel");

api.MapGet("/referentiel/aeroports/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "Paramètre paysId (ou paysProvenance) requis." });

    var data = await db.Aeroports
        .Where(x => x.Actif && x.PaysId.HasValue && paysIds.Contains(x.PaysId.Value))
        .OrderBy(x => x.Nom)
        .ToListAsync();
    return Results.Ok(data);
}).WithName("GetAeroportsByPays").WithTags("Referentiel");

api.MapGet("/referentiel/fleuves/by-pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var paysIds = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "Paramètre paysId (ou paysProvenance) requis." });

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
    if (paysIds.Count == 0) return Results.BadRequest(new { error = "Paramètre paysId (ou paysProvenance) requis." });

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
    if (ids.Count == 0) return Results.BadRequest(new { error = "Paramètre paysProvenance requis." });
    var data = await db.Ports.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value)).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

api.MapGet("/referentiel/getAeroports", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "Paramètre paysProvenance requis." });
    var data = await db.Aeroports.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value)).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

api.MapGet("/referentiel/getFleuves", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "Paramètre paysProvenance requis." });
    var data = await db.Ports.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value) && (x.Type == "FL" || x.Module == "FL")).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

api.MapGet("/referentiel/getCorridors", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db, Guid? paysId, string? paysProvenance) =>
{
    var ids = await ResolvePaysIdsAsync(db, paysId, paysProvenance);
    if (ids.Count == 0) return Results.BadRequest(new { error = "Paramètre paysProvenance requis." });
    var data = await db.Corridors.Where(x => x.Actif && x.PaysId.HasValue && ids.Contains(x.PaysId.Value)).OrderBy(x => x.Nom).ToListAsync();
    return Results.Ok(data);
}).WithTags("Referentiel");

await app.RunAsync();
