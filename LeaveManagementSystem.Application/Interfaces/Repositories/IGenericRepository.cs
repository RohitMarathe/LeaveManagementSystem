namespace LeaveManagementSystem.Application.Interfaces.Repositories;

public interface IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}