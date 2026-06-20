
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Roles
{
    // El objeto de respuesta
    public record RoleDto(string RolId, string RolName);

    public static class GetAllRolesQueryHandler
    {
        [Authorize]
        [WolverineGet("/api/rol/getall")]
        [Tags("Roles")]
        [EndpointSummary("Obtiene el listado de todos los roles disponibles")]
        public static async Task<IResult> Handle(ISaguntoDbContext dbContext, CancellationToken cancellationToken)
        {
            var roles = await dbContext.Roles
                .AsNoTracking()
                .Select(r => new RoleDto(r.Id.ToString(), r.Name))
                .ToListAsync(cancellationToken);

            return TypedResults.Ok(roles);
        }
    }
}