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

    public class SingleTransation
    {
        public string TransactionId { get; set; }
        public Guid Integratorid { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class RequestResponse
    {
        public object Request { get; set; }
        public object Response { get; set; }
        public RequestResponse(ElectricitySaleRequest request, ExecutionResult response)
        {
            Request = request;
            Response = response;
        }
        public RequestResponse(SaleStatusRequest request, ExecutionResult response)
        {
            Request = request;
            Response = response;
        }
        public RequestResponse(RTSRequestmodel request, RTSResponse response)
        {
            Request = request;
            Response = response;
        }
        public RequestResponse(RTSRequestmodel request, RTSErorResponse response)
        {
            Request = request;
            Response = response;
        }
    }
}
