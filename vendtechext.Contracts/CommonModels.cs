namespace vendtechext.Contracts
{
    public class PaginatedSearchRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortOrder { get; set; } = "Desc";
        public string SortBy { get; set; } = "CreatedAt";
        public string SortValue { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public Guid? IntegratorId { get; set; } = null;
        public int Status { get; set; } = 1;
        public int IsClaimedStatus { get; set; } = -2;
    }

    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Data { get; set; }

        public PagedResponse(IEnumerable<T> data, int totalRecords, int pageNumber, int pageSize)
        {
            Data = data;
            TotalRecords = totalRecords;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        }
    }
}
