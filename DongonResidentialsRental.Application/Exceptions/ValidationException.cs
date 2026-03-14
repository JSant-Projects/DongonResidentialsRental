using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Exceptions;

public sealed class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message)
    {
    }
}
