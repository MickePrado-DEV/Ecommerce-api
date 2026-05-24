using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions;

public interface IPdfTicketGenerator
{
    byte[] GenerateDispatchTicket(Shipment shipment);
}
