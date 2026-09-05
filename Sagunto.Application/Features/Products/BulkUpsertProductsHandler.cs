using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Products
{

    public record BulkUpsertProductsItemDto(string Name, decimal PriceGuest, decimal PriceMember);

    public record BulkUpsertProductsCommand(List<BulkUpsertProductsItemDto> Products );

    public static class BulkUpsertProductsHandler
    {
        [WolverinePost("/api/products/bulkUpsert")]
        [Tags("Products")]
        [EndpointSummary("Upsert a collection of products")]
        public static async Task<IResult> Handle(BulkUpsertProductsCommand command, ISaguntoDbContext dbContext, CancellationToken cancellationToken)
        {
            if (command.Products == null || command.Products.Count == 0)
                return Results.BadRequest("La lista de productos no puede estar vacía.");

            var incomingNames = command.Products.Select(p => p.Name.Trim().ToLower()).ToList();

            var existingProducts = await dbContext.Products.Where(p => incomingNames.Contains(p.Name.ToLower())).ToListAsync(cancellationToken);

            foreach (var item in command.Products)
            {
                var trimmedName = item.Name.Trim();
                var existing = existingProducts.FirstOrDefault(p => p.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                    existing.UpdatePrices(item.PriceMember, item.PriceGuest);
                else
                    dbContext.Products.Add(new Product(trimmedName, item.PriceMember, item.PriceGuest));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { message = $"Se han procesado {command.Products.Count} productos correctamente." });
        }
    }
}