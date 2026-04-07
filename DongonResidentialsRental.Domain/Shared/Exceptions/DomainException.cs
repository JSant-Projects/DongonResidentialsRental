using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared.Exceptions;

public class DomainException(string message) : Exception(message)
{
}


