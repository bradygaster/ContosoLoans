namespace ContosoLoans {
    [GenerateSerializer]
    public class CreditCheck {
        [Id(0)]
        public Guid ApplicationId { get; set; }
        [Id(1)]
        public string? Agency { get; set; }
        [Id(2)]
        public bool? IsApproved { get; set; } = false;
        [Id(3)]
        public DateTime? Completed { get; set; }
        [Id(4)]
        public DateTime Started { get; set; }
    }
}