using System;
using System.Net.Mail;

namespace Codex.Core.Tools;

public static class EmailValidator
{
    public static bool EmailValid(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            MailAddress mailAddress = new(email);
            return (mailAddress.Address == email);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}