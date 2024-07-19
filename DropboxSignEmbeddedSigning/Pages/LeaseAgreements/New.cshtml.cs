using System.ComponentModel.DataAnnotations;
using DropboxSignEmbeddedSigning.Data;
using DropboxSignEmbeddedSigning.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DropboxSignEmbeddedSigning.Pages.LeaseAgreements;

[Authorize(Roles = "Administrator")]
public class New(IWebHostEnvironment environment, ApplicationDbContext dbContext) : PageModel
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
        // TODO: Implement signing logic
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