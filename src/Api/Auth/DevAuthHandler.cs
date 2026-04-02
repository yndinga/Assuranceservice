using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Ton “contrat” actuel côté API : headers fournis par le client
        var ocre = Request.Headers["X-User-OCRE"].FirstOrDefault();
        var intermediaireId = Request.Headers["X-User-IntermediaireId"].FirstOrDefault();
        var assureurId = Request.Headers["X-User-AssureurId"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(ocre))
            return Task.FromResult(AuthenticateResult.NoResult()); // pas de user si header absent

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, ocre),
            new Claim("ocre", ocre),
        };

        if (Guid.TryParse(intermediaireId, out var iid))
            claims.Add(new Claim("intermediaireId", iid.ToString()));

        if (Guid.TryParse(assureurId, out var aid))
            claims.Add(new Claim("assureurId", aid.ToString()));

        // Exemple: tu peux aussi mapper des rôles selon assureur/intermédiaire
        // claims.Add(new Claim(ClaimTypes.Role, "Assureur"));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, Scheme.Name)));
    }
}