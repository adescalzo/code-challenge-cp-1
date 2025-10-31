using Microsoft.AspNetCore.Routing;

namespace EmployeeChallenge.Infrastructure;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
