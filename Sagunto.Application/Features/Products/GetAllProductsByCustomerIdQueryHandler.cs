using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Products
{
    public record ProductByCustomerIdResponse(int Id, string Name, decimal Price);

    public static class GetAllProductsByCustomerIdQueryHandler
    {
        [WolverineGet("/api/products/{isSaguntino}")]
        [Tags("Products")]
        [EndpointSummary("Get all products by type of customer")]
        public static async Task<List<ProductByCustomerIdResponse>> Handle(bool isSaguntino, ISaguntoDbContext dbContext)
        {
            var products = new List<ProductByCustomerIdResponse>();

            if (isSaguntino)
                products = await dbContext.Products.AsNoTracking()
                    .Select(p => new ProductByCustomerIdResponse(p.Id, p.Name, p.PriceMember))
                    .ToListAsync();
            else
                products = await dbContext.Products.AsNoTracking()
                        .Select(p => new ProductByCustomerIdResponse(p.Id, p.Name, p.PriceGuest))
                        .ToListAsync();


            return products;
        }
    }
}
