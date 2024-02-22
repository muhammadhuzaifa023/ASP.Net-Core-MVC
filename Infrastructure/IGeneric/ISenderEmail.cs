namespace ASP.Net_Core_MVC.Infrastructure.IGeneric
{
    public interface ISenderEmail
    {
        Task SendEmailAsync(string ToEmail, string Subject, string Body, bool IsBodyHtml = false);
    }

}
