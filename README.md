# Hydra Project

Hydra is a powerful framework designed to simplify query generation and execution. It enables developers to construct complex SQL queries programmatically while maintaining readability and flexibility.

## Current Solution Demonstration

Here is an example of how Hydra helps in constructing a query:

### Code Example
```csharp
var table = new Table(name: "Person.Person", alias: "pper")
    .SetMetaColumns(new SelectedColumn(name: nameof(PersonPerson.FirstName)),
                    new SelectedColumn(name: nameof(PersonPerson.LastName)),
                    new OrderedColumn(name: nameof(PersonPerson.FirstName), direction: "desc"),
                    new FilteredColumn(name: nameof(PersonPerson.FirstName),
                        filter: new ContainsFilter(value: "ia")))
    .SetJoins(t => new List<IJoinTable>()
        {
            new JoinTable("Person.PersonPhone","pphn",JoinType.Inner)
                .SetLeftTable((Table?)t)
                .On(nameof(PersonPerson.BusinessEntityId), nameof(PersonPhone.BusinessEntityId))
                .SetMetaColumns(new SelectedColumn("PhoneNumber", "PersonPhone.PhoneNumber", false))
        })
    .SetPageNumber(1)
    .SetPageSize(10);
```

### Generated SQL Query
The above code generates the following SQL query:
```sql
exec sp_executesql N'select o.* from(select  pper.FirstName as [FirstName], pper.LastName as [LastName], pphn.PhoneNumber as [PersonPhone.PhoneNumber], ROW_NUMBER() OVER (ORDER BY pper.FirstName desc) AS RowNumber from Person.Person pper Inner join Person.PersonPhone pphn on pper.BusinessEntityId=pphn.BusinessEntityId  where pper.FirstName like ''%''+@0+''%'') o where o.RowNumber between 1 and 10',N'@0 nvarchar(2)',@0=N'ia'
```

### Query Output
The query retrieves data from the `Person.Person` and `Person.PersonPhone` tables, joining them based on the `BusinessEntityId`. It filters results where `FirstName` contains the substring "ia," orders the results by `FirstName` in descending order, and paginates the results to return rows 1 through 10.

### Visual Example
Below is a screenshot of the output in Visual Studio:
*(You can include the image here.)*

## Key Features
- **Dynamic Query Generation:** Write queries in code without sacrificing performance or flexibility.
- **Joins and Filtering:** Supports complex joins and filter conditions with intuitive syntax.
- **Pagination Support:** Built-in support for paginated query results.
- **Optimized for SQL Server:** Leverages SQL Server features like `ROW_NUMBER` and parameterized queries.

## Future Enhancements
- Support for additional database systems.
- More flexible filter operators.
- Extended metadata for dynamic mappings.

## Contributing
Contributions are welcome! Feel free to open issues or submit pull requests to improve Hydra.

---

For more details, check the project documentation or contact the development team.

