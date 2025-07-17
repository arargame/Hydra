﻿using Hydra.Core;
using Hydra.DBAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public interface ILogDbWriterService
    {
        Task<bool> SaveAsync(ILog log);
    }
    public class LogDbWriterService : ILogDbWriterService
    {
        //private readonly string _connectionString;
        //private readonly ConnectionType _connectionType;

        //private readonly ICustomConfigurationService _config;

        private readonly IDbConnection _connection;

        public LogDbWriterService(IDbConnection connection)
        {
            //_connectionString = connectionString;
            //_config = config;
            //_connectionType = connectionType;
            _connection = connection;
        }

        public async Task<bool> SaveAsync(ILog log)
        {
            try
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "@Id", log.Id },
                    { "@Name", log.Name },
                    { "@Description", log.Description },
                    { "@Category", log.Category },
                    { "@EntityId", log.EntityId },
                    { "@Type", log.Type.ToString() },
                    { "@ProcessType", log.ProcessType.ToString() },
                    { "@AddedDate", log.AddedDate },
                    { "@ModifiedDate", log.ModifiedDate },
                    { "@SessionInformationId", log.SessionInformationId }
                };

                var query = @"
                INSERT INTO Log 
                (Id, Name, Description, Category, EntityId, Type, ProcessType, AddedDate, ModifiedDate, SessionInformationId)
                VALUES 
                (@Id, @Name, @Description, @Category, @EntityId, @Type, @ProcessType, @AddedDate, @ModifiedDate, @SessionInformationId)";

                //var connectionString = _config.Get("LogDb");

                //var connection = ConnectionFactory.CreateConnection(_connectionType, connectionString);

                await Task.Run(() =>
                {
                    AdoNetDatabaseService.ExecuteNonQuery(query, parameters, _connection);
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogDbWriterService] Log DB kayıt hatası: {ex.Message}");
                return false;
            }
        }
    }

}
