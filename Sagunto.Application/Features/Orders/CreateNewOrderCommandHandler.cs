using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record CreateNewOrderCommand(Guid Id, bool IsPaid, int UserId, int? CustomerId, List<OrderLineCommand> Products);
    public record OrderLineCommand(int ProductId, int Quantity, decimal PriceSnapshot);

    public static class CreateNewOrderCommandHandler
    {
        [WolverinePost("/api/orders")]
        [Tags("Orders")]
        [EndpointSummary("Create new order (Idempotent)")]
        public static async Task<IResult> Handle(CreateNewOrderCommand command, ISaguntoDbContext dbContext)
        {
            // Idempotencia: Verificamos directamente contra la PK
            var existingOrder = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == command.Id);

            if (existingOrder != null)
            {
                return Results.Ok(new { Message = "Order already processed", OrderId = existingOrder.Id });
            }

            var order = new Order(command.Id, command.IsPaid, command.UserId, command.CustomerId);

            var productIds = command.Products.Select(ol => ol.ProductId).ToList();
            var productsInLines = await dbContext.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id)).ToListAsync();

            var totalPrice = 0m;

            command.Products.ForEach(product =>
            {
                if (productsInLines.Any(p => p.Id == product.ProductId))
                {
                    totalPrice += (product.PriceSnapshot * product.Quantity);
                    order.AddLine(new OrderLine(product.ProductId, product.Quantity, product.PriceSnapshot));
                }
            });

            order.SetTotalPrice(totalPrice);
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/api/orders/{order.Id}", order.Id);
        }
    }
}