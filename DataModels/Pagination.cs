using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DataModels
{
    public class Pagination
    {
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }

        public int FilteredTotalRecordsCount { get; set; }

        public int TotalRecordsCount { get; set; }

        public int TotalPagesCount { get; set; }

        public int Start { get; private set; }

        public int Finish { get; private set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPagesCount;

        public Pagination(int pageNumber, int pageSize, int totalRecordsCount, int filteredTotalRecordsCount = 0)
        {
            PageSize = pageSize;
            TotalRecordsCount = totalRecordsCount;
            FilteredTotalRecordsCount = filteredTotalRecordsCount;

            if (filteredTotalRecordsCount > 0)
            {
                TotalPagesCount = (int)Math.Ceiling((double)filteredTotalRecordsCount / PageSize);
            }
            else
            {
                TotalPagesCount = (int)Math.Ceiling((double)TotalRecordsCount / PageSize);
            }

            SetPageNumber(pageNumber);
        }

        public Pagination SetPageSize(int pageSize)
        {
            PageSize = pageSize;
            CalculateStartAndFinish();
            return this;
        }

        public Pagination SetPageNumber(int pageNumber)
        {
            PageNumber = pageNumber;

            if (PageNumber < 1)
            {
                PageNumber = 1; 
            }
            else if (PageNumber > TotalPagesCount)
            {
                PageNumber = TotalPagesCount; 
            }

            CalculateStartAndFinish();
            return this;
        }

        private void CalculateStartAndFinish()
        {
            Start = ((PageNumber - 1) * PageSize) + 1;

            Finish = PageNumber * PageSize;

            if (Finish > TotalRecordsCount)
            {
                Finish = TotalRecordsCount;
            }
        }
    }
}
