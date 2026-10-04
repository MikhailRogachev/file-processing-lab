namespace domain.Interfaces.data;

public interface IUnitOfWork
{
    Task SaveAsync(CancellationToken cancellationToken);
}
