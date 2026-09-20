using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        [EndpointSummary("Exporta a CSV el consumo pivotado de saguntinos con ratio diario de comandas")]
        public static async Task<IResult> Handle(ISaguntoDbContext db, CancellationToken ct)
        {
            var orders = await db.Orders
                .Include(o => o.Customer)
                .Where(o => o.Customer != null && o.Customer.Id > 0)
                .ToListAsync(ct);

            // Mantenemos la estructura pivot plana por usuario
            var consumptions = orders
                .GroupBy(o => new { o.Customer.Id, o.Customer.Name, o.Customer.Surname })
                .Select(g => new
                {
                    FullName = (g.Key.Name + " " + g.Key.Surname).Trim(),
                    TotalAbsolute = g.Sum(o => o.Total),
                    Orders = g.ToList()
                })
                .OrderBy(c => c.FullName)
                .ToList();

            var builder = new StringBuilder();
            var cultureEs = new CultureInfo("es-ES");

            // 1 Construimos la cabecera dinámica con duplas por día
            var header = new StringBuilder("Nombre + Apellidos;Total Absoluto");

            var startDay = new DateTime(2026, 9, 17);

            for (int i = 0; i < 11; i++)
            {
                var currentDay = startDay.AddDays(i);
                var nextDay = currentDay.AddDays(1);
                var timeBlock = $"{currentDay:dd/MM/yyyy} 14:00 - {nextDay:dd/MM/yyyy} 13:59";

                // Añadimos la dupla de columnas para este día garantizando nombres únicos
                header.Append($";Total {timeBlock};Pagado (Si/No) {timeBlock}");
            }
            builder.AppendLine(header.ToString());

            // 2 Rellenamos las filas usuario a usuario
            foreach (var item in consumptions)
            {
                var safeFullName = item.FullName.Replace("\"", "\"\"");
                var totalAbsFormatted = item.TotalAbsolute.ToString("F2", cultureEs);

                var row = new StringBuilder($"\"{safeFullName}\";{totalAbsFormatted}");

                // 3 Procesamos los 11 cubos de tiempo de forma individual
                for (int i = 0; i < 11; i++)
                {
                    var targetDate = startDay.AddDays(i).Date;

                    // Filtramos las comandas exclusivas de esta jornada aplicando el desfase horario
                    var dailyOrders = item.Orders
                        .Where(o => o.CreatedAt.AddHours(-14).Date == targetDate)
                        .ToList();

                    var dailyTotal = dailyOrders.Sum(o => o.Total).ToString("F2", cultureEs);
                    var paidCount = dailyOrders.Count(o => o.IsPaid);
                    var unpaidCount = dailyOrders.Count(o => !o.IsPaid);

                    // Formateamos el ratio diario tal y como lo pidió el cliente
                    var paymentRatioText = $"{paidCount}/{unpaidCount}";

                    // Añadimos los datos a la dupla de columnas
                    row.Append($";{dailyTotal};{paymentRatioText}");
                }

                builder.AppendLine(row.ToString());
            }

            var bom = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var fileBytes = bom.Concat(contentBytes).ToArray();

            var fileName = $"consumo_diario_pivotado_sagunto_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";

            return Results.File(
                fileContents: fileBytes,
                contentType: "text/csv; charset=utf-8",
                fileDownloadName: fileName
            );
        }
    }
}