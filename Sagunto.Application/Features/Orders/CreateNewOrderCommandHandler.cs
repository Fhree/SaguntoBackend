using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record CreateNewOrderCommand(bool IsPaid, int UserId, int? CustomerId, List<OrderLineCommand> Products);
    public record OrderLineCommand(int ProductId, int Quantity, decimal PriceSnapshot);

    public static class CreateNewOrderCommandHandler
    {
        [WolverinePost("/api/orders")]
        [Tags("Orders")]
        [EndpointSummary("Create new order")]
        public static async Task<CreationResponse> Handle(CreateNewOrderCommand command, ISaguntoDbContext dbContext)
        {
            var order = new Order(command.IsPaid, command.UserId, command.CustomerId);

            dbContext.Orders.Add(order);

            var productIds = command.Products.Select(ol => ol.ProductId).ToList();


            var productsInLines = await dbContext.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var totalPrice = 0m;


            command.Products.ForEach(product =>
            {
                if (productsInLines.Any(p => p.Id == product.ProductId))
                {
                    totalPrice += (product.PriceSnapshot * product.Quantity);
                    order.AddLine(new OrderLine(order.Id, product.ProductId, product.Quantity, product.PriceSnapshot));
                }
            });
            

            order.SetTotalPrice(totalPrice);

            await dbContext.SaveChangesAsync();

            return new CreationResponse($"/api/orders/{order.Id}");
        }
    }
}