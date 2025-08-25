namespace DocumentAccessApprovalSystem.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sensitivity { get; set; }
    }
}
