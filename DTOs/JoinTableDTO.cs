using Hydra.DataModels;
using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.DTOs
{
    public class JoinTableDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Alias { get; set; }

        public JoinType JoinType { get; set; }

        public string LeftTableName { get; set; }

        public string LeftTableColumnName { get; set; }

        public string RightTableName { get; set; }

        public string RightTableColumnName { get; set; }

        public RelationType RelationType { get; set; }

        public int Depth { get; set; }

        public List<MetaColumnDTO> MetaColumns { get; set; }

        public List<JoinTableDTO> JoinTables { get; set; }

        public List<JoinTableDTO> GetAllJoinTableDTOs
        {
            get
            {
                var subJoins = new List<JoinTableDTO>();

                JoinTables.ForEach(jt => JoinTableDTO.GetAllJoinTables(jt, true, ref subJoins));

                return subJoins;
            }
        }


        public JoinTableDTO()
        {
            Initialize();
        }

        public JoinTableDTO(string name, string alias, string leftTableName, string leftTableColumnName, JoinType joinType, string rightTableColumnName, string rightTableName)
        {
            Name = name;

            Alias = alias ?? Name;

            JoinType = joinType;

            SetLeftTableName(leftTableName);

            SetLeftTableColumnName(leftTableColumnName);

            SetRightTableName(rightTableName);

            SetRightTableColumnName(rightTableColumnName);

            Initialize();
        }
        public JoinTableDTO(string name, string alias, string leftTableColumnName, JoinType joinType, string rightTableColumnName, string rightTableName) : this(name: name, alias: alias, leftTableName: null, leftTableColumnName: leftTableColumnName, joinType: joinType, rightTableColumnName: rightTableColumnName, rightTableName: rightTableName)
        {
        }

        public void Initialize()
        {
            MetaColumns = new List<MetaColumnDTO>();

            JoinTables = new List<JoinTableDTO>();

            Id = Guid.NewGuid();
        }


        public static JoinTable ConvertToJoinTable(JoinTableDTO joinTableDTO, Table leftTable)
        {
            JoinTable joinTable = new JoinTable(joinTableDTO.Name, joinTableDTO.Alias, joinTableDTO.JoinType)
                                        .On(joinTableDTO.LeftTableColumnName, joinTableDTO.RightTableColumnName)
                                        .SetMetaColumns(joinTableDTO.MetaColumns.Select(mc => MetaColumnDTO.ConvertToColumn(mc)).Where(mc => mc != null).ToArray())
                                        .SetLeftTable(leftTable)
                                        .SetRelationType(joinTableDTO.RelationType)
                                        .SetDepth(joinTableDTO.Depth);

            joinTable.Id = joinTableDTO.Id;

            return joinTable;
        }

        public static JoinTableDTO ConvertToJoinTableDTO(JoinTable joinTable)
        {
            JoinTableDTO joinTableDTO = new JoinTableDTO()
            {
                Id = joinTable.Id,

                Name = joinTable.Name,

                //LeftTable = TableDTO.FromTableToDTO(joinTable.LeftTable),

                JoinType = joinTable.JoinType,

                LeftTableName = joinTable.LeftTable?.Name,

                LeftTableColumnName = joinTable.ColumnEquality?.LeftColumn?.Name,

                RightTableName = joinTable.Name,

                RightTableColumnName = joinTable.ColumnEquality?.RightColumn?.Name,

                JoinTables = joinTable.JoinTables.Select(jt => ConvertToJoinTableDTO(jt)).ToList(),

                RelationType = joinTable.RelationType,

                Depth = joinTable.Depth
            };

            joinTableDTO.SetMetaColumns(joinTable.MetaColumns.Select(mc => MetaColumnDTO.ConvertToColumnDTO(mc)).ToArray());

            return joinTableDTO;
        }

        public JoinTableDTO AlterOrAddMetaColumn(MetaColumnDTO column)
        {
            column.SetTableName(Name);

            var mc = MetaColumns.FirstOrDefault(mc => (mc.Id == column.Id) || (mc.Name == column.Name && mc.TypeName == column.TypeName));

            if (mc == null)
            {
                MetaColumns.Add(column);
            }
            else
            {
                mc.Alter(column);
            }

            return this;
        }

        public JoinTableDTO SetDepth(int depth)
        {
            Depth = depth;

            return this;
        }

        public JoinTableDTO SetLeftTableColumnName(string leftTableColumnName)
        {
            LeftTableColumnName = leftTableColumnName;

            return this;
        }

        public JoinTableDTO SetLeftTableName(string leftTableName)
        {
            LeftTableName = leftTableName;

            return this;
        }

        public JoinTableDTO SetRightTableColumnName(string rightTableColumnName)
        {
            RightTableColumnName = rightTableColumnName;

            return this;
        }

        public JoinTableDTO SetRightTableName(string rightTableName)
        {
            RightTableName = rightTableName;

            return this;
        }

        public JoinTableDTO SetMetaColumns(params MetaColumnDTO[] columns)
        {
            foreach (var column in columns)
            {
                column.TableName = Name;
            }

            MetaColumns = columns.ToList();

            return this;
        }

        public JoinTableDTO SetRelationType(RelationType relationType)
        {
            RelationType = relationType;

            return this;
        }

        public List<MetaColumnDTO> GetFilteredMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(FilteredColumn)).OrderBy(mc => mc.FilterDTO.Priority).ToList();
            }
        }

        //public List<ColumnDTO> GetFilteredMetaColumnsExceptForeignKey
        //{
        //    get
        //    {
        //        return GetFilteredMetaColumns.Where(fc => !fc.IsForeignKey).ToList();
        //    }
        //}


        public List<MetaColumnDTO> GetSelectedMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(SelectedColumn)).ToList();
            }
        }


        public List<MetaColumnDTO> GetSelectedMetaColumnsExceptPrimaryKey
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(SelectedColumn)).Except(MetaColumns.Where(mc => mc.IsPrimaryKey)).ToList();
            }
        }


        public List<MetaColumnDTO> GetOrderedMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(OrderedColumn)).ToList();
            }
        }

        public JoinTableDTO AddJoinTable(JoinTableDTO joinTable)
        {
            JoinTables.Add(joinTable);

            return this;
        }
        public JoinTableDTO AddJoinTable(string name, string alias, string leftTableColumnName, JoinType joinType, string rightTableColumnName, string rightTableName)
        {
            return AddJoinTable(new JoinTableDTO(name: name,
                                                alias: alias,
                                                leftTableName: Name,
                                                leftTableColumnName: leftTableColumnName,
                                                joinType: joinType,
                                                rightTableColumnName: rightTableColumnName,
                                                rightTableName: rightTableName));
        }

        public static void GetAllJoinTables(JoinTableDTO parent, bool includesParent, ref List<JoinTableDTO> list, int depth = 0)
        {
            if (parent.JoinTables == null || !parent.JoinTables.Any())
            {
                if (includesParent)
                {
                    var clonedParent = ReflectionHelper.Clone<JoinTableDTO>(parent);

                    clonedParent.SetDepth(depth);

                    list.Add(clonedParent);
                }

                return;
            }

            if (includesParent)
            {
                var clonedParent = ReflectionHelper.Clone<JoinTableDTO>(parent);

                clonedParent.SetDepth(depth);

                list.Add(clonedParent);
            }

            foreach (var child in parent.JoinTables)
            {
                GetAllJoinTables(child, includesParent, ref list, depth + 1);
            }
        }
    }
}
