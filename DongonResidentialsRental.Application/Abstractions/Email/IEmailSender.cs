using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Email;
public interface IEmailSender
{
    Task SendAsync(
        EmailMessage emailMessage,
        CancellationToken cancellationToken = default);
}
