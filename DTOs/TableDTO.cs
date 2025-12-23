using Hydra.DataModels;
using Hydra.DTOs.ViewConfigurations;
using Hydra.DTOs.ViewDTOs;
using Hydra.Utils;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using static Hydra.DataModels.SortingFilterDirectionExtension;

namespace Hydra.DTOs
{
    public class TableDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public string? Alias { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public List<RowDTO> Rows { get; set; }

        public int TotalRecordsCount { get; set; }

        public int TotalPagesCount { get; set; }

        public int FilteredTotalRecordsCount { get; set; }

        public int StartRowNumber { get; set; }

        public int FinishRowNumber { get; set; }

        public bool HasManyToManyRelationship { get; set; }

        public string? ViewDTOTypeName { get; set; }

        //[JsonIgnore]
        //public Type? ViewDTOType
        //{
        //    get
        //    {
        //        return ReflectionHelper.GetTypeFromAssembly(sampleTypeInAssembly: typeof(DTO), typeName: ViewDTOTypeName);
        //    }
        //}

        public ViewType ViewType { get; set; }

        public RelationType RelationType { get; set; }

        public int FilteredRowsCount
        {
            get
            {
                return Rows != null ? Rows.Count : 0;
            }
        }

        public List<JoinTableDTO> JoinTables { get; set; }

        public List<MetaColumnDTO> MetaColumns { get; set; }

        public List<MetaColumnDTO> GetPrimaryKeyColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.IsPrimaryKey).ToList();
            }
        }

        public List<MetaColumnDTO> GetMetaColumnsExceptForeignKey
        {
            get
            {
                return MetaColumns.Where(mc => !mc.IsForeignKey).ToList();
            }
        }

        public List<MetaColumnDTO> GetFilteredMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(FilteredColumn)).ToList();
            }
        }

        public string GetFilteredMetaColumnsNames
        {
            get
            {
                return $"{string.Join(',', GetFilteredMetaColumns.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetFilteredMetaColumnsWithIncludes
        {
            get
            {
                return GetFilteredMetaColumns.Union(GetAllJoinTableDTOs.SelectMany(jt => jt.GetFilteredMetaColumns))
                                            .OrderBy(mc => mc.FilterDTO.Priority)
                                            .ToList();
            }
        }

        public string GetFilteredMetaColumnsWithIncludesNames
        {
            get
            {
                return $"{string.Join(',', GetFilteredMetaColumnsWithIncludes.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetFilteredMetaColumnsExceptForeignKeyWithIncludes
        {
            get
            {
                return GetFilteredMetaColumnsWithIncludes.Where(fc => !fc.IsForeignKey).ToList();
            }
        }

        public string GetFilteredMetaColumnsExceptForeignKeyWithIncludesNames
        {
            get
            {
                return $"{string.Join(',', GetFilteredMetaColumnsExceptForeignKeyWithIncludes.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetSelectedMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(SelectedColumn)).ToList();
            }
        }

        public string GetSelectedMetaColumnsNames
        {
            get
            {
                return $"{string.Join(',', GetSelectedMetaColumns.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetSelectedMetaColumnsWithIncludes
        {
            get
            {
                return GetSelectedMetaColumns.Union(GetAllJoinTableDTOs.SelectMany(jt => jt.GetSelectedMetaColumns)).ToList();
            }
        }

        public string GetSelectedMetaColumnsWithIncludesNames
        {
            get
            {
                return $"{string.Join(',', GetSelectedMetaColumnsWithIncludes.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetSelectedMetaColumnsExceptPrimaryKey
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(SelectedColumn) && !mc.IsPrimaryKey).ToList();
            }
        }

        public string GetSelectedMetaColumnsExceptPrimaryKeyNames
        {
            get
            {
                return $"{string.Join(',', GetSelectedMetaColumnsExceptPrimaryKey.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }


        public List<MetaColumnDTO> GetSelectedMetaColumnsExceptPrimaryKeyWithIncludes
        {
            get
            {
                return GetSelectedMetaColumnsWithIncludes.Where(mc => mc.TypeName == nameof(SelectedColumn) && !mc.IsPrimaryKey).ToList();
            }
        }

        public string GetSelectedMetaColumnsExceptPrimaryKeyWithIncludesNames
        {
            get
            {
                return $"{string.Join(',', GetSelectedMetaColumnsExceptPrimaryKeyWithIncludes.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetOrderedMetaColumns
        {
            get
            {
                return MetaColumns.Where(mc => mc.TypeName == nameof(OrderedColumn)).ToList();
            }
        }

        public string GetOrderedMetaColumnsNames
        {
            get
            {
                return $"{string.Join(',', GetOrderedMetaColumns.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public List<MetaColumnDTO> GetOrderedMetaColumnsWithIncludes
        {
            get
            {
                return GetOrderedMetaColumns.Union(GetAllJoinTableDTOs.SelectMany(jt => jt.GetOrderedMetaColumns))
                                            .ToList();
            }
        }

        public string GetOrderedMetaColumnsWithIncludesNames
        {
            get
            {
                return $"{string.Join(',', GetOrderedMetaColumnsWithIncludes.Select(c => $"{c.TableName}.{c.Name}"))}";
            }
        }

        public string GetMetaColumnsWithDuplicatedId
        {
            get
            {
                var group = MetaColumns.Union(GetAllJoinTableDTOs.SelectMany(jt => jt.MetaColumns)).GroupBy(mc => mc.Id);

                return $"{string.Join(',', group.Where(g => g.ToList().Count > 1).Select(g => g.Key))}";
            }
        }


        public List<JoinTableDTO> GetAllJoinTableDTOs
        {
            get
            {
                var subJoins = new List<JoinTableDTO>();

                JoinTables.ForEach(jt => JoinTableDTO.GetAllJoinTables(jt, true, ref subJoins));

                return subJoins;
            }
        }


        public string CreateLink
        {
            get
            {
                return $"{Name}/CreateView";
            }
        }

        [JsonIgnore]
        public bool HasFilter
        {
            get
            {
                return GetFilteredMetaColumnsWithIncludes?.Count > 0;
            }
        }

        [JsonIgnore]
        public bool HasFilterWithIncludes
        {
            get
            {
                return GetFilteredMetaColumnsWithIncludes?.Count > 0;
            }
        }

        [JsonIgnore]
        public bool HasSelectedColumnExceptPrimaryKey
        {
            get
            {
                return GetSelectedMetaColumnsExceptPrimaryKey?.Count > 0;
            }
        }


        [JsonIgnore]
        public bool HasSelectedColumnExceptPrimaryKeyWithIncludes
        {
            get
            {
                return GetSelectedMetaColumnsExceptPrimaryKeyWithIncludes?.Count > 0;
            }
        }

        [JsonIgnore]
        public bool HasResults
        {
            get
            {
                return Rows?.Count > 0;
            }
        }

        private static List<TableDTO> TableDTOCaches = new List<TableDTO>();

        public TableDTO()
        {
            Initialize();
        }

        public TableDTO(string name, string alias = null, int? pageNumber = null, int? pageSize = null)
        {
            Name = name;

            Alias = alias ?? Name;

            if (pageNumber != null)
                PageNumber = pageNumber.Value;

            if (pageSize != null)
                PageSize = pageSize.Value;

            Initialize();
        }

        public void Initialize()
        {
            Rows = new List<RowDTO>();

            MetaColumns = new List<MetaColumnDTO>();

            JoinTables = new List<JoinTableDTO>();

            Id = Guid.NewGuid();
        }


        //public TableDTO SetResults(List<object> results)
        //{
        //    //Results = results;

        //    return this;
        //}

        public TableDTO SetRelationType(RelationType relationType)
        {
            RelationType = relationType;

            return this;
        }


        public TableDTO SetMetaColumns(params MetaColumnDTO[] columns)
        {
            MetaColumns = columns.ToList();

            foreach (var column in columns)
            {
                column.TableName = Name;

                column.ViewType = ViewType;
            }

            return this;
        }

        public TableDTO AddJoinTable(JoinTableDTO joinTable)
        {
            if (string.IsNullOrEmpty(joinTable.LeftTableName))
                joinTable.SetLeftTableName(Name);

            JoinTables.Add(joinTable);

            return this;
        }

        public TableDTO AddJoinTable(string name, string alias, string leftTableColumnName, JoinType joinType, string rightTableColumnName, string rightTableName)
        {
            return AddJoinTable(new JoinTableDTO(name: name,
                                                alias: alias,
                                                leftTableName: Name,
                                                leftTableColumnName: leftTableColumnName,
                                                joinType: joinType,
                                                rightTableColumnName: rightTableColumnName,
                                                rightTableName: rightTableName));
        }

        public TableDTO SetJoinTables(params JoinTableDTO[] joinTables)
        {
            if (joinTables == null)
                return this;

            JoinTables.AddRange(joinTables);

            return this;
        }

        public TableDTO AlterOrAddMetaColumn(MetaColumnDTO column)
        {
            if (column.BelongsToJoins)
            {
                var joinTable = GetAllJoinTableDTOs.FirstOrDefault(jt => jt.Name == column.NavigationColumnInfoDTO.RightTableName);

                if (joinTable != null)
                {
                    joinTable.AlterOrAddMetaColumn(column);
                }
                else
                {
                    ConfigureJoinTables(new List<MetaColumnDTO>() { column });
                }

                return this;
            }
            else
            {
                column.SetTableName(Name).SetViewType(ViewType);

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
        }

        public TableDTO SetViewDTOTypeName(string viewDTOTypeName)
        {
            ViewDTOTypeName = viewDTOTypeName;

            return this;
        }

        public TableDTO SetViewType(ViewType viewType)
        {
            ViewType = viewType;

            return this;
        }

        public TableDTO SetRows(params RowDTO[] rowDTOs)
        {
            Rows.AddRange(rowDTOs);

            return this;
        }

        public List<T?> GetResultsAs<T>(Action<Exception>? logAction = null) where T : class
        {
            return Rows.Select(r => r.ToObject<T>(logAction)).ToList();
        }

        public async Task<List<object?>> GetResultsAsViewDTOType(Assembly? assembly= null,Action<Exception> ? logAction = null)
        {
            List<object?> results = new List<object?>();

            Stopwatch sw = new Stopwatch();

            sw.Start();

            foreach (var row in Rows)
            {
                results.Add(await row.ToObjectAsync(assembly: assembly, typeName: ViewDTOTypeName, logAction: logAction));
            }

            sw.Stop();

            var elapsed = sw.Elapsed;

            return results;
        }


        public TableDTO PrepareUsingConfigurations(Type viewDTOType)
        {
            SetViewDTOTypeName(viewDTOType.Name);

            //Type? dtoType, ViewType viewType
            //viewDTOType, ViewType
            var group = ViewDTOConfigurationCacheManager.Instance.GetOrLoad(viewDTOType, Name, ViewType);

            var configuredMetaColumns = BuildMetaColumnsFromConfiguration(group?.List ?? new());

            if (configuredMetaColumns.Any(mc => mc.TableName != Name))
                configuredMetaColumns.ForEach(c => c.TableName = Name);

            var selectedColumnDTOs = configuredMetaColumns.Where(c => c.TypeName == nameof(SelectedColumn));

            var filteredColumnDTOs = configuredMetaColumns.Where(c => c.TypeName == nameof(FilteredColumn));

            var orderedColumnDTOs = configuredMetaColumns.Where(c => c.TypeName == nameof(OrderedColumn));


            foreach (var selectedColumnDTO in selectedColumnDTOs)
            {
                if (selectedColumnDTO.BelongsToJoins)
                    continue;

                if (!GetSelectedMetaColumns.Any(mc => mc.Name == selectedColumnDTO.Name))
                    AlterOrAddMetaColumn(selectedColumnDTO);
            }

            foreach (var filteredColumnDTO in filteredColumnDTOs)
            {
                if (filteredColumnDTO.BelongsToJoins)
                    continue;

                if (!GetFilteredMetaColumns.Any(mc => mc.Name == filteredColumnDTO.Name))
                    AlterOrAddMetaColumn(filteredColumnDTO);
            }

            foreach (var orderedColumnDTO in orderedColumnDTOs)
            {
                if (orderedColumnDTO.BelongsToJoins)
                    continue;

                if (!GetOrderedMetaColumns.Any(mc => mc.Name == orderedColumnDTO.Name))
                    AlterOrAddMetaColumn(orderedColumnDTO);
            }

            ConfigureJoinTables(configuredMetaColumns.Where(cmc => cmc.BelongsToJoins).ToList());

            return this;
        }

        /// <summary>
        /// Builds a list of <see cref="MetaColumnDTO"/> objects by applying 
        /// the given ViewDTO configurations for the specified DTO type and view.
        /// </summary>
        /// <param name="dtoType">The DTO type to load configurations for.</param>
        /// <param name="viewType">The view type (ListView, DetailsView, etc.)</param>
        /// <returns>A list of generated <see cref="MetaColumnDTO"/> based on configurations.</returns>
        public List<MetaColumnDTO> BuildMetaColumnsFromConfiguration(IEnumerable<IConfiguration>? configurations)
        {

            //if (!Configuration.CacheGroups.Any(cg => cg.ViewName == dtoType.Name && cg.ViewType == viewType))
            //{
            //    var dtoInstance = ReflectionHelper.CreateInstance(typeName: dtoType.Name) as ViewDTO;

            //    dtoInstance.LoadConfigurations();

            //    if (dtoInstance == null)
            //        throw new Exception($"An instance of {dtoType.Name} could not be created");

            //    configurations = dtoInstance.Configurations.Where(c => c.ViewType == viewType);

            //    Configuration.CacheGroups.Add(new ConfigurationCacheGroup()
            //    {
            //        ViewName = dtoType.Name,
            //        ViewType = viewType,
            //        List = configurations.ToList()
            //    });
            //}
            //else
            //{
            //    configurations = Configuration.CacheGroups.FirstOrDefault(cg => cg.ViewName == dtoType.Name && cg.ViewType == viewType).List;
            //}

            var selectedColumns = new List<MetaColumnDTO>();
            var filteredColumns = new List<MetaColumnDTO>();
            var orderedColumns = new List<MetaColumnDTO>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var configuration in configurations)
            {
                if (configuration.ToSelect != null)
                {
                    var toSelect = configuration.ToSelect;

                    var selectedColumn = configuration.ToColumnDTO(nameof(SelectedColumn));

                    selectedColumns.Add(selectedColumn);
                }

                if (!configuration.IsPrimaryKey && configuration.ToFilter != null)
                {
                    var toFilter = configuration.ToFilter;

                    var filteredColumn = configuration.ToColumnDTO(nameof(FilteredColumn));

                    filteredColumns.Add(filteredColumn);
                }

                if (configuration.ToOrder != null)
                {
                    var toOrder = configuration.ToOrder;

                    var orderedColumn = configuration.ToColumnDTO(nameof(OrderedColumn));

                    orderedColumns.Add(orderedColumn);
                }
            }

            sw.Stop();
            var x = sw.Elapsed;

            if (!configurations.Any(c => c.PropertyName == "Id"))
                selectedColumns.Add(MetaColumnDTO.CreateColumnDTOToSelect(name: "Id", alias: null).SetAsPrimaryKey());

            var allColumns = selectedColumns.Union(filteredColumns).Union(orderedColumns).ToList();

            return allColumns;
        }


        public JoinTableDTO GetOrCreateJoinTableDTO(string tableName,
                                                    string alias,
                                                    string leftTableName,
                                                    string leftTableKeyName,
                                                    JoinType joinType,
                                                    string rightTableKeyName,
                                                    string rightTableName,
                                                    MetaColumnDTO? traceColumnToJoin = null)
        {
            var createNewJoinTable = new Func<JoinTableDTO>(() =>
            {
                return new JoinTableDTO(name: tableName,
                                        alias: alias,
                                        leftTableName: leftTableName,
                                        leftTableColumnName: leftTableKeyName,
                                        joinType: joinType,
                                        rightTableName: rightTableName,
                                        rightTableColumnName: rightTableKeyName);
            });

            JoinTableDTO? joinTable = null;

            if (leftTableName != Name)
            {
                var leftTable = GetAllJoinTableDTOs.FirstOrDefault(jt => jt.Name == leftTableName);

                if (leftTable == null && traceColumnToJoin != null)
                {
                    var navigationColumnInfoDTO = traceColumnToJoin.NavigationColumnInfoDTO;

                    var leftTableOfLeftTable = GetAllJoinTableDTOs.FirstOrDefault(jt => jt.RightTableName == navigationColumnInfoDTO.LeftTableName);

                    if (leftTableOfLeftTable != null)
                    {
                        leftTable = new JoinTableDTO(name: leftTableName,
                                        alias: alias,
                                        leftTableName: navigationColumnInfoDTO.LeftTableName,
                                        leftTableColumnName: navigationColumnInfoDTO.LeftTableKeyName,
                                        joinType: joinType,
                                        rightTableName: navigationColumnInfoDTO.RightTableName,
                                        rightTableColumnName: navigationColumnInfoDTO.RightTableKeyName);

                        leftTableOfLeftTable.AddJoinTable(leftTable);
                    }
                }

                joinTable = leftTable.GetAllJoinTableDTOs.FirstOrDefault(jt => jt.Name == rightTableName);

                if (joinTable == null)
                {
                    joinTable = createNewJoinTable();

                    leftTable.AddJoinTable(joinTable);
                }

                return joinTable;
            }
            else
            {
                joinTable = GetAllJoinTableDTOs.FirstOrDefault(jt => jt.LeftTableName == leftTableName && jt.Name == rightTableName);

                if (joinTable == null)
                {
                    joinTable = createNewJoinTable();

                    AddJoinTable(joinTable);
                }

                return joinTable;
            }
        }


        public JoinTableDTO GetJoinTableDTOByMetaColumnDTO(MetaColumnDTO navigationMetaColumn,
                                                            MetaColumnDTO traceColumnToJoin = null)
        {
            return GetOrCreateJoinTableDTO(tableName: navigationMetaColumn.NavigationColumnInfoDTO.RightTableName,
                                        alias: null,
                                        leftTableName: navigationMetaColumn.NavigationColumnInfoDTO.LeftTableName,
                                        leftTableKeyName: navigationMetaColumn.NavigationColumnInfoDTO.LeftTableKeyName,
                                        joinType: JoinType.Left,
                                        rightTableKeyName: navigationMetaColumn.NavigationColumnInfoDTO.RightTableKeyName,
                                        rightTableName: navigationMetaColumn.NavigationColumnInfoDTO.RightTableName,
                                        traceColumnToJoin: traceColumnToJoin);
        }

        public TableDTO ConfigureJoinTables(List<MetaColumnDTO> configuredMetaColumns)
        {
            //Dont change the following lines
            //The ghost of a dead girl hides between them

            var selectedColumnDTOs = configuredMetaColumns.Where(c => c.TypeName == "SelectedColumn");

            var filteredColumnDTOs = configuredMetaColumns.Where(c => c.TypeName == "FilteredColumn");

            var orderedColumnDTOs = configuredMetaColumns.Where(c => c.TypeName == "OrderedColumn");

            if (configuredMetaColumns.Any())
            {
                var actionToLinkMetaColumnsToJoinTables = new Action<MetaColumnDTO>((sampleNavigationColumnToGetJoinTable) =>
                {
                    var traceColumnToJoin = configuredMetaColumns.FirstOrDefault(mc => mc.NavigationColumnInfoDTO.RightTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.LeftTableName);

                    JoinTableDTO joinTable = GetJoinTableDTOByMetaColumnDTO(navigationMetaColumn: sampleNavigationColumnToGetJoinTable,
                                                                            traceColumnToJoin: traceColumnToJoin);

                    foreach (var selectedColumnDTO in selectedColumnDTOs.Where(c => c.NavigationColumnInfoDTO.LeftTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.LeftTableName && c.NavigationColumnInfoDTO.RightTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.RightTableName))
                    {
                        if (!joinTable.GetSelectedMetaColumns.Any(mc => mc.Name == selectedColumnDTO.NavigationColumnInfoDTO.NameToDisplay))
                        {
                            selectedColumnDTO.Alias = selectedColumnDTO.Name;

                            selectedColumnDTO.Name = selectedColumnDTO.NavigationColumnInfoDTO.NameToDisplay;

                            joinTable.AlterOrAddMetaColumn(selectedColumnDTO);
                        }
                    }

                    foreach (var filteredColumnDTO in filteredColumnDTOs.Where(c => c.NavigationColumnInfoDTO.LeftTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.LeftTableName && c.NavigationColumnInfoDTO.RightTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.RightTableName))
                    {
                        if (!joinTable.GetFilteredMetaColumns.Any(mc => mc.Name == filteredColumnDTO.NavigationColumnInfoDTO.NameToDisplay))
                        {
                            filteredColumnDTO.Alias = filteredColumnDTO.Name;

                            filteredColumnDTO.Name = filteredColumnDTO.NavigationColumnInfoDTO.NameToDisplay;

                            joinTable.AlterOrAddMetaColumn(filteredColumnDTO);
                        }
                    }

                    foreach (var orderedColumnDTO in orderedColumnDTOs.Where(c => c.NavigationColumnInfoDTO.LeftTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.LeftTableName && c.NavigationColumnInfoDTO.RightTableName == sampleNavigationColumnToGetJoinTable.NavigationColumnInfoDTO.RightTableName))
                    {
                        if (!joinTable.GetOrderedMetaColumns.Any(mc => mc.Name == orderedColumnDTO.NavigationColumnInfoDTO.NameToDisplay))
                        {
                            orderedColumnDTO.Alias = orderedColumnDTO.Name;

                            orderedColumnDTO.Name = orderedColumnDTO.NavigationColumnInfoDTO.NameToDisplay;

                            joinTable.AlterOrAddMetaColumn(orderedColumnDTO);
                        }
                    }
                });

                var tableDepths = new Dictionary<string, int>();

                //foreach (var column in navigationMetaColumnsGroupByTableNameToJoin.OrderBy(g => g.Key != Name)
                //                                            .ThenBy(g => g.Key)
                //                                            .SelectMany(g=>g.ToList())
                //                                            .ToList())
                //{
                //    CalculateTableDepth(configuredMetaColumns, tableDepths, column.NavigationColumnInfoDTO.LeftTableName, 0);
                //}
                CalculateTableDepth(configuredMetaColumns, tableDepths, Name, 0);

                var navigationMetaColumnsGroupByTableNameToJoin = configuredMetaColumns
                                        .GroupBy(mc => mc.NavigationColumnInfoDTO?.LeftTableName)
                                        .ToList();

                foreach (var tableDepth in tableDepths.OrderBy(td => td.Value))
                {
                    foreach (var group in navigationMetaColumnsGroupByTableNameToJoin.Where(g => g.Key == tableDepth.Key))
                    {
                        foreach (var sampleNavigationColumnToGetJoinTable in group.ToList())
                        {
                            actionToLinkMetaColumnsToJoinTables(sampleNavigationColumnToGetJoinTable);
                        }
                    }
                }



                //foreach (var group in navigationMetaColumnsGroupByTableNameToJoin
                //                        .OrderBy(g => g.Key != Name)
                //                        .ThenBy(g => g.Key))
                //{
                //    foreach (var sampleNavigationColumnToGetJoinTable in group.ToList())
                //    {

                //    }
                //}
            }

            return this;
        }


        private static void CalculateTableDepth(List<MetaColumnDTO> columns,
                                                Dictionary<string, int> tableDepths,
                                                string tableName,
                                                int currentDepth)
        {
            if (!tableDepths.ContainsKey(tableName))
            {
                tableDepths[tableName] = currentDepth;
            }


            var relatedColumns = columns.Where(column =>column.NavigationColumnInfoDTO!=null && column.NavigationColumnInfoDTO.LeftTableName == tableName && column.NavigationColumnInfoDTO.RightTableName != tableName);

            foreach (var relatedColumn in relatedColumns)
            {
                CalculateTableDepth(columns: columns,
                                    tableDepths: tableDepths,
                                    tableName: relatedColumn.NavigationColumnInfoDTO!.RightTableName,
                                    currentDepth: currentDepth + 1);
            }
        }

        public static TableDTO FromTableToDTO(ITable table)
        {
            var tableDTO = new TableDTO()
            {
                Id = table.Id,
                Name = table.Name,
                Alias = table.Alias,
                PageNumber = table.PageNumber,
                PageSize = table.PageSize,
                FilteredTotalRecordsCount = table.Pagination != null ? table.Pagination.FilteredTotalRecordsCount : 0,
                TotalPagesCount = table.Pagination != null ? table.Pagination.TotalPagesCount : 0,
                TotalRecordsCount = table.Pagination != null ? table.Pagination.TotalRecordsCount : 0,
                StartRowNumber = table.Pagination != null ? table.Pagination.Start : 1,
                FinishRowNumber = table.Pagination != null ? table.Pagination.Finish : 10,
                Rows = table.Rows.Select(r => RowDTO.ConvertToRowDTO(r)).ToList(),
                MetaColumns = table.MetaColumns.Select(mc => MetaColumnDTO.ConvertToColumnDTO(mc)).ToList(),
                JoinTables = table.JoinTables.Select(jt => JoinTableDTO.ConvertToJoinTableDTO(jt)).ToList(),
                HasManyToManyRelationship = table.HasManyToManyRelationship,
                ViewType = table.ViewType,
              //  RelationType = table.RelationType
            };

            return tableDTO;
        }

        public static Table ConvertToTable(TableDTO tableDTO)
        {
            var table = new Table(tableDTO.Name, tableDTO.Alias);

            table.Id = tableDTO.Id;

            if (tableDTO.MetaColumns != null)
                table.SetMetaColumns(tableDTO.MetaColumns.Select(mc => MetaColumnDTO.ConvertToColumn(mc)).Where(mc => mc != null).ToArray());

            table.SetFilter()
                .SetPageSize(tableDTO.PageSize)
                .SetPageNumber(tableDTO.PageNumber)
                .SetViewType(tableDTO.ViewType);
  //              .SetRelationType(tableDTO.RelationType);

            var subJoinTableDTOs = new List<JoinTableDTO>();

            foreach (var joinTableDTO in tableDTO.JoinTables)
            {
                var addedJoinTable = JoinTableDTO.ConvertToJoinTable(joinTableDTO: joinTableDTO, leftTable: table);

                table.JoinTables.Add(addedJoinTable);

                JoinTableDTO.GetAllJoinTables(joinTableDTO, true, ref subJoinTableDTOs);
            }

            foreach (var subJoinTableDTO in subJoinTableDTOs.OrderBy(jt => jt.Depth))
            {
                if (table.GetAllJoinTables.Any(jt => jt.Id == subJoinTableDTO.Id))
                    continue;

                var leftTable = table.GetAllJoinTables.FirstOrDefault(jt => jt.Name == subJoinTableDTO.LeftTableName);

                //Ensure.NotNull(leftTable,nameof(leftTable));

                var addedSubJoinTable = JoinTableDTO.ConvertToJoinTable(joinTableDTO: subJoinTableDTO, leftTable: leftTable);

                leftTable?.JoinTables.Add(addedSubJoinTable);
            }

            table.HasManyToManyRelationship = tableDTO.HasManyToManyRelationship;

            return table;
        }


        public Dictionary<Guid, object> ExtractValuesUsingColumnName(string columnName)
        {
            return Rows.Select(r => new
            {
                Key = Helper.GetNullableGuid(r.GetValueByColumnName("Id")).Value,
                Value = r.GetValueByColumnName(columnName)
            }).ToDictionary(o => o.Key, o => o.Value);
        }

        public RowDTO GetRowDTO(object entity, List<MetaColumnDTO> metaColumns)
        {
            var rowDTO = new RowDTO().SetTableName(Name);

            var id = Helper.GetNullableGuid(ReflectionHelper.GetValueOf(entity.GetType(), "Id", entity));

            rowDTO.SetId(id.Value);

            foreach (var column in metaColumns)
            {
                var value = ReflectionHelper.GetValueOf(entity.GetType(), column.BelongsToJoins ? column.Alias : column.Name, entity);

                rowDTO.Columns.Add(new DataColumnDTO()
                {
                    Name = column.Name,
                    Value = value,
                    TableName = Name
                });
            }

            return rowDTO;
        }

        public MetaColumnDTO GetSelectedMetaColumnDTO(string columnName, bool isNavigation = false, string leftTableName = null, string rightTableName = null)
        {
            return GetMetaColumnDTO(columnType: nameof(SelectedColumn), columnName: columnName, isNavigation: isNavigation, leftTableName: leftTableName, rightTableName: rightTableName);
        }

        public MetaColumnDTO GetFilteredMetaColumnDTO(string columnName, bool isNavigation = false, string leftTableName = null, string rightTableName = null)
        {
            return GetMetaColumnDTO(columnType: nameof(FilteredColumn), columnName: columnName, isNavigation: isNavigation, leftTableName: leftTableName, rightTableName: rightTableName);
        }

        public MetaColumnDTO GetOrderedMetaColumnDTO(string columnName, bool isNavigation = false, string leftTableName = null, string rightTableName = null)
        {
            return GetMetaColumnDTO(columnType: nameof(OrderedColumn), columnName: columnName, isNavigation: isNavigation, leftTableName: leftTableName, rightTableName: rightTableName);
        }

        public MetaColumnDTO GetMetaColumnDTO(string columnType, string columnName, bool isNavigation = false, string leftTableName = null, string rightTableName = null)
        {
            MetaColumnDTO metaColumnDTO = null;

            if (string.IsNullOrEmpty(leftTableName))
                leftTableName = Name;

            if (isNavigation)
            {
                switch (columnType)
                {
                    case nameof(SelectedColumn):

                        metaColumnDTO = GetSelectedMetaColumnsWithIncludes.FirstOrDefault(mc => mc.IsNavigation && mc.NavigationColumnInfoDTO.LeftTableName == leftTableName && mc.NavigationColumnInfoDTO.RightTableName == rightTableName && mc.NavigationColumnInfoDTO.NameToDisplay == columnName);

                        break;

                    case nameof(FilteredColumn):

                        metaColumnDTO = GetFilteredMetaColumnsWithIncludes.FirstOrDefault(mc => mc.IsNavigation && mc.NavigationColumnInfoDTO.LeftTableName == leftTableName && mc.NavigationColumnInfoDTO.RightTableName == rightTableName && mc.NavigationColumnInfoDTO.NameToDisplay == columnName);

                        break;

                    case nameof(OrderedColumn):

                        metaColumnDTO = GetOrderedMetaColumnsWithIncludes.FirstOrDefault(mc => mc.IsNavigation && mc.NavigationColumnInfoDTO.LeftTableName == leftTableName && mc.NavigationColumnInfoDTO.RightTableName == rightTableName && mc.NavigationColumnInfoDTO.NameToDisplay == columnName);

                        break;
                }
            }
            else
            {
                metaColumnDTO = MetaColumns.FirstOrDefault(mc => mc.TypeName == columnType && mc.Name == columnName);
            }

            return metaColumnDTO;
        }
    }
}
