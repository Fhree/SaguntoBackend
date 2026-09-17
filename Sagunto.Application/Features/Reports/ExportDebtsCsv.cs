using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sagunto.Application.Interfaces;
using Wolverine.Http;

namespace Sagunto.Backend.Features.Reports
{

    public static class ExportDebtsCsvEndpoint
    {
        [WolverineGet("/api/reports/general-consumption/export-csv")]
        [Tags("Reports")]
        [EndpointSummary("Exporta a CSV el listado de saguntinos con comandas pendientes y su deuda")]
        public static async Task<IResult> Handle(ISaguntoDbContext db, CancellationToken ct)
        {
            var consumptions = await db.Orders
            .Where(o => o.User != null)
            .GroupBy(o => new { o.User.Id, o.User.Name, o.User.Surname })
            .Select(g => new
            {
                CustomerId = g.Key.Id,
                FullName = (g.Key.Name + " " + g.Key.Surname).Trim(),
                TotalConsumed = g.Sum(o => o.Total)
            })
            .OrderBy(c => c.CustomerId)
            .ToListAsync(ct);

            var builder = new StringBuilder();
            builder.AppendLine("ID;Saguntino;Total Consumido (€)");

            var cultureEs = new CultureInfo("es-ES");

            foreach (var item in consumptions)
            {
                // Mapeo visual de los IDs especiales para que el CSV sea legible
                var idDisplay = item.CustomerId switch
                {
                    -1 => "INV",
                    -2 => "BAR",
                    _ => item.CustomerId.ToString()
                };

                var nameDisplay = item.CustomerId switch
                {
                    < 0 => $"{item.FullName} (Anónimo)",
                    _ => item.FullName
                };

                var safeFullName = nameDisplay.Replace("\"", "\"\"");
                var formattedTotal = item.TotalConsumed.ToString("F2", cultureEs);

                builder.AppendLine($"{idDisplay};\"{safeFullName}\";{formattedTotal}");
            }

            var bom = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var fileBytes = bom.Concat(contentBytes).ToArray();

            var fileName = $"consumo_general_sagunto_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";

            return Results.File(
                fileContents: fileBytes,
                contentType: "text/csv; charset=utf-8",
                fileDownloadName: fileName
            );
        }
    }
}