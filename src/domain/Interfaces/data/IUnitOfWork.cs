namespace domain.Interfaces.Data;

public interface IUnitOfWork
{
    Task SaveAsync(CancellationToken cancellationToken);
}
