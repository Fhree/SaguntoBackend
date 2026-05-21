using Microsoft.AspNetCore.Http;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Products
{
    public record CreateNewProductCommand(string Name, decimal PriceMember, decimal PriceGuest);

    public record CreateNewProductResponse(int UserId);

    public static class CreateNewProductCommandHandler
    {
        [WolverinePost("/api/products")]
        [Tags("Products")]
        [EndpointSummary("Create a new product")]
        public static async Task<(CreationResponse, CreateNewProductResponse)> Handle(CreateNewProductCommand command, ISaguntoDbContext dbContext)
        {
            var product = new Product(command.Name, command.PriceMember, command.PriceGuest);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return (
                new CreationResponse($"/api/products/{product.Id}"),
                new CreateNewProductResponse(product.Id)
            );
        }
    }
}
