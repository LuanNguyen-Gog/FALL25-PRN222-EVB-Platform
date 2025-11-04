namespace EVBTradingContract.Request
{
    public class ListingUpdateRequest
    {
        public string Title { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public decimal PriceVnd { get; set; }
        public decimal AiSuggestedPriceVnd { get; set; }
        public string? Status { get; set; }
    }
}