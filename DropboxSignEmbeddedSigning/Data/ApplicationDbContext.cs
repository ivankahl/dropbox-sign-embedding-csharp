using DropboxSignEmbeddedSigning.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DropboxSignEmbeddedSigning.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<LeaseAgreement> LeaseAgreements => Set<LeaseAgreement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<LeaseAgreement>(leaseAgreement =>
        {
            leaseAgreement.Property(x => x.LeaseTerm)
                .HasConversion<EnumToStringConverter<LeaseTerm>>();
            
            leaseAgreement.OwnsMany(la => la.Signatories, signatory =>
            {
                signatory.WithOwner(s => s.LeaseAgreement)
                    .HasForeignKey(s => s.LeaseAgreementId);

                signatory.HasKey(s => s.Id);

                signatory.Property(x => x.Type)
                    .HasConversion<EnumToStringConverter<SignatoryType>>();
            });
        });
        
        base.OnModelCreating(builder);
    }
}