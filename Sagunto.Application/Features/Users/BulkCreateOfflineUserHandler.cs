using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users;

public record BulkCreateOfflineUserItemDto(string Name, string Surname);
public record BulkCreateOfflineUserCommand(List<BulkCreateOfflineUserItemDto> Users);

public static class BulkCreateOfflineUserHandler
{
    [WolverinePost("/api/admin/users/bulk")]
    [Tags("Admin")]
    [EndpointSummary("Bulk create offline Saguntino users")]
    public static async Task<IResult> Handle(BulkCreateOfflineUserCommand command, ISaguntoDbContext dbContext, CancellationToken cancellationToken)
    {
        if (command.Users == null || command.Users.Count == 0)
            return Results.BadRequest("La lista de usuarios está vacía.");

        var existingCodes = await dbContext.Users
            .Where(u => u.SaguntinoCode != null)
            .Select(u => u.SaguntinoCode)
            .ToListAsync(cancellationToken);

        var usedCodes = new HashSet<string>(existingCodes!);

        var existingUsers = await dbContext.Users
            .Select(u => new { u.Name, u.Surname })
            .ToListAsync(cancellationToken);

        var newUsersToInsert = new List<User>();

        foreach (var item in command.Users)
        {
            var trimmedName = item.Name.Trim();
            var trimmedSurname = item.Surname.Trim();

            bool userExists = existingUsers.Any(u =>
                u.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase) &&
                u.Surname.Equals(trimmedSurname, StringComparison.OrdinalIgnoreCase));

            if (userExists) continue;

            string saguntinoCode;
            do
            {
                saguntinoCode = GenerateSaguntinoCode();
            } while (usedCodes.Contains(saguntinoCode));

            usedCodes.Add(saguntinoCode);

            var newUser = new User(trimmedName, 2, saguntinoCode, trimmedSurname);
            newUsersToInsert.Add(newUser);
        }

        if (newUsersToInsert.Any())
        {
            dbContext.Users.AddRange(newUsersToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(new
        {
            message = $"Se han insertado {newUsersToInsert.Count} usuarios saguntinos nuevos."
        });
    }

    private static string GenerateSaguntinoCode()
    {
        char letter = (char)Random.Shared.Next(65, 91);
        int number = Random.Shared.Next(0, 100);
        return $"{letter}{number:D2}";
    }
}