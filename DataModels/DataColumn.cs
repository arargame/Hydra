using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public interface IDataColumn : IColumn
    {
        object? Value { get; set; }
        IRow? Row { get; set; }

        IColumn SetRow(IRow row);

        IColumn SetValue(object? value);
    }
    public class DataColumn : Column, IDataColumn
    {
        public object? Value { get; set; } = null;

        public DataColumn() { }

        public DataColumn(string name, object? value)
        {
            SetName(name);

            SetValue(value);
        }

        public IColumn SetValue(object? value)
        {
            Value = value;

            return this;
        }
    }
}
