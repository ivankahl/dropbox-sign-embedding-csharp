using System.Net.Mail;

namespace DropboxSignEmbeddedSigning.Extensions;

public static class ValidationExtensions
{
    public static bool AreAllValidEmailAddresses(this ICollection<string> emailAddressList)
    {
        return emailAddressList.All(x => x.IsValidEmailAddress());
    }

    public static bool IsValidEmailAddress(this string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress))
            return false;
        
        try
        {
            _ = new MailAddress(emailAddress);
            return true;
        }
        catch (FormatException e)
        {
            return false;
        }
    }
}