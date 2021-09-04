namespace Services.Interfaces;

public interface IStandardDataAccess<TEntity>
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(string id);
    Task UpdateAsync(TEntity entity);
    Task<string> CreateAsync(TEntity entity);
    Task DeleteAsync(string id);
}