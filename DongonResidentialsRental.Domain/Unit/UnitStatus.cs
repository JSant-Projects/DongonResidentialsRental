namespace DongonResidentialsRental.Domain.Unit;

public enum UnitStatus
{
    Available,      // Can be leased
    Maintenance,    // Temporarily unavailable
    Inactive        // Permanently removed / not rentable
}
