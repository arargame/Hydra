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

    /// <summary>
    /// Bir ITable'ı çalıştırıp satırlarını dolduran servis.
    ///
    /// Her tablo aynı veritabanında olmak zorunda değildir: bazı tablolar (ör. Log) ayrı bir
    /// veritabanında yaşar. Bu yüzden bağlantı, tablo adına göre çözülebilir. Eşleme
    /// konfigürasyondan okunur:
    ///
    ///     "Hydra": { "TableConnections": { "Log": "LogDbConnection" } }
    ///
    /// Eşleme yoksa varsayılan bağlantı kullanılır — yani mevcut davranış aynen korunur.
    /// </summary>
    public class TableService : ITableService
    {
        private readonly IDbConnection _defaultConnection;

        //İsimden bağlantı üreten fabrika (ör. "LogDbConnection" → MsSqlConnection). Null ise
        //tablo bazında yönlendirme kapalıdır.
        private readonly Func<string, IDbConnection>? _connectionByName;

        //Tablo adı → bağlantı adı çözümleyici. Null/boş dönerse varsayılan bağlantı kullanılır.
        private readonly Func<string, string?>? _connectionNameForTable;

        /// <summary>Tek bağlantılı (klasik) kurulum.</summary>
        public TableService(IDbConnection connection)
        {
            _defaultConnection = connection;
        }

        /// <summary>Tablo bazında bağlantı yönlendirmesi destekleyen kurulum.</summary>
        public TableService(IDbConnection defaultConnection,
                            Func<string, IDbConnection> connectionByName,
                            Func<string, string?> connectionNameForTable)
        {
            _defaultConnection = defaultConnection;
            _connectionByName = connectionByName;
            _connectionNameForTable = connectionNameForTable;
        }

        public async Task<ITable> GetTableAsync(TableDTO tableDTO)
        {
            var table = TableDTO.ConvertToTable(tableDTO);

            return await GetTableAsync(table);
        }

        public async Task<ITable> GetTableAsync(ITable table)
        {
            _ = new QueryBuilder(table, ResolveConnection(table.Name))
                               .BuildSelectQuery()
                               .SetTableRows();

            return await Task.FromResult(table);
        }

        /// <summary>
        /// Tablonun hangi bağlantı üzerinden sorgulanacağını belirler.
        /// Yönlendirme kapalıysa, tablo adı boşsa ya da eşleme tanımlı değilse varsayılana düşer.
        /// Bağlantı üretimi sırasında bir hata olursa da varsayılana düşülür — yanlış bir
        /// konfigürasyon yüzünden bütün sorgular çalışmaz hâle gelmesin.
        /// </summary>
        private IDbConnection ResolveConnection(string? tableName)
        {
            if (_connectionByName == null || _connectionNameForTable == null || string.IsNullOrWhiteSpace(tableName))
                return _defaultConnection;

            try
            {
                var connectionName = _connectionNameForTable(tableName!);

                if (string.IsNullOrWhiteSpace(connectionName))
                    return _defaultConnection;

                return _connectionByName(connectionName!) ?? _defaultConnection;
            }
            catch
            {
                return _defaultConnection;
            }
        }
    }
}
