using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Products
{
    public record UpdateProductRoleCommand(int Id, decimal PriceMember, decimal  PriceGuest);

    public static class UpdateProductPricesCommandHandler
    {
        [WolverinePut("api/products/{id}/updatePrices")]
        [Tags("Products")]
        [EndpointSummary("Update product prices")]
        public static async Task<AcceptResponse?> Handle(UpdateProductRoleCommand command, ISaguntoDbContext dbContext)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.Id);

            if (product == null)
                return null;

            product.UpdatePrices(command.PriceMember, command.PriceGuest);

            return new AcceptResponse($"api/products/{command.Id}/updatePrices");
        }
    }
}
