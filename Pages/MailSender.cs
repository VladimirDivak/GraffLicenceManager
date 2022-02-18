using System.Net;
using System.Linq;
using System.Net.Mail;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GraffLicenceManager
{
    public sealed class MailSender
    {
        string senderMail;

        MailMessage mail;
        SmtpClient smptServer;
        string dangeonMasterMail;

        public MailSender()
        {
            senderMail = "graff.validation@gmail.com";

            mail = new MailMessage();
            smptServer = new SmtpClient("smtp.gmail.com");
            dangeonMasterMail = "gera@graff.tech";

            smptServer.EnableSsl = true;
            smptServer.Port = 587;
            smptServer.UseDefaultCredentials = false;
            smptServer.Credentials = new NetworkCredential(
                senderMail,
                "tBUL3WbWUnARhCe"
            );
        }

        public async void SendWarningAsync(string subject, string message)
        {
            mail.From = new MailAddress(senderMail);
            mail.Subject = subject;
            mail.Body = message;

            mail.To.Add(dangeonMasterMail);

            await Task.Run(() =>
            {
                smptServer?.Send(mail);
            });
        }
    }
}
