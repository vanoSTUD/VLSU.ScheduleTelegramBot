namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Repositories;
public interface IBaseRepository<TEntity> where TEntity : class
{
    public IQueryable<TEntity> GetAll();
    public Task<TEntity> UpdateAsync(TEntity entity);
    public Task<TEntity> RemoveAsync(TEntity entity);
    public Task<TEntity> CreateAsync(TEntity entity);
}