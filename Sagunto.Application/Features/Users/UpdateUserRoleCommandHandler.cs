using Sagunto.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{
    public record UpdateUserRoleCommand(int UserId, int NewRoleId);

    public static class UpdateUserRoleCommandHandler
    {
        [WolverinePut("api/users/updateRol")]
        public static async Task<AcceptResponse?> Handle(UpdateUserRoleCommand command, ISaguntoDbContext dbContext)
        {
            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == command.UserId);
            if (user == null)
                return null;

            var newRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == command.NewRoleId);
            if (newRole == null) 
                return null;

            user.ChangeRole(newRole);

            return new AcceptResponse("api/users/updateRol");
        }
    }
}
