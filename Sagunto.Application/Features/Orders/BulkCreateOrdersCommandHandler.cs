using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Sagunto.Domain.Entities;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record BulkCreateOrdersCommand(List<CreateNewOrderCommand> Orders);
    public record BulkCreationResponse(int InsertedOrders, int SkippedOrders);

    public static class BulkCreateOrdersCommandHandler
    {
        [WolverinePost("/api/orders/bulk")]
        [Tags("Orders")]
        [EndpointSummary("Bulk create orders from offline sync")]
        public static async Task<BulkCreationResponse> Handle(BulkCreateOrdersCommand command, ISaguntoDbContext dbContext)
        {
            if (command.Orders == null || !command.Orders.Any())
                return new BulkCreationResponse(0, 0);

            var incomingIds = command.Orders.Select(o => o.Id).ToList();

            // Idempotencia masiva contra la PK
            var existingIds = await dbContext.Orders
                .Where(o => incomingIds.Contains(o.Id))
                .Select(o => o.Id)
                .ToListAsync();

            var newOrdersCmds = command.Orders.Where(o => !existingIds.Contains(o.Id)).ToList();

            if (!newOrdersCmds.Any())
                return new BulkCreationResponse(0, existingIds.Count);

            var allRequiredProductIds = newOrdersCmds.SelectMany(o => o.Products).Select(p => p.ProductId).Distinct().ToList();
            var productsInDb = await dbContext.Products
                .AsNoTracking()
                .Where(p => allRequiredProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var ordersToInsert = new List<Order>();

            foreach (var cmd in newOrdersCmds)
            {
                var order = new Order(cmd.Id, cmd.IsPaid, cmd.UserId, cmd.CustomerId);
                decimal total = 0;

                foreach (var pCmd in cmd.Products)
                {
                    if (productsInDb.ContainsKey(pCmd.ProductId))
                    {
                        total += (pCmd.PriceSnapshot * pCmd.Quantity);
                        order.AddLine(new OrderLine(pCmd.ProductId, pCmd.Quantity, pCmd.PriceSnapshot));
                    }
                }

                order.SetTotalPrice(total);
                ordersToInsert.Add(order);
            }

            dbContext.Orders.AddRange(ordersToInsert);
            await dbContext.SaveChangesAsync();

            return new BulkCreationResponse(ordersToInsert.Count, existingIds.Count);
        }
    }
}