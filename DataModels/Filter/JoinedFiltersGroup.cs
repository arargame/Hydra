using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels.Filter
{
    public interface IJoinFilter : IQueryableFilter
    {
        IFilter LeftFilter { get; set; }

        IFilter RightFilter { get; set; }

        IQueryableFilter? AnotherFilter { get; set; }

        //IQueryableFilter? RootFilter { get; set; }

        FilterJoinType JoinTypeForAnother { get; set; }

        FilterJoinType JoinType { get; set; }

        //List<IFilterParameter> Parameters { get; set; }

        //IFilter SetRootFilter(IQueryableFilter filter);
    }

    public class JoinedFiltersGroup : QueryableFilter, IJoinFilter
    {
        public IFilter LeftFilter { get; set; }

        public IFilter RightFilter { get; set; }

        public IQueryableFilter? AnotherFilter { get; set; } = null;

        //public IQueryableFilter? RootFilter { get; set; } = null;

        public FilterJoinType JoinTypeForAnother { get; set; }

        public FilterJoinType JoinType { get; set; }

        //public int StartParameterIndex { get; set; }



        public int Priority { get; set; }

        public bool CreateFilterComponentFromThis { get; set; }

        public List<IFilterParameter> GetParameters
        {
            get
            {
                return Parameters;
            }
        }

        public JoinedFiltersGroup(IFilter leftFilter, IFilter rightFilter, FilterJoinType joinType) : this(leftFilter, rightFilter)
        {
            JoinType = joinType;
        }

        private JoinedFiltersGroup(IFilter leftFilter, IFilter rightFilter)
        {
            LeftFilter = leftFilter;

            RightFilter = rightFilter;

            Initialize();
        }


        public override void Initialize()
        {
            Parameters = new List<IFilterParameter>();

            Parameters.AddRange(LeftFilter.Parameters);

            Parameters.AddRange(RightFilter.Parameters);

            LeftFilter.SetStartParameterIndex(0);

            RightFilter.SetStartParameterIndex(LeftFilter.FinishParameterIndex);
        }

        //public void SetRootFiltersParameters(IFilter? root)
        //{
        //    if (root == null)
        //        return;

        //    root.GetParameters.AddRangeIfNotNull(AnotherFilter?.Parameters);

        //    SetRootFiltersParameters(root.RootFilter);
        //}

        public JoinedFiltersGroup Bind(IQueryableFilter anotherFilter, FilterJoinType joinTypeForAnother)
        {
            if (AnotherFilter != null)
                throw new Exception("The joined filters group does not support to bind more than 3 filters.3rd one is already defined.Please join a new JoinFilter");

            AnotherFilter = anotherFilter;

            JoinTypeForAnother = joinTypeForAnother;

            AnotherFilter.SetStartParameterIndex(RightFilter.FinishParameterIndex);

            AnotherFilter.SetRootFilter(this);

            Parameters.AddRange(AnotherFilter.Parameters);

            //SetRootFiltersParameters(RootFilter);

            if (AnotherFilter is JoinedFiltersGroup)
            {
                var anotherJoinedFiltersGroup = AnotherFilter as JoinedFiltersGroup;

                if(anotherJoinedFiltersGroup!=null)
                {
                    anotherJoinedFiltersGroup.LeftFilter.SetStartParameterIndex(anotherJoinedFiltersGroup.StartParameterIndex);

                    anotherJoinedFiltersGroup.RightFilter.SetStartParameterIndex(anotherJoinedFiltersGroup.LeftFilter.FinishParameterIndex);
                }
            }

            return this;
        }

        public override string PrepareQueryString()
        {
            var queryString = $"({LeftFilter?.PrepareQueryString()} {JoinType} {RightFilter?.PrepareQueryString()})";

            if (AnotherFilter != null)
                queryString += $" {JoinTypeForAnother} ({AnotherFilter.PrepareQueryString()})";

            return queryString;
        }

        public static List<JoinedFiltersGroup> SetFromColumns(List<IMetaColumn> filteredColumns)
        {
            var joinedFiltersGroupList = new List<JoinedFiltersGroup>();

            if (filteredColumns.Any())
            {
                var pagination = new Pagination(1, 2, filteredColumns.Count);

                for (int i = 0; i < pagination.TotalPagesCount; i++)
                {
                    pagination.SetPageNumber(i + 1);

                    var skip = pagination.Start > 0 ? pagination.Start - 1 : 0;

                    var filteredColumnList = filteredColumns.Skip(skip).Take(pagination.PageSize).ToList();

                    JoinedFiltersGroup? joinedFiltersGroup = null;

                    if (filteredColumnList.Count == 2)
                    {
                        joinedFiltersGroup = new JoinedFiltersGroup(filteredColumnList[0].Filter!, filteredColumnList[1].Filter!, FilterJoinType.And);

                        if (joinedFiltersGroupList.Any())
                            joinedFiltersGroupList.Last().Bind(joinedFiltersGroup, FilterJoinType.And);
                        else
                            joinedFiltersGroupList.Add(joinedFiltersGroup);
                    }
                    else if (filteredColumnList.Count == 1)
                    {
                        if (joinedFiltersGroupList.Any() && filteredColumnList.First().Filter != null)
                        {
                            joinedFiltersGroupList.Last().Bind(filteredColumnList.First().Filter!, FilterJoinType.And);
                        }
                    }
                }
            }


            return joinedFiltersGroupList;
        }
        //public IJoinFilter SetRootFilter(IQueryableFilter filter)
        //{
        //    RootFilter = filter;

        //    return this;
        //}
    }
}
