namespace DropboxSignEmbeddedSigning.DropboxSign;

public class DropboxSignConfiguration
{
    public string ApiKey { get; set; }
    public string ClientId { get; set; }
    public bool TestMode { get; set; }
    public List<string> CcEmailAddress { get; set; }
}