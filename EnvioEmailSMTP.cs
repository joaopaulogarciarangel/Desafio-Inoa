using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;                       
using System.Net.Mail;                  
using System.Text.RegularExpressions;   

namespace EnvioEmail
{
    public class Email
    {
        public string Provedor { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public Email(string provedor, string username, string password)
        {
            Provedor = provedor ?? throw new ArgumentNullException(nameof(provedor));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
        public void EnviarEmail( List<string> emailTo, string assunto, string mensagem)
        {
            var mail = PrepararMensagem(emailTo, assunto, mensagem);
            EnviarEmailSMTP(mail);
        }

        private MailMessage PrepararMensagem(List<string> emailTo, string assunto, string mensagem)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(Username);

            foreach (var email in emailTo)
            {
                if(ValidarEmail(email))
                {
                mail.To.Add(email);
                }
            }

            mail.Subject = assunto;
            mail.Body = mensagem;
            mail.IsBodyHtml = true;

            return mail;
        }

        private bool ValidarEmail(string email)
        {
            Regex expression = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");
            if (expression.IsMatch(email))
                return true;

            return false;
        }

        private void EnviarEmailSMTP(MailMessage mensagem)
        {
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = Provedor;
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Timeout = 50000;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(Username, Password);
                smtp.Send(mensagem);
                smtp.Dispose();
            }
        }
    }
}
