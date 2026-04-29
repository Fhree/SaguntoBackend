using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record CreateNewOrderCommand(decimal Total, bool IsPaid, int UserId, int? CustomerId, Dictionary<int, int> Products);

    public static class CreateNewOrderCommandHandler
    {
        [WolverinePost("/api/orders")]
        public static async Task<CreationResponse> Handle(CreateNewOrderCommand command, ISaguntoDbContext dbContext)
        {
            var order = new Order(command.Total, command.IsPaid, command.UserId, command.CustomerId);

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            var productsInLines = await dbContext.Products.AsNoTracking()
                .Where(p => command.Products.Keys.Contains(p.Id))
                .ToListAsync();

            productsInLines.ForEach(p =>
            {
                var price = order.CustomerId.HasValue ? p.PriceMember : p.PriceGuest;
                order.AddLine(new OrderLine(order.Id, p.Id, command.Products[p.Id], price));
            });

            await dbContext.SaveChangesAsync();

            return new CreationResponse($"/api/orders/{order.Id}");
        }
    }
}
