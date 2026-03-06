using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public sealed class DomainException(string message) : Exception(message)
{
}
