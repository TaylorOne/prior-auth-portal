namespace PriorAuthApi.Entities
{
    public class Coverage
    {
        public int Id { get; set; }
        public string MemberId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string PayerName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public DateOnly CoverageStartDate { get; set; }
        public DateOnly? CoverageEndDate { get; set; }
        public string? RxBIN { get; set; }
        public string? RxPCN { get; set; }
        public string? RxGrp { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
    }
}