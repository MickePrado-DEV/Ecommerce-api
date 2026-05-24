using Ecommerce.Application.Abstractions;
using Ecommerce.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Ecommerce.Infrastructure.Documents;

public class PdfTicketGenerator : IPdfTicketGenerator
{
    static PdfTicketGenerator() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] GenerateDispatchTicket(Shipment shipment)
    {
        var order = shipment.Order;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Header().Text("Ticket de despacho").FontSize(20).Bold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Ticket: {shipment.Ticket?.TicketNumber}");
                    col.Item().Text($"Orden: {order.OrderNumber}");
                    col.Item().Text($"Cliente: {order.Address?.FullName}");
                    col.Item().Text($"Tracking: {shipment.TrackingNumber ?? "N/A"}");
                    col.Item().PaddingTop(10).Text("Artículos:");
                    foreach (var item in order.Items)
                        col.Item().Text($"- {item.ProductName} x{item.Quantity} ({item.Sku})");
                });
            });
        }).GeneratePdf();
    }
}
