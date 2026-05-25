namespace Ecommerce.Application.DTOs.Orders;

public record PayOrderRequest(
    string HolderName,
    string Number,
    int ExpMonth,
    int ExpYear,
    string Cvv);
