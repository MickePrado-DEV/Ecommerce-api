using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// Perfil de repartidor. Si tiene UserId, puede iniciar sesión en la app mobile repartidor.
/// Los creados solo desde admin pueden tener UserId null hasta que se vinculen.
/// </summary>
public class Driver : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string? LicenseNumber { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? StartLatitude { get; set; }
    public decimal? StartLongitude { get; set; }
    public int? Capacity { get; set; }

    public User? User { get; set; }
    public ICollection<Shipment> Shipments { get; set; } = [];
}
