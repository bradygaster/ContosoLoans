namespace ContosoLoans {
    [GenerateSerializer]
    public class LoanApplication {
        [Id(0)]
        public Guid ApplicationId { get; set; } = Guid.NewGuid();
        [Id(1)]
        public Guid CustomerId { get; set; } = Guid.NewGuid();
        [Id(2)]
        public bool? IsApproved { get; set; }
        [Id(3)]
        public double LoanAmount { get; set; } = 0;
        [Id(4)]
        public DateTime? Received { get; set; }
        [Id(5)]
        public DateTime? Processed { get; set; }
        [Id(6)]
        public DateTime? LastUpdate { get; set; }
    }
}
