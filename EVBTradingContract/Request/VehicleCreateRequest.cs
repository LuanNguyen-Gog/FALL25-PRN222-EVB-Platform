namespace EVBTradingContract.Request
{
    public class VehicleCreateRequest
    {
        public Guid OwnerId { get; set; }
        public string Brand { get; set; } = string.Empty; 
        public string Model { get; set; } = string.Empty; 
        public int Year { get; set; }
        public int OdometerKm { get; set; }
        public string? Status { get; set; } 
    }
}