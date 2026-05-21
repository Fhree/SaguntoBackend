using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record UserByIdResponseDto(int Id, string Name, string SaguntinoCode,string Role);

    public static class GetUserByIdQueryHandler
    {
        [WolverineGet("/api/users/{id}")]
        [Tags("Users")]
        [EndpointSummary("Get user by Id")]
        public static async Task<UserByIdResponseDto?> Handle(int id, ISaguntoDbContext dbContext)
        { 
            var user = await dbContext.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) 
                return null;
            else 
                return new UserByIdResponseDto(user.Id, user.Name, user.SaguntinoCode, user.Role?.Name ?? "No role"); 
        }
    }
}