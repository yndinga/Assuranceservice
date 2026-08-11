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
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return "System";
            }

            var headerName = httpContext.Request.Headers["X-User-Name"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerName))
            {
                return headerName.Trim();
            }

            var user = httpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var ocre = user.FindFirst("ocre")?.Value;
                if (!string.IsNullOrWhiteSpace(ocre))
                {
                    return ocre.Trim();
                }

                var displayName = user.FindFirstValue("displayName");
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    return displayName.Trim();
                }

                var claimName =
                    user.FindFirstValue(ClaimTypes.Name) ??
                    user.FindFirstValue("name") ??
                    user.Identity.Name;

                if (!string.IsNullOrWhiteSpace(claimName))
                {
                    return claimName.Trim();
                }
            }

            var headerCode = httpContext.Request.Headers["X-User-Code"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerCode))
            {
                return headerCode.Trim();
            }

            return "System";
        }
    }

    public string OrganisationCode
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return string.Empty;
            }

            var headerCode = httpContext.Request.Headers["X-Organisation-Code"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerCode))
            {
                return headerCode.Trim();
            }

            return httpContext.User.FindFirstValue("organisationCode")?.Trim() ?? string.Empty;
        }
    }

    public string OrganisationType
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return string.Empty;
            }

            var headerType = httpContext.Request.Headers["X-Organisation-Type"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerType))
            {
                return headerType.Trim();
            }

            return httpContext.User.FindFirstValue("organisationType")?.Trim() ?? string.Empty;
        }
    }
}
