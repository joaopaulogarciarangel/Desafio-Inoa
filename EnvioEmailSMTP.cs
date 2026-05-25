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
    // Classe responsável por montar e enviar os emails por SMTP
    public class Email
    {
        //Construtor para dados do servidor, remetente e senha
        public string Provedor { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public Email(string provedor, string username, string password)
        {
            Provedor = provedor ?? throw new ArgumentNullException(nameof(provedor));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
        // Método para monstar e em seguida a enviar o email
        public void EnviarEmail( List<string> emailTo, string assunto, string mensagem)
        {
            var mail = PrepararMensagem(emailTo, assunto, mensagem);
            EnviarEmailSMTP(mail);
        }

        // monta o objeto MailMessage com remente, destinatário, assunto e corpo do email
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
            Regex expression = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}"); // Validaçao simples para conferir se o texto tem o formato de email
            if (expression.IsMatch(email))
                return true;

            return false;
        }

        // conecta no servidor SMTP e envia a mensagem de fato
        private void EnviarEmailSMTP(MailMessage mensagem)
        {
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = Provedor; // servidor de saída
                smtp.Port = 587; //Porta de submissão
                smtp.EnableSsl = true;  
                smtp.Timeout = 50000;   // Desiste após 50s sem resposta
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(Username, Password); // Login no SMTP
                smtp.Send(mensagem); // Envia o email
                smtp.Dispose();
            }
        }
    }
}
