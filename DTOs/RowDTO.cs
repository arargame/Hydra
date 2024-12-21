using Hydra.DataModels;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public object GetValueByColumnName(string columnName)
        {
            return Columns.FirstOrDefault(c => c.Name == columnName).Value;
        }

        public static RowDTO ConvertToRowDTO(Row row)
        {
            var rowDTO = new RowDTO()
            {
                Id = row.Id,
                TableName = row.Table?.Name,
                Columns = row.Columns.Select(c => DataColumnDTO.ConvertToDataColumnDTO(c)).ToList()
            };

            return rowDTO;
        }

        public static Row ConvertToRow(RowDTO rowDTO)
        {
            var row = new Row()
            {
                Id = rowDTO.Id,
                Columns = rowDTO.Columns.Select(c => DataColumnDTO.ConvertToDataColumn(c)).ToList()
            };

            return row;
        }

        public T ToObject<T>() where T : class
        {
            var instance = ReflectionHelper.CreateInstance<T>();

            foreach (var column in Columns)
            {
                try
                {
                    column.Value = column.Value?.ToString();

                    ReflectionHelper.SetValueOf(obj: instance, propertyName: column.Name, value: column.Value, logAction: (ex) =>
                    {

                    });
                }
                catch (Exception ex)
                {

                }
            }

            return instance;
        }

        public async Task<object> ToObject(Type genericType)
        {
            return await Task.Run(() =>
            {
                var obj = Helper.InvokeMethod(invokerType: typeof(RowDTO),
                                            invokerObject: this,
                                            methodName: "ToObject",
                                            genericTypes: new[]
                                            {
                                            genericType
                                            },
                                            logAction: (ex) =>
                                            {

                                            });

                Helper.SetValueOf(obj, "Id", this.Id, (ex) =>
                {

                });

                return obj;
            });
        }

        public RowDTO SetId(Guid id)
        {
            Id = id;

            return this;
        }
    }
}
