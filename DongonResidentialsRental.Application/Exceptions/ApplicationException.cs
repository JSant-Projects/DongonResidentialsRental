using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Exceptions;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }
}
