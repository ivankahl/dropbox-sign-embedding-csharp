using System.ComponentModel.DataAnnotations;

namespace DropboxSignEmbeddedSigning.Data.Models;

public enum LeaseTerm
{
    [Display(Order = 0, Name = "Monthly")]
    Monthly,
    [Display(Order = 1, Name = "Three Months")]
    ThreeMonths,
    [Display(Order = 2, Name = "Six Months")]
    SixMonths,
    [Display(Order = 3, Name = "Nine Months")]
    NineMonths,
    [Display(Order = 4, Name = "One Year")]
    OneYear,
    [Display(Order = 5, Name = "Two Years")]
    TwoYears,
    [Display(Order = 6, Name = "Three Years")]
    ThreeYears,
    [Display(Order = 7, Name = "Four Years")]
    FourYears,
    [Display(Order = 8, Name = "Five Years")]
    FiveYears
}

public class LeaseAgreement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Property { get; set; }
    public string LeaseFilePath { get; set; }
    public LeaseTerm LeaseTerm { get; set; }
    public string? DropboxSignSignatureRequestId { get; set; }
    public ICollection<Signatory> Signatories { get; set; }
}