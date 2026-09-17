using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Reports;

public record OrderKpisDto(
    decimal TotalPendingDebt,
    decimal TotalImmediatePayments,
    decimal TotalGuests,
    decimal TotalAbsolute,
    int PendingDebtCount,
    int ImmediatePaymentsCount,
    int GuestsCount,
    int TotalOrdersCount
);

public static class GetOrderKpisEndpoint
{
    [WolverineGet("/api/reports/kpis")]
    [Tags("Reports")]
    [EndpointSummary("Obtiene los KPIs generales para el dashboard superior")]
    public static async Task<OrderKpisDto> Handle(ISaguntoDbContext db, CancellationToken ct)
    {
        // 1. Deuda pendiente de saguntinos (IDs > 0 y cuenta sin pagar)
        var pendingSaguntinos = await db.Orders
            .Where(o => o.CustomerId != null && o.CustomerId > 0 && !o.IsPaid)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { Total = g.Sum(x => x.Total) })
            .ToListAsync(ct);

        var totalPendingDebt = pendingSaguntinos.Sum(x => x.Total);
        var pendingDebtCount = pendingSaguntinos.Count;

        // 2. Pagos en el acto / Barra (ID == -2)
        // Agrupamos por una constante para sacar Sum y Count en una sola query optimizada
        var barOrdersInfo = await db.Orders
            .Where(o => o.CustomerId != null && o.CustomerId == -2)
            .GroupBy(o => 1)
            .Select(g => new {
                Total = g.Sum(x => x.Total),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        // 3. Consumo de invitados (ID == -1)
        var guestOrdersInfo = await db.Orders
            .Where(o => o.CustomerId != null && o.CustomerId == -1)
            .GroupBy(o => 1)
            .Select(g => new {
                Total = g.Sum(x => x.Total),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        // 4. Consumo total absoluto (suma de todos los pedidos)
        var totalAbsolute = await db.Orders.SumAsync(o => o.Total, ct);

        // 5. Total de pedidos (suma de todos los pedidos)
        var totalOrdersCount = await db.Orders.CountAsync(ct);

        return new OrderKpisDto(
            totalPendingDebt,
            barOrdersInfo?.Total ?? 0,
            guestOrdersInfo?.Total ?? 0,
            totalAbsolute,
            pendingDebtCount,
            barOrdersInfo?.Count ?? 0,
            guestOrdersInfo?.Count ?? 0,
            totalOrdersCount
        );
    }
}