using Hydra.DataModels.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public class JoinRelationship
    {
        public JoinRelationship? LeftColumnRelation { get; private set; } = null;

        public JoinRelationship? RightColumnRelation { get; private set; } = null;

        public FilterJoinType FilterJoinType { get; private set; }

        public IColumn LeftColumn { get; set; }

        public IColumn RightColumn { get; set; }

        public JoinRelationship(IColumn leftColumn, IColumn rightColumn)
        {
            LeftColumn = leftColumn;

            RightColumn = rightColumn;
        }

        public string PrepareQueryString()
        {
            if (RightColumnRelation != null)
                return $"{LeftColumn.Table?.Alias}.{LeftColumn.Name}={RightColumn.Table?.Alias}.{RightColumn.Name} {FilterJoinType} {RightColumnRelation.PrepareQueryString()}";
            else
                return $"{LeftColumn.Table?.Alias}.{LeftColumn.Name}={RightColumn.Table?.Alias}.{RightColumn.Name}";
        }

        public JoinRelationship SetRightRelation(JoinRelationship rightRelation, FilterJoinType filterJoinType)
        {
            RightColumnRelation = rightRelation;

            rightRelation.SetLeftRelation(this);

            FilterJoinType = filterJoinType;

            return this;
        }

        public JoinRelationship SetLeftRelation(JoinRelationship leftRelation)
        {
            LeftColumnRelation = leftRelation;

            return this;
        }
    }
}
