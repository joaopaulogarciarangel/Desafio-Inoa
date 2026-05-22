using EnvioEmail;

// para rodar:
// coloque seu e-mail (remetente) no lugar de "seuemail@gmail.com".
// coloque aqui a senha de app do seu e-mail
// No Gmail tem que gerar uma "senha de app em https://myaccount.google.com/apppasswords
// em "emailTo", coloque o e-mail de destino do teste. 

var gmail = new Email("smtp.gmail.com", "seuemail@gmail.com", "senhadeapp");
gmail.EnviarEmail(
    emailTo: new List<string> { "EmailDeDestino@gmail.com" },
    assunto: "Teste",
    mensagem: "Segue a mensagem");