using System.Text.Json;

// Serviço responsável por buscar a cotação de um ativo da B3
// Escolhi a API da brapi.dev porque é gratuita, fácil de usar e nativa para a bolsa brasileira
public class ServicoCotacao
{
    private static HttpClient _http = new HttpClient();

    private readonly string _token; // token gratuito obtido no site da brapi.dev

    public ServicoCotacao(string token) => _token = token;

    public async Task<decimal> ObterPreco(string ativo)
    {
        // Endpoint da brapi
        string url = $"https://brapi.dev/api/quote/{ativo}?token={_token}";

        // utiliza o GET e lê o corpo da resposta como texto
        string json = await _http.GetStringAsync(url);

        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("results")[0]
            .GetProperty("regularMarketPrice")
            .GetDecimal();
    }
}