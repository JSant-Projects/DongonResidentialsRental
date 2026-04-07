using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared.Exceptions;

public class OperationNotAllowedException : DomainException
{
    public OperationNotAllowedException(string message) : base(message)
    {
    }
}
