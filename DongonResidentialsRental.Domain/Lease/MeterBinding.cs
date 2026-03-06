using DongonResidentialsRental.Domain.Meter;

namespace DongonResidentialsRental.Domain.Lease;

public sealed record MeterBinding(ElectricityMeterId? ElectricityMeterId, WaterMeterId? WaterMeterId);
