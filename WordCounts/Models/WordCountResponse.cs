namespace WordCounts.Models
{
    public class WordCountResponse
    {
        public string Word { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
