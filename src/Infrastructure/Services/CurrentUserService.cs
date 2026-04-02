using AssuranceService.Application.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AssuranceService.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return "System";

            // Priorité : claim "ocre" (mapping DevAuthHandler)
            var ocre = user.FindFirst("ocre")?.Value;
            if (!string.IsNullOrWhiteSpace(ocre)) return ocre;

            // Ensuite le Name classique
            var name = user.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;

            return "System";
        }
    }
}

