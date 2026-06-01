using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

internal static class ShipmentDriverQueries
{
    public static async Task<Driver> SaveDriverAsync(EcommerceDbContext db, Driver driver, CancellationToken ct = default)
    {
        if (driver.Id == Guid.Empty)
        {
            driver.Id = Guid.NewGuid();
            db.Drivers.Add(driver);
        }
        else
        {
            var tracked = await db.Drivers.FindAsync([driver.Id], ct);
            if (tracked is null)
            {
                db.Drivers.Add(driver);
            }
            else
            {
                tracked.Name = driver.Name;
                tracked.Phone = driver.Phone;
                tracked.Email = driver.Email;
                tracked.LicenseNumber = driver.LicenseNumber;
                tracked.VehicleType = driver.VehicleType;
                tracked.VehiclePlate = driver.VehiclePlate;
                tracked.Notes = driver.Notes;
                tracked.IsActive = driver.IsActive;
                driver = tracked;
            }
        }

        await db.SaveChangesAsync(ct);
        return driver;
    }
}
