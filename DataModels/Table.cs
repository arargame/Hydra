using Hydra.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public enum JoinType
    {
        Inner,
        Left,
        Right,
        Full
    }

    public interface ITable : IBaseObject<ITable>
    {
        string? Alias { get; set; }

        string? Query { get; set; }

        List<IMetaColumn> MetaColumns { get; set; }

        List<IRow> Rows { get; set; }

        ITable SetAlias(string? alias);

        ITable SetQuery(string query);
    }



    public class Table : BaseObject<Table>, ITable
    {
        public string? Alias { get; set; } = null;

        public string? Query { get; set; } = null;


        public List<IMetaColumn> MetaColumns { get; set; } = new List<IMetaColumn>();

        public List<IRow> Rows { get; set; } = new List<IRow>();

        public Table() { }

        public ITable SetAlias(string? alias)
        {
            if (!string.IsNullOrWhiteSpace(alias) && alias.Contains(' '))
                throw new ArgumentException("Alias cannot contain spaces.");

            Alias = alias;

            return this;
        }

        public ITable SetQuery(string query)
        {
            Query = query;

            return this;
        }
    }


}
