using VLSU.ScheduleTelegramBot.Domain.Interfaces.Repositories;

namespace VLSU.ScheduleTelegramBot.DAL.Repositories;

internal class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _dbContext;

    public BaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(nameof(entity));

        await _dbContext.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }

    public IQueryable<TEntity> GetAll()
    {
        return _dbContext.Set<TEntity>();
    }

    public Task<TEntity> RemoveAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(nameof(entity));

        _dbContext.Update(entity);
        await _dbContext.SaveChangesAsync();

        return entity;
    }
}
