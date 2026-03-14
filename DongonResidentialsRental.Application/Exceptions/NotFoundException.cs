using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Exceptions;

public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"{name} with id '{key}' was not found.")
    {
    }
}
