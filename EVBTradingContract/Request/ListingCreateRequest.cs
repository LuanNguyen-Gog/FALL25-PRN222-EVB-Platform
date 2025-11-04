namespace EVBTradingContract.Request
{
    public class ListingCreateRequest
    {
        public Guid SellerId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid BatteryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PriceVnd { get; set; }
        public decimal AiSuggestedPriceVnd { get; set; }
        public string? Status { get; set; }
    }
}