using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Products
{

    public record ProductDto(int Id, string Name, decimal PriceMember, decimal PriceGuest);


    public static class GetAllProductsQueryHandler
    {
        [WolverineGet("/api/products")]
        [Tags("Products")]
        [EndpointSummary("Get all products")]
        public static async Task<List<ProductDto>> Handle(ISaguntoDbContext dbContext)
        {
            var products = await dbContext.Products.AsNoTracking().ToListAsync();
            return products.Select(p => new ProductDto(p.Id, p.Name, p.PriceMember, p.PriceGuest)).ToList();
        }
    }
}
