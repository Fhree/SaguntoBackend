using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{
    public record CheckoutUserCommand(int CustomerId, List<Guid> OrderIds);

    public static class CheckoutUserCommandHandler
    {
        [WolverinePut("/api/orders/checkout")]
        [Tags("Orders")]
        [EndpointSummary("Checkout User bills")]
        public static async Task<AcceptResponse?> Handle(CheckoutUserCommand command, ISaguntoDbContext dbContext)
        {
            var billsToCheckout = await dbContext.Orders.Where(o => command.OrderIds.Contains(o.Id) && o.UserId == command.CustomerId).ToListAsync();

            billsToCheckout.ForEach(bill => bill.Pay());

            await dbContext.SaveChangesAsync();
            return new AcceptResponse($"/api/orders/{command.CustomerId}/checkout");
        }
    }
}
