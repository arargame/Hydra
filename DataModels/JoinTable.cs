using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public interface IJoinTable : ITable
    {
        string? LeftTableName { get; set; }

        Table? LeftTable { get; set; }

        JoinType JoinType { get; set; }

        IJoinTable SetLeftTableName(string leftTableName);

        IJoinTable SetLeftTable(Table? leftTable);

        IJoinTable SetJoinType(JoinType joinType);
    }
    public class JoinTable : Table, IJoinTable
    {
        public string? LeftTableName { get; set; } = null;

        public Table? LeftTable { get; set; } = null;

        public JoinType JoinType { get; set; } = JoinType.Inner;

        public JoinTable()
        {

        }

        public IJoinTable SetLeftTableName(string leftTableName)
        {
            LeftTableName = leftTableName;

            return this;
        }

        public IJoinTable SetLeftTable(Table? leftTable)
        {
            LeftTable = leftTable;

            return this;
        }

        public IJoinTable SetJoinType(JoinType joinType)
        {
            JoinType = joinType;

            return this;
        }
    }
}
