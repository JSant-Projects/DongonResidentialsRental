namespace DongonResidentialsRental.Application.Abstractions.Email;

public sealed record EmailMessage(
    IReadOnlyList<string> To,
    string Subject,
    string Body,
    IReadOnlyList<string>? Cc = null,
    IReadOnlyList<string>? Bcc = null,
    EmailContentType ContentType = EmailContentType.PlainText
);
