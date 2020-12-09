using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace SueñoCelestePagos.Utilities
{

    public class Email
    {
        private string smtpServer;
        private int smtpPort;
        private bool smtpCredentials;
        private bool enableSsl;

        public Email()
        {
            smtpServer = "mail.mcbrill.com.ar";
            smtpPort = 587/*465*/;
            smtpCredentials = true;
            enableSsl = false/*true*/;
        }

        public Email(string _stmpServer, int _smtpPort, bool _smtpCredentials, bool _enableSsl)
        {
            smtpServer = _stmpServer;
            smtpPort = _smtpPort;
            smtpCredentials = _smtpCredentials;
            enableSsl = _enableSsl;
        }

        public string SendEmail(string emailBody, string from, string user, string password, string To, string subject)
        {
            try
            {
                //Configuring webMail class to send emails  
                //gmail smtp server  
                WebMail.SmtpServer = smtpServer;
                //gmail port to send emails  
                WebMail.SmtpPort = smtpPort;
                WebMail.SmtpUseDefaultCredentials = smtpCredentials;
                //sending emails with secure protocol  
                WebMail.EnableSsl = enableSsl;
                //EmailId used to send emails from application  
                WebMail.UserName = user;
                WebMail.Password = password;

                //Sender email address.  
                WebMail.From = from;

                //Send email  
                WebMail.Send(to: To, subject: subject, body: emailBody, isBodyHtml: true);

                return "Enviado Correctamente";
            }
            catch (Exception e)
            {
                return e.InnerException.Message.ToString();
            }
        }
    }
}
