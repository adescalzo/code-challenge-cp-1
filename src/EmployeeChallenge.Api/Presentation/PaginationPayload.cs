using Microsoft.AspNetCore.Mvc;

namespace EmployeeChallenge.Api.Presentation;

internal record PaginationPayload(
    [property: FromQuery] int Page = 1,
    [property: FromQuery] int PageSize = 50
);
