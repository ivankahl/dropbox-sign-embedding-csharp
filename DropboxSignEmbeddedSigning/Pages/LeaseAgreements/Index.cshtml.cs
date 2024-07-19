using DropboxSignEmbeddedSigning.Data;
using DropboxSignEmbeddedSigning.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DropboxSignEmbeddedSigning.Pages.LeaseAgreements;

[Authorize]
public class Index(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager) : PageModel
{
    [FromQuery(Name = "successMessage")]
    public string? SuccessMessage { get; set; }
    [FromQuery(Name = "errorMessage")]
    public string? ErrorMessage { get; set; }
    public LeaseOutputModel[] Leases { get; set; }

    public class LeaseOutputModel
    {
        public Guid Id { get; set; }
        public string Property { get; set; }
        public LeaseTerm Term { get; set; }
        public string LessorName { get; set; }
        public string LesseeName { get; set; }
        public string? AgentName { get; set; }
        public string? DropboxSignSignatureId { get; set; }
    }
    
    public async Task OnGetAsync()
    {
        var leaseQuery = dbContext.LeaseAgreements.AsQueryable();
        leaseQuery.Include(x => x.Signatories);
        
        // If the user has an Administrator role, show all lease agreements. Otherwise, only show agreements
        // where the current user is a signatory.
        var user = await userManager.GetUserAsync(User) ??
                   throw new InvalidOperationException("User must be authenticated.");
        if (!await userManager.IsInRoleAsync(user, "Administrator"))
        {
            leaseQuery = leaseQuery.Where(l => l.Signatories.Any(s => EF.Functions.Like(s.EmailAddress, user.Email)));
        }

        var dbLeases = await leaseQuery.ToListAsync();

        Leases = dbLeases.Select(l => new LeaseOutputModel()
        {
            Id = l.Id,
            Property = l.Property,
            Term = l.LeaseTerm,
            LessorName = l.Signatories.First(s => s.Type == SignatoryType.Lessor).Name,
            LesseeName = l.Signatories.First(s => s.Type == SignatoryType.Lessee).Name,
            AgentName = l.Signatories.FirstOrDefault(s => s.Type == SignatoryType.Agent)?.Name,
            DropboxSignSignatureId = l.Signatories
                .First(s => string.Equals(s.EmailAddress, user.Email, StringComparison.InvariantCultureIgnoreCase))
                .DropboxSignSignatureId
        }).ToArray();
    }
}