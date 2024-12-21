using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DTOs
{
    public class DataColumnDTO
    {
        public Guid Id { get; set; }

        public string TableName { get; set; }

        public string Name { get; set; }

        public RowDTO RowDTO { get; set; }

        public object Value { get; set; }

        public DataColumnDTO SetRowDTO(RowDTO rowDTO)
        {
            RowDTO = rowDTO;

            return this;
        }

        public static DataColumnDTO ConvertToDataColumnDTO(IDataColumn dataColumn)
        {
            var dataColumnDTO = new DataColumnDTO()
            {
                Id = dataColumn.Id,
                TableName = dataColumn.Table?.Name,
                Name = dataColumn.Name,
                Value = dataColumn.Value
            };

            return dataColumnDTO;
        }

        public static IDataColumn ConvertToDataColumn(DataColumnDTO dataColumnDTO)
        {
            IDataColumn dataColumn = null;

            dataColumn = new DataColumn()
            {
                Id = dataColumnDTO.Id,
                Name = dataColumnDTO.Name,
                Value = dataColumnDTO.Value,
                Row = RowDTO.ConvertToRow(dataColumnDTO.RowDTO)
            };

            return dataColumn;
        }

    }
}
