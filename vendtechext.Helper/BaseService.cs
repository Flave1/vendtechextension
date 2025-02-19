using vendtechext.Contracts;

namespace vendtechext.Helper
{
    public class BaseService
    {
        public BaseService()
        {
            Response = new Response();
        }
        public Response Response { get; set; } = new Response();

        public ExecutionResult GenerateExecutionResult(Exception ex, int code)
        {
            ExecutionResult executionResult = new ExecutionResult();
            executionResult.failedResponse = new FailedResponse();
            executionResult.failedResponse.ErrorMessage = ex.Message;
            executionResult.failedResponse.ErrorDetail = ex.Message;
            executionResult.code = code;
            return executionResult;
        }
    }
}
