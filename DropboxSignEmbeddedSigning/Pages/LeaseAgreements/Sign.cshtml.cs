using DropboxSignEmbeddedSigning.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DropboxSignEmbeddedSigning.Pages.LeaseAgreements;

[Authorize]
public class Sign(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext) : PageModel
{
    [FromQuery(Name = "leaseAgreementId")]
    public Guid LeaseAgreementId { get; set; }
    public string SignUrl { get; set; } = "";
    
    public async Task OnGetAsync()
    {
        
    }
}