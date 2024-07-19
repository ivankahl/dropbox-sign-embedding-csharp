namespace DropboxSignEmbeddedSigning.Data.Models;

public enum SignatoryType
{
    Lessee,
    Lessor,
    Agent
}

public class Signatory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public SignatoryType Type { get; set; }
    public string? DropboxSignSignatureId { get; set; }
    
    public Guid LeaseAgreementId { get; set; }
    public LeaseAgreement LeaseAgreement { get; set; }
}