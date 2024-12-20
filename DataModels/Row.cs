using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public interface IRow : IBaseObject<Row>
    {
        object? PrimaryKey { get; set; }
        ITable? Table { get; set; }

        List<IDataColumn> Columns { get; set; }

        IRow AddColumn(IDataColumn column);

        IDataColumn? GetColumnByName(string columnName);

        IRow SetTable(ITable table);

        IRow SetPrimaryKey(object? primaryKey);
    }
    public class Row : BaseObject<Row>,IRow
    {
        public object? PrimaryKey { get; set; } = null;

        public ITable? Table { get; set; } = null;

        public List<IDataColumn> Columns { get; set; } = new List<IDataColumn>();

        public Row() { }

        public IRow AddColumn(IDataColumn column)
        {
            Columns.Add(column);

            column.SetRow(this);

            return this;
        }

        public IDataColumn? GetColumnByName(string columnName)
        {
            return Columns.FirstOrDefault(c => c.Name == columnName);
        }

        public IRow SetTable(ITable table)
        {
            Table = table;

            return this;
        }

        public IRow SetPrimaryKey(object? primaryKey)
        {
            PrimaryKey = primaryKey;

            return this;
        }
    }
}
