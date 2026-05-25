using System.Text.Json;
using EnvioEmail;

// para rodar:
// 1) preencha o config.json com o e-mail de destino e os dados do SMTP do remetente
//    No Gmail, gere uma "senha de app" em https://myaccount.google.com/apppasswords e use-a no campo SmtpSenha.
// 2) chame o programa passando 3 parâmetros: o ativo, o preço de venda e o de compra.
//    dotnet run -- PETR4 22,67 22,59

// lê os 3 parâmetros vindos da linha de comando 
// Sem exatamente 3, não há o que monitorar: mostra o uso e encerra. 
if (args.Length != 3)
{
    Console.WriteLine("Uso: dotnet run -- <ativo> <preço de venda> <preço de compra>");
    Console.WriteLine("Exemplo: dotnet run -- PETR4 22,67 22,59");
    return;
}

// Lê o primeiro parâmetro do console e coloca em maiúsculas para a API
string ativo = args[0].ToUpper();

//Convertendo os valores recebidos no console para executar o programa em decimal
decimal precoVenda = decimal.Parse(args[1]);
decimal precoCompra = decimal.Parse(args[2]);

// A linha de venda fica acima da de compra, se vier invertido a lógica falha
if (precoVenda <= precoCompra)
{
    Console.WriteLine("Erro: O preço de venda deve ser maior que o preço de compra");
    return;
}

// lê o arquivo de configuração 
var config = JsonSerializer.Deserialize<Configuracao>(File.ReadAllText("config.json"));

// Instancia a classe de envio de emails usando os dados do json
var gmail = new Email(config.SmtpHost, config.SmtpUsuario, config.SmtpSenha);

// Serviço que consulta a cotação atual do ativo na API
var cotacao = new ServicoCotacao(config.TokenBrapi);

//Flag para evitar spam; Quando o preço está na zona neutra e flag = true e ultrapassa uma das fronteiras, envia o email e flag = false
// Só volta a ser true quando voltar a zona neutra de novo
bool alertaArmado = true;

Console.WriteLine($"Monitorando {ativo} / vender acima de {precoVenda} / comprar abaixo de {precoCompra}");

// loop contínuo de monitoramento
while (true)
{
    // Pega o preço atual da API e mostra no console como está a operação
    decimal preco = await cotacao.ObterPreco(ativo);
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {ativo} = {preco}");

    // O preço está na zona neutra quando fica entre as duas linhas dados pelo usuário
    bool naZonaNeutra = preco <= precoVenda && preco >= precoCompra;

    if (naZonaNeutra)
    {
        //entre as linhas: rearma a flag para o próximo cruzamento
        alertaArmado = true;
    }
    else if (alertaArmado)
    {
        // Saiu da zona neutra e a flag está armada: manda o email e deixa a flag= false
        if (preco > precoVenda)
        {
            // Acima da linha de venda -> aconselha vender
            gmail.EnviarEmail(
                emailTo: new List<string> { config.EmailDestino },
                assunto: $"[Alerta!] Vender {ativo}",
                mensagem: $"{ativo} está a R$ {preco} (acima de R$ {precoVenda}). Sugestão: VENDER!");
            Console.WriteLine("email de venda enviado");
        }
        else
        {
            // Abaixo da linha de compra: aconselha comprar
            gmail.EnviarEmail(
                emailTo: new List<string> { config.EmailDestino },
                assunto: $"[Alerta!] Comprar {ativo}",
                mensagem: $"{ativo} está a R$ {preco} (abaixo de R$ {precoCompra}). Sugestão: COMPRAR!");
            Console.WriteLine("email de compra enviado");
        }
        alertaArmado = false; // não envia outro email até voltar para a zona neutra
    }

    // coloquei rate limit porque estava fazendo requisição a cada segundo e fiquei com medo da API bloquear
    await Task.Delay(config.IntervaloSegundos * 1000);
}

// Espelha o config.json 
record Configuracao(
    string EmailDestino,
    string SmtpHost,
    string SmtpUsuario,
    string SmtpSenha,
    string TokenBrapi,
    int IntervaloSegundos
);