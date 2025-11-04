namespace EVBTradingContract.Request
{
    public class BatteryUpdateRequest
    {
        public string Brand { get; set; } = string.Empty; 
        public string Model { get; set; } = string.Empty; 
        public decimal BatteryCapacityKwh { get; set; }
        public decimal BatteryHealthPct { get; set; }
        public int CycleCount { get; set; }
        public string Chemistry { get; set; } = string.Empty; 
        public decimal NominalVoltageV { get; set; }
        public string? CompatibilityNote { get; set; }
        public string? Status { get; set; }
    }
}