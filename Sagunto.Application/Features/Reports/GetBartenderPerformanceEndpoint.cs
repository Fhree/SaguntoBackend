using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Application.Features.Reports
{
    public record BartenderPerformanceDto(
        int BartenderId,
        string Name,
        int TotalConsumptions,
        decimal TotalRevenue,
        List<DailyPerformanceDto> DailyStats
    );

    public record DailyPerformanceDto(
        string Date,
        int Consumptions,
        decimal Revenue
    );

    public static class GetBartenderPerformanceEndpoint
    {
        [WolverineGet("/api/reports/bartenders")]
        [Tags("Reports")]
        [EndpointSummary("Obtiene el rendimiento y las ventas agrupadas de los 4 camareros")]
        public static async Task<List<BartenderPerformanceDto>> Handle(ISaguntoDbContext db, CancellationToken ct)
        {
            // 1. Filtramos solo los pedidos creados por los camareros (IDs 1 al 4)
            var rawOrders = await db.Orders
                .Include(o => o.User)
                    .ThenInclude(u => u.Role)
                .Where(o => o.User.RoleId == 3)
                .ToListAsync(ct);

            // 2. Agrupamos en memoria por camarero para montar el objeto complejo
            var performanceList = rawOrders
                .GroupBy(o => new { o.UserId, o.User.Name })
                .Select(g => new BartenderPerformanceDto(
                    BartenderId: g.Key.UserId,
                    Name: g.Key.Name,
                    TotalConsumptions: g.Count(),
                    TotalRevenue: g.Sum(x => x.Total),
                    DailyStats: g.GroupBy(x => x.CreatedAt.ToString("yyyy-MM-dd"))
                                 .Select(dayGroup => new DailyPerformanceDto(
                                     Date: dayGroup.Key,
                                     Consumptions: dayGroup.Count(),
                                     Revenue: dayGroup.Sum(x => x.Total)
                                 ))
                                 .OrderBy(d => d.Date)
                                 .ToList()
                ))
                .OrderBy(b => b.BartenderId)
                .ToList();

            return performanceList;
        }
    }
}