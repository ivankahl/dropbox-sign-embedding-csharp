using DropboxSignEmbeddedSigning.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DropboxSignEmbeddedSigning.DropboxSign;
using Dropbox.Sign.Api;
using Dropbox.Sign.Client;

namespace DropboxSignEmbeddedSigning.Pages.LeaseAgreements;

[Authorize]
public class Sign(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext, DropboxSignConfiguration dsConfig) : PageModel
{
    [FromQuery(Name = "leaseAgreementId")]
    public Guid LeaseAgreementId { get; set; }
    public string SignUrl { get; set; } = "";
    
    public async Task OnGetAsync()
    {
        // If the user is not a signatory on the document, redirect back to the index page
        var user = await userManager.GetUserAsync(User) ??
                   throw new InvalidOperationException("User must be authenticated.");

        // Retrieve the lease agreement using the ID in the query string
        var leaseAgreement = await dbContext.LeaseAgreements.FindAsync(LeaseAgreementId);
        if (leaseAgreement == null)
        {
            Response.Redirect("/LeaseAgreements?errorMessage=Lease agreement not found.");
            return;
        }
        
        // Check if the current user is a signatory on the lease agreement. If they aren't redirected back to the
        // index page with an error message.
        var signatory = leaseAgreement.Signatories.FirstOrDefault(s =>
            string.Equals(s.EmailAddress, user.Email, StringComparison.InvariantCultureIgnoreCase));
        if (signatory?.DropboxSignSignatureId is null)
        {
            Response.Redirect("/LeaseAgreements?errorMessage=You are not a signatory on this lease agreement.");
            return;
        }
        
        // Since the current user is a signatory, use the Embedded API to generate a Sign URL for the user
        var api = new EmbeddedApi(new Configuration() { Username = dsConfig.ApiKey });

        var signUrlResponse = await api.EmbeddedSignUrlAsync(signatory.DropboxSignSignatureId);

        // Set the SignUrl property to the generated URL so that you can access it in the view
        SignUrl = signUrlResponse.Embedded.SignUrl;
    }
}