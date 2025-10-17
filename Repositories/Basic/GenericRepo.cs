using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Basic
{
    public class GenericRepo<T> where T : class
    {
        // Implement generic repository methods here
        protected EVBatteryTradingContext _context;

        public GenericRepo(EVBatteryTradingContext context)
        {
            _context = context;
        }

        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public void Create(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }

        public async Task<int> CreateAsync(T entity)
        {
            _context.Add(entity);
            return await _context.SaveChangesAsync();
        }
        public void Update(T entity)
        {
            //// Turning off Tracking for UpdateAsync in Entity Framework
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            //// Turning off Tracking for UpdateAsync in Entity Framework
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public bool Remove(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public T GetById(string code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(string code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        public async Task<T> GetByIdAsync(long code)
        {
            return await _context.Set<T>().FindAsync(code);
        }
        /*
        https://guidgenerator.com/
        daacb4fb-ff73-46ef-98f1-4af9aab2a30a
         */
        public T GetById(Guid code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(Guid code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        #region Separating asigned entity and save operators        

        public void PrepareCreate(T entity)
        {
            _context.Add(entity);
        }

        public void PrepareUpdate(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
        }

        public void PrepareRemove(T entity)
        {
            _context.Remove(entity);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        //Dùng cho Login check user có tồn tại hay không
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true)
        {
            var query = _context.Set<T>().AsQueryable();
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(predicate);
        }

        //Hàm viết thêm
        public DbSet<T> Set<T>()
        {
            return _db.Set<T>();
        }

        public async Task AddAsync<TEntity>(TEntity entity, CancellationToken ct = default)
        {
            await _db.Set<TEntity>().AddAsync(entity, ct);
        }

        public async Task<int> SaveAsync(CancellationToken ct = default)
        {
            return await _db.SaveChangesAsync(ct);
        }

        public IQueryable<TEntity> Query<TEntity>()
        {
            return _db.Set<TEntity>().AsQueryable();
        }

        public void Remove<TEntity>(TEntity entity)
        {
            _db.Set<TEntity>().Remove(entity);
        }
        #endregion Separating asign entity and save operators
    }
}
