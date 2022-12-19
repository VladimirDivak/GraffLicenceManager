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
            senderMail = "license.validation@graff.tech";

            mail = new MailMessage();
            smptServer = new SmtpClient("mail.netangels.ru");
            dangeonMasterMail = "gera@graff.tech";

            smptServer.Port = 25;
            smptServer.EnableSsl = false;
            smptServer.UseDefaultCredentials = false;
            smptServer.Credentials = new NetworkCredential(
                senderMail,
                "L010203V"
            );
        }

        public async Task SendWarningAsync(string subject, string message)
        {
            mail.From = new MailAddress(senderMail);
            mail.Subject = subject;
            mail.Body = message;

            mail.To.Add(dangeonMasterMail);
            mail.To.Add("vladimir.divak@gmail.com");
            mail.To.Add("1andariel1@mail.ru");

            await Task.Run(() =>
            {
                smptServer?.Send(mail);
            });
        }
    }
}
