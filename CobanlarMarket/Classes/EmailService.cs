using System.Net;
using System.Net.Mail;

public class EmailService
{
    public void SendEmail(string toEmail, string subject, string body)
    {
        var fromAddress = new MailAddress("fratbhckpl@gmail.com", "Fırat");
        var toAddress = new MailAddress(toEmail);
        string fromPassword = "jlcj mjix aqkw mryz";

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
