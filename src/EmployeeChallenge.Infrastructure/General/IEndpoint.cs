using Microsoft.AspNetCore.Routing;

namespace EmployeeChallenge.Infrastructure.General;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
