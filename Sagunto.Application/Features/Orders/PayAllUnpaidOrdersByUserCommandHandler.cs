using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Orders
{

    public class PayAllOrdersCommandHandler
    {
        // Mantenemos la ruta exacta a la que ataca tu app móvil
        [WolverinePost("/api/orders/{customerId}/payAll")]
        [Tags("Orders")]
        [EndpointSummary("Pay all unpaid orders for a customer")]
        public static async Task<IResult> Handle(
            int customerId,
            ISaguntoDbContext dbContext)
        {
            // Buscamos todas las comandas de este saguntino que sigan impagadas
            var orders = await dbContext.Orders
                .Where(o => o.CustomerId == customerId && !o.IsPaid)
                .ToListAsync();

            if (!orders.Any())
            {
                return Results.Ok(new
                {
                    Message = "El saguntino no tiene deudas pendientes o ya han sido liquidadas."
                });
            }

            orders.ForEach(order => order.Pay());
            await dbContext.SaveChangesAsync();

            return Results.Ok(new
            {
                Message = $"{orders.Count} consumición(es) liquidada(s) correctamente.",
                SettledOrderIds = orders.Select(o => o.Id).ToList()
            });
        }
    }
}