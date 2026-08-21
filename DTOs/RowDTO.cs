using Hydra.Core;
using Hydra.DataModels;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class RowDTO
    {
        public Guid Id { get; set; }

        public string TableName { get; set; }

        public List<DataColumnDTO> Columns { get; set; }

        public RowDTO()
        {
            Columns = new List<DataColumnDTO>();
        }

        public RowDTO SetTableName(string tableName)
        {
            TableName = tableName;

            return this;
        }

        public RowDTO SetColumns(List<DataColumnDTO> columns)
        {
            Columns = columns;

            Columns.ForEach(c => c.SetRowDTO(this));

            return this;
        }

        public object? GetValueByColumnName(string columnName)
        {
            return Columns?.FirstOrDefault(c => c.Name == columnName)?.Value;
        }

        public static RowDTO ConvertToRowDTO(IRow row)
        {
            //Use the real entity id (primary key from database or the selected "Id" column).
            //row.Id is a randomly generated Guid of the Row object itself and must not leak to clients.
            var idValue = row.PrimaryKey ?? row.GetColumnByName(nameof(IHasId.Id))?.Value;

            var id = Guid.TryParse(idValue?.ToString(), out var parsedId) ? parsedId : row.Id;

            var rowDTO = new RowDTO()
            {
                Id = id,
                TableName = row.Table?.Name,
                Columns = row.Columns.Select(c => DataColumnDTO.ConvertToDataColumnDTO(c)).ToList()
            };

            return rowDTO;
        }

        public static Row ConvertToRow(RowDTO? rowDTO)
        {
            var row = new Row()
            {
                Id = rowDTO.Id,
                Columns = rowDTO.Columns.Select(c => DataColumnDTO.ConvertToDataColumn(c)).ToList()
            };

            return row;
        }


        public object? ToObject(Assembly? assembly,string? typeName, Action<Exception>? logAction = null)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                            return null;

            var type = assembly!=null ? ReflectionHelper.GetTypeFromAssembly(assembly,typeName) : ReflectionHelper.GetTypeFromAssembly(typeof(BaseObject<>), typeName);

            if (type == null)
                return null;

            return ToObject(type, logAction);
        }

        public T? ToObject<T>(Action<Exception>? logAction = null) where T : class
        {
            var instance = ReflectionHelper.CreateInstance<T>();

            foreach (var column in Columns)
            {
                column.Value = column.Value?.ToString();

                ReflectionHelper.SetValueOf(obj: instance, propertyName: column.Name, value: column.Value, logAction: logAction);
            }

            if(ReflectionHelper.HasProperty(type : typeof(T), propertyName: nameof(IHasId.Id)))
            {
                ReflectionHelper.SetValueOf(obj: instance,
                                            propertyName: nameof(IHasId.Id),
                                            value: Id,
                                            logAction: logAction);
            }


            return instance;
        }

        public object? ToObject(Type type, Action<Exception>? logAction = null)
        {
            return ToObject(assembly:type.Assembly,typeName: type.FullName!, logAction: logAction);
        }

        public Task<object?> ToObjectAsync(Assembly? assembly, string? typeName, Action<Exception>? logAction = null, CancellationToken ct = default)
            => Task.Run(() => ToObject(assembly, typeName, logAction), ct);

        public Task<object?> ToObjectAsync(Type type, Action<Exception>? logAction = null, CancellationToken ct = default)
            => Task.Run(() => ToObject(type, logAction), ct);

        public Task<T?> ToObjectAsync<T>(Action<Exception>? logAction = null, CancellationToken ct = default) where T : class
            => Task.Run(() => ToObject<T>(logAction), ct);

        //public async Task<object> ToObject(Type genericType)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var obj = ReflectionHelper.InvokeMethod(invokerType: typeof(RowDTO),
        //                                    invokerObject: this,
        //                                    methodName: "ToObject",
        //                                    genericTypes: new[]
        //                                    {
        //                                    genericType
        //                                    },
        //                                    logAction: (ex) =>
        //                                    {

        //                                    });

        //        ReflectionHelper.SetValueOf(obj, "Id", this.Id, (ex) =>
        //        {

        //        });

        //        return obj;
        //    });
        //}

        public RowDTO SetId(Guid id)
        {
            Id = id;

            return this;
        }
    }
}
