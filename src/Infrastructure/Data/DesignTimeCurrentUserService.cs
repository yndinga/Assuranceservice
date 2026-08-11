using AssuranceService.Application.Common;

namespace AssuranceService.Infrastructure.Data;

internal sealed class DesignTimeCurrentUserService : ICurrentUserService
{
    public string UserName => "System";
    public string OrganisationCode => string.Empty;
    public string OrganisationType => string.Empty;
}
