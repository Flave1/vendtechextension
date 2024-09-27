namespace vendtechext.DAL.Models
{
    public class Log
    {
        public long Id { get; set; }
        public int LogType { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
        public string StackTrace { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
