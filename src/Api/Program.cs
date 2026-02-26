using AssuranceService.Api.ExceptionHandling;
using AssuranceService.Application;
using AssuranceService.Application.Assurances.Commands;
using AssuranceService.Application.Assurances.Queries;
using AssuranceService.Application.Marchandises.Commands;
using AssuranceService.Application.Primes.Commands;
using AssuranceService.Application.Garanties.Commands;
using AssuranceService.Application.Garanties.Queries;
using AssuranceService.Application.Voyages.Commands;
using AssuranceService.Application.Voyages.Queries;
using AssuranceService.Infrastructure;
using AssuranceService.Api.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/", () => Results.Ok(new { message = "Le service assurance fonctionne", version = "v1.0.2", status = "OK" }))
    .WithName("ServiceStatus")
    .WithTags("Health");

// API v1 endpoints avec préfixe /api/v1/
var api = app.MapGroup("/api/v1");

// Assurances endpoints
api.MapPost("/assurances", async (CreateAssuranceCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/api/v1/assurances/{id}", new { id });
});

api.MapGet("/assurances", async (IMediator mediator) =>
{
    var assurances = await mediator.Send(new GetAllAssurancesQuery());
    return Results.Ok(assurances);
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

// Marchandises endpoints
api.MapPost("/marchandises", async (CreateMarchandiseCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/api/v1/marchandises/{id}", new { id });
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

// Voyages endpoints
api.MapPost("/voyages", async (CreateVoyageCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Created($"/api/v1/voyages/{id}", new { id });
});

api.MapGet("/voyages/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var voyage = await mediator.Send(new GetVoyageByIdQuery(id));
    return voyage is null ? Results.NotFound() : Results.Ok(voyage);
});

api.MapGet("/assurances/{assuranceId:guid}/voyages", async (Guid assuranceId, IMediator mediator) =>
{
    var voyages = await mediator.Send(new GetVoyagesByAssuranceIdQuery(assuranceId));
    return Results.Ok(voyages);
});

// ===== API Référentiel (même structure que DeclarationImportationService) =====
api.MapGet("/referentiel/pays", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Pays.Where(p => p.Actif).OrderBy(p => p.Nom).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielPays").WithTags("Referentiel");

api.MapGet("/referentiel/departements", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Departements.Where(d => d.Actif).OrderBy(d => d.Ordre).ThenBy(d => d.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielDepartements").WithTags("Referentiel");

api.MapGet("/referentiel/devises", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Devises.Where(d => d.Actif).OrderBy(d => d.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielDevises").WithTags("Referentiel");

api.MapGet("/referentiel/modules", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Modules.Where(m => m.Actif).OrderBy(m => m.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielModules").WithTags("Referentiel");

api.MapGet("/referentiel/aeroports", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Aeroports.Where(a => a.Actif).OrderBy(a => a.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielAeroports").WithTags("Referentiel");

api.MapGet("/referentiel/ports", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Ports.Where(p => p.Actif).OrderBy(p => p.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielPorts").WithTags("Referentiel");

api.MapGet("/referentiel/corridors", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Corridors.Where(c => c.Actif).OrderBy(c => c.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielCorridors").WithTags("Referentiel");

api.MapGet("/referentiel/routes", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Routes.Where(r => r.Actif).OrderBy(r => r.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielRoutes").WithTags("Referentiel");

api.MapGet("/referentiel/troncons", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.Troncons.Where(t => t.Actif).OrderBy(t => t.Code).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielTroncons").WithTags("Referentiel");

api.MapGet("/referentiel/taux-de-change", async (AssuranceService.Infrastructure.Data.AssuranceDbContext db) =>
{
    var list = await db.TauxDeChanges.Where(t => t.Actif).OrderBy(t => t.ValideDe).ToListAsync();
    return Results.Ok(list);
}).WithName("GetReferentielTauxDeChange").WithTags("Referentiel");

await app.RunAsync();
