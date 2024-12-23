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

    public interface IJoinTable : ITable
    {
        string? LeftTableName { get; set; }

        Table? LeftTable { get; set; }

        JoinType JoinType { get; set; }

        JoinRelationship? Relationship { get; set; }

        RelationType RelationType { get; set; }

        int Depth { get; set; }

        IJoinTable SetLeftTableName(string leftTableName);

        IJoinTable SetLeftTable(Table? leftTable);

        IJoinTable SetJoinType(JoinType joinType);

        IJoinTable On(string leftTableColumnName, string rightTableColumnName);
        IJoinTable On(JoinRelationship joinRelationship);
        new IJoinTable SetMetaColumns(params IMetaColumn[] columns);

        IJoinTable SetDepth(int depth);

        IJoinTable SetRelationType(RelationType relationType);
    }
    public class JoinTable : Table, IJoinTable
    {
        public string? LeftTableName { get; set; } = null;

        public Table? LeftTable { get; set; } = null;

        public JoinType JoinType { get; set; } = JoinType.Inner;

        public JoinRelationship? Relationship { get; set; } = null;

        public RelationType RelationType { get; set; }

        public int Depth { get; set; }

        public JoinTable(string? name, string? alias, JoinType joinType) : base(name, alias)
        {
            SetJoinType(joinType);
        }
        public JoinTable(Table? leftTable, string? name, string? alias, JoinType joinType) : base(name, alias)
        {
            SetLeftTable(leftTable);
        }

        public IJoinTable On(string? leftTableColumnName, string? rightTableColumnName)
        {
            return On(new JoinRelationship(new MetaColumn(leftTableColumnName).SetTable(LeftTable), new MetaColumn(rightTableColumnName).SetTable(this)));
        }

        public IJoinTable On(JoinRelationship joinRelationship)
        {
            Relationship = joinRelationship;

            return this;
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

        public new IJoinTable SetMetaColumns(params IMetaColumn[] columns)
        {
            base.SetMetaColumns(columns);

            return this;
        }

        public IJoinTable SetDepth(int depth)
        {
            Depth = depth;

            return this;
        }

        public IJoinTable SetRelationType(RelationType relationType)
        {
            RelationType = relationType;

            return this;
        }
    }
}
