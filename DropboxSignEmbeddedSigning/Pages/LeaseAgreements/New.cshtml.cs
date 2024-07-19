using System.ComponentModel.DataAnnotations;
using DropboxSignEmbeddedSigning.Data;
using DropboxSignEmbeddedSigning.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DropboxSignEmbeddedSigning.DropboxSign;
using Dropbox.Sign.Api;
using Dropbox.Sign.Client;
using Dropbox.Sign.Model;

namespace DropboxSignEmbeddedSigning.Pages.LeaseAgreements;

[Authorize(Roles = "Administrator")]
public class New(IWebHostEnvironment environment, ApplicationDbContext dbContext, DropboxSignConfiguration dsConfig) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }
    
    public class InputModel
    {
        [Required]
        public string Property { get; set; }
        [Required]
        public LeaseTerm Term { get; set; }
        [Required]
        [Display(Name = "Lease Agreement")]
        public IFormFile LeaseAgreementFile { get; set; }
        [Required]
        public string LesseeName { get; set; }
        [Required]
        public string LesseeEmailAddress { get; set; }
        [Required]
        public string LessorName { get; set; }
        [Required]
        public string LessorEmailAddress { get; set; }
        public string? AgentName { get; set; }
        public string? AgentEmailAddress { get; set; }
    }
    
    public async Task OnGetAsync()
    {
    }

    public async Task OnPostAsync()
    {
        if (!ModelState.IsValid)
            return;

        // Save the uploaded lease agreement file to the server
        var leaseAgreementFilePath = await SaveLeaseAgreementAsync();
        
        // Create a new lease agreement and save it to the database
        var leaseAgreement = await SaveToDatabaseAsync(leaseAgreementFilePath);
        
        // Send the document to Dropbox Sign
        await SendToDropboxSignAsync(leaseAgreementFilePath, leaseAgreement);
        
        // Redirect to list of lease agreements with success message
        Response.Redirect("/LeaseAgreements?successMessage=Lease agreement created successfully.");
    }

    private async Task SendToDropboxSignAsync(string leaseAgreementFilePath, LeaseAgreement leaseAgreement)
    {
        // Create a new instance of the Signature Request API using the API Key in the app settings.
        var api = new SignatureRequestApi(new Configuration() { Username = dsConfig.ApiKey });

        // Specify which signature options should be available when signing the document.
        var signingOptions = new SubSigningOptions(
            draw: true,
            phone: true,
            type: true,
            upload: true,
            defaultType: SubSigningOptions.DefaultTypeEnum.Draw);

        // Create a list of signers for the document based on the signatories in the lease agreement.
        var signers = leaseAgreement.Signatories.Select(x => new SubSignatureRequestSigner(x.Name, x.EmailAddress))
    .ToList();

        // Create the signature request object which you'll send to the Dropbox Sign API    
        var lessee = leaseAgreement.Signatories.First(x => x.Type == SignatoryType.Lessee);
        var request = new SignatureRequestCreateEmbeddedRequest(
            clientId: dsConfig.ClientId,
            title: $"{leaseAgreement.Property} for {lessee.Name}",
            message: $"Please sign the lease agreement for {leaseAgreement.Property}.",
            ccEmailAddresses: dsConfig.CcEmailAddress,
            testMode: dsConfig.TestMode,
            files: [System.IO.File.OpenRead(leaseAgreementFilePath)],
            signingOptions: signingOptions,
            signers: signers);

        try
        {
            // Send the request to the API and retrieve the response.
            var response = await api.SignatureRequestCreateEmbeddedAsync(request);
            
            // Update the lease agreement in the database and signers with the Dropbox Sign IDs
            leaseAgreement.DropboxSignSignatureRequestId = response.SignatureRequest.SignatureRequestId;
            
            foreach (var signer in response.SignatureRequest.Signatures)
            {
                var signatory = leaseAgreement.Signatories.First(x => string.Equals(signer.SignerEmailAddress,
                    x.EmailAddress, StringComparison.InvariantCultureIgnoreCase));
                signatory.DropboxSignSignatureId = signer.SignatureId;
            }

            dbContext.Update(leaseAgreement);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while sending the lease agreement for signature. Please try again");
        }
    }

    /// <summary>
    /// Method that saves the uploaded lease agreement file contained in the Input.LeaseAgreementFile property. Will
    /// return the path to the uploaded file on the server.
    /// </summary>
    /// <returns>The file path to the uploaded file, including the file name.</returns>
    private async Task<string> SaveLeaseAgreementAsync()
    {
        var fileName = Path.GetFileNameWithoutExtension(Input.LeaseAgreementFile.FileName);
        var extension = Path.GetExtension(Input.LeaseAgreementFile.FileName);

        var uploadsFolder = Path.Combine(environment.ContentRootPath, "Uploads");
        Directory.CreateDirectory(uploadsFolder);

        var uploadedFilePath = Path.Combine(uploadsFolder, $"{fileName}-{Guid.NewGuid()}{extension}");
        await using (var stream = new FileStream(uploadedFilePath, FileMode.Create))
        {
            await Input.LeaseAgreementFile.CopyToAsync(stream);
        }

        return uploadedFilePath;
    }

    private async Task<LeaseAgreement> SaveToDatabaseAsync(string leaseAgreementFilePath)
    {
        var leaseAgreement = new LeaseAgreement
        {
            Property = Input.Property,
            LeaseTerm = Input.Term,
            LeaseFilePath = leaseAgreementFilePath,
            Signatories = new List<Signatory>
            {
                new Signatory
                {
                    Type = SignatoryType.Lessee,
                    Name = Input.LesseeName,
                    EmailAddress = Input.LesseeEmailAddress
                },
                new Signatory
                {
                    Type = SignatoryType.Lessor,
                    Name = Input.LessorName,
                    EmailAddress = Input.LessorEmailAddress
                }
            }
        };
        
        // Add the agent as a signatory if they specified a name and email address
        if (!string.IsNullOrWhiteSpace(Input.AgentName) && !string.IsNullOrWhiteSpace(Input.AgentEmailAddress))
        {
            leaseAgreement.Signatories.Add(new Signatory
            {
                Type = SignatoryType.Agent,
                Name = Input.AgentName,
                EmailAddress = Input.AgentEmailAddress
            });
        }

        await dbContext.AddAsync(leaseAgreement);
        await dbContext.SaveChangesAsync();

        return leaseAgreement;
    }
}