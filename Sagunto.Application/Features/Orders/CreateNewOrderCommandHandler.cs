using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record CreateNewOrderCommand(bool IsPaid, int UserId, int? CustomerId, Dictionary<int, int> Products);

    public static class CreateNewOrderCommandHandler
    {
        [WolverinePost("/api/orders")]
        [Tags("Orders")]
        [EndpointSummary("Create new order")]
        public static async Task<CreationResponse> Handle(CreateNewOrderCommand command, ISaguntoDbContext dbContext)
        {
            var order = new Order(command.IsPaid, command.UserId, command.CustomerId);

            dbContext.Orders.Add(order);
            
            var productsInLines = await dbContext.Products.AsNoTracking()
                .Where(p => command.Products.Keys.Contains(p.Id))
                .ToListAsync();

            var totalPrice = 0m;
            productsInLines.ForEach(p =>
            {
                var price = order.CustomerId.HasValue ? p.PriceMember : p.PriceGuest;
                var quantity = command.Products[p.Id];

                totalPrice += (price * quantity);
                order.AddLine(new OrderLine(order.Id, p.Id, quantity, price));
            });

            order.SetTotalPrice(totalPrice);

            await dbContext.SaveChangesAsync();

            return new CreationResponse($"/api/orders/{order.Id}");
        }
    }
}
