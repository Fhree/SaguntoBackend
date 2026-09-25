using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Backend.Features.Reports
{
    // DTOs atómicos para que Angular reciba los números limpios
    public record SegmentDto(int Quantity, decimal Revenue);
    public record DailyProductSaleDto(string FestiveBlock, SegmentDto Saguntinos, SegmentDto Guests);
    public record ProductSalesDto(string ProductName, SegmentDto TotalSaguntinos, SegmentDto TotalGuests, decimal AbsoluteTotal, List<DailyProductSaleDto> DailySales);

    public static class GetProductSalesEndpoint
    {
        [WolverineGet("/api/reports/products/sales")]
        [Tags("Reports")]
        [EndpointSummary("Devuelve ventas por producto segmentadas aplicando proyección optimizada de BD")]
        public static async Task<IReadOnlyList<ProductSalesDto>> Handle(ISaguntoDbContext db, CancellationToken ct)
        {
            var rawItems = await db.Orders
                .SelectMany(o => o.Lines.Select(i => new
                {
                    i.Product.Name,
                    i.Quantity,
                    i.PriceSnapshot,
                    IsSaguntino = o.Customer != null && o.Customer.Id > 0,
                    OrderDate = o.CreatedAt
                }))
                .ToListAsync(ct);

            var startDay = new DateTime(2026, 9, 17);

            var groupedProducts = rawItems
                .GroupBy(x => x.Name)
                .Select(g =>
                {
                    var dailySales = new List<DailyProductSaleDto>();

                    for (int i = 0; i < 11; i++)
                    {
                        var targetDate = startDay.AddDays(i).Date;

                        var itemsForDay = g.Where(x => x.OrderDate.AddHours(-14).Date == targetDate).ToList();

                        var saguntinos = itemsForDay.Where(x => x.IsSaguntino).ToList();
                        var guests = itemsForDay.Where(x => !x.IsSaguntino).ToList();

                        dailySales.Add(new DailyProductSaleDto(
                            FestiveBlock: $"Día {i + 1}",
                            Saguntinos: new SegmentDto(
                                Quantity: saguntinos.Sum(x => x.Quantity),
                                Revenue: saguntinos.Sum(x => (decimal)x.Quantity * x.PriceSnapshot)
                            ),
                            Guests: new SegmentDto(
                                Quantity: guests.Sum(x => x.Quantity),
                                Revenue: guests.Sum(x => (decimal)x.Quantity * x.PriceSnapshot)
                            )
                        ));
                    }

                    var allSaguntinos = g.Where(x => x.IsSaguntino).ToList();
                    var allGuests = g.Where(x => !x.IsSaguntino).ToList();

                    return new ProductSalesDto(
                        ProductName: g.Key,
                        TotalSaguntinos: new SegmentDto(
                            Quantity: allSaguntinos.Sum(x => x.Quantity),
                            Revenue: allSaguntinos.Sum(x => (decimal)x.Quantity * x.PriceSnapshot)
                        ),
                        TotalGuests: new SegmentDto(
                            Quantity: allGuests.Sum(x => x.Quantity),
                            Revenue: allGuests.Sum(x => (decimal)x.Quantity * x.PriceSnapshot)
                        ),
                        AbsoluteTotal: g.Sum(x => (decimal)x.Quantity * x.PriceSnapshot),
                        DailySales: dailySales
                    );
                })
                .OrderByDescending(p => p.AbsoluteTotal)
                .ToList();

            return groupedProducts;
        }
    }
}