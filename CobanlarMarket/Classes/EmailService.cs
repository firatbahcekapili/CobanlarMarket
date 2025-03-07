using System.Net;
using System.Net.Mail;
using System.Security.RightsManagement;

public class EmailService
{
    public void SendEmail(string toEmail, string subject, string body)
    {
        var fromAddress = new MailAddress("", "Fırat");
        var toAddress = new MailAddress(toEmail);
        string fromPassword = "";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com", // Gmail SMTP sunucusu
            Port = 587, // TLS için doğru port
            EnableSsl = true, // SSL/TLS'i etkinleştirin
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword) // Kimlik doğrulama bilgileri
        };

        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        })
        {
            smtp.Send(message);
        }
    }




    public void ContactMail(string subject, string body)
    {
        var fromAddress = new MailAddress("", "Fırat");
        //var toAddress = new MailAddress("usermessage@cobanlarmarket.com");
        var toAddress = new MailAddress("");

        string fromPassword = "";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com", // Gmail SMTP sunucusu
            Port = 587, // TLS için doğru port
            EnableSsl = true, // SSL/TLS'i etkinleştirin
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword) // Kimlik doğrulama bilgileri
        };

        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        })
        {
            smtp.Send(message);
        }
    }
}
