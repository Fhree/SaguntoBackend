using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record PayOrderCommand(int OrderId);
    public static class PayOrderCommandHandler
    {
        [WolverinePut("/api/orders/{OrderId}/pay")]
        public static async Task<AcceptResponse?> Handle(PayOrderCommand command, ISaguntoDbContext dbContext)
        {
            var order = await dbContext.Orders.FindAsync(command.OrderId);
            
            if (order == null)
                return null;

            order.Pay();
            await dbContext.SaveChangesAsync();
            return new AcceptResponse($"/api/orders/{command.OrderId}/pay");
        }
    }
}
