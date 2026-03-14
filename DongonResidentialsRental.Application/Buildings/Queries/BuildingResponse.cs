using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries
{
    public sealed record BuildingResponse(
        Guid BuildingId, 
        string Name, 
        string AddressStreet, 
        string AddressCity, 
        string AddressProvince, 
        string AddressPostalCode);
}
