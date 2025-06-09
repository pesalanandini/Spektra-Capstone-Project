namespace OLMS.Api.Models
{
    public class ReportResult
    {
        public int TotalBooks { get; set; }
        public int BorrowedBooks { get; set; }
        public int OverdueBooks { get; set; }
        public decimal FinesCollected { get; set; }
        public string?  MostBorrowedBook { get; set; }
    }
}