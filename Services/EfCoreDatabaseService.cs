using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public class EfCoreDatabaseService
    {
        private readonly DbContext _context;

        public EfCoreDatabaseService(DbContext context)
        {
            _context = context;
        }

        public async Task<List<T>> QueryAsync<T>(FormattableString sql) where T : class
        {
            return await _context.Set<T>()
                                 .FromSqlInterpolated(sql)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<T?> QuerySingleAsync<T>(FormattableString sql) where T : class
        {
            return await _context.Set<T>()
                                 .FromSqlInterpolated(sql)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();
        }

        public async Task<int> ExecuteAsync(FormattableString sql)
        {
            return await _context.Database.ExecuteSqlInterpolatedAsync(sql);
        }
    }
}
