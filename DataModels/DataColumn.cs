using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public interface IDataColumn : IColumn
    {
        IRow? Row { get; set; }

        IColumn SetRow(IRow row);
    }
    public class DataColumn : Column, IDataColumn
    {
        public DataColumn() { }

        public DataColumn(string name, object value)
        {
            SetName(name);

            SetValue(value);
        }
    }
}
