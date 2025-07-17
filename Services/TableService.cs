using Hydra.DataModels;
using Hydra.DBAccess;
using Hydra.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public interface ITableService
    {
        Task<ITable> GetTableAsync(TableDTO tableDTO);

        Task<ITable> GetTableAsync(ITable table);
    }
    public class TableService : ITableService
    {
        private readonly IDbConnection _connection;

        public TableService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ITable> GetTableAsync(TableDTO tableDTO)
        {
            var table = TableDTO.ConvertToTable(tableDTO);

            return await GetTableAsync(table);
        }

        public async Task<ITable> GetTableAsync(ITable table)
        {
            _ = new QueryBuilder(table, _connection)
                               .BuildSelectQuery()
                               .SetTableRows();

            return await Task.FromResult(table);
        }
    }
}
