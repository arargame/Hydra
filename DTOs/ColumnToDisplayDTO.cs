using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class ColumnToDisplayDTO
    {
        public Guid RowId { get; set; }

        public RowDTO Row { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public static ColumnToDisplayDTO FromColumnToDisplayToDTO(ColumnToDisplay columnToDisplay)
        {
            return new ColumnToDisplayDTO()
            {
                RowId = columnToDisplay.Row.Id,
                Id = columnToDisplay.Id,
                Name = columnToDisplay.Name,
                Value = columnToDisplay.Value
            };
        }

        public ColumnToDisplayDTO SetRowDTO(RowDTO row)
        {
            Row = row;

            return this;
        }
    }
}
