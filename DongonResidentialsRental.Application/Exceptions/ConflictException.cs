using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Exceptions;

public sealed class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message)
    {
    }
}
