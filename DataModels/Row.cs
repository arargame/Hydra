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
        ITable? Table { get; set; }

        List<IDataColumn> Columns { get; set; }

        IRow AddColumn(IDataColumn column);

        IDataColumn? GetColumnByName(string columnName);
    }
    public class Row : BaseObject<Row>,IRow
    {
        public ITable? Table { get; set; } = null;

        public List<IDataColumn> Columns { get; set; } = new List<IDataColumn>();

        public Row() { }

        public IRow AddColumn(IDataColumn column)
        {
            if (Table != null && !Table.MetaColumns.Any(mc => mc.Name == column.Name))
                throw new InvalidOperationException($"Column '{column.Name}' does not exist in the table metadata.");

            Columns.Add(column);

            column.SetRow(this);

            return this;
        }

        public IDataColumn? GetColumnByName(string columnName)
        {
            return Columns.FirstOrDefault(c => c.Name == columnName);
        }
    }
}
