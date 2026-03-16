namespace DongonResidentialsRental.Domain.Unit;

public enum UnitStatus
{
    Active,      // Can be leased
    Maintenance,    // Temporarily unavailable
    Inactive        // Permanently removed / not rentable
}
