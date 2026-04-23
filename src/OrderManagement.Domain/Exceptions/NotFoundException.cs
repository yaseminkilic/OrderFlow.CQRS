namespace OrderFlow.CQRS.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"'{entityName}' ({key}) bulunamadı.", "NOT_FOUND")
    {
    }
}
