using ImTools;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using System.Globalization;
using System.Text;
using Wolverine.Http;

namespace Sagunto.Application.Features.Users
{

    public record UserByNameResponseDto(int Id, string Name, string Surname, string SaguntinoCode);

    public static class GetUserByNameQueryHandler
    {
        [WolverineGet("/api/users/name/{name}")]
        [Tags("Users")]
        [EndpointSummary("Get user by name")]
        public static async Task<List<UserByNameResponseDto>> Handle(string name, ISaguntoDbContext dbContext)
        {
            if (string.IsNullOrWhiteSpace(name))
                return [];

            string cleanedQuery = RemoveAccentsAndLowercase(name);

            var searchTokens = cleanedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (searchTokens.Length == 0)
                return [];

            var query = dbContext.Users.AsNoTracking();

            foreach (var token in searchTokens)
            {
                query = query.Where(u => u.NormalizedSearch.Contains(token));
            }

            var users = await query.ToListAsync();

            return users.Select(u => new UserByNameResponseDto(u.Id, u.Name, u.Surname, u.SaguntinoCode)).ToList();
        }

        private static string RemoveAccentsAndLowercase(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }
    }
}
