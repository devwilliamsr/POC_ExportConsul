using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();

        #region ESTACIO PRD
        string url = "http://10.130.100.131:8500/v1/kv/";
        string path = "estacio";
        string urlDestino = "http://10.130.100.131:8500/v1/kv/"; //As vezes temos a necessidade de buscar em PRD para gerar o arquivo de bkp de dev e/ou hml, por isso a variável
        string nomeDoArquivo = $"popula-consul-PRD-{path}-26-04-2024.sh";
        #endregion

        var request = new HttpRequestMessage(HttpMethod.Get, $"{url}{path}?recurse=true");

        try
        {
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                var objetoDesserializado = JsonSerializer.Deserialize<ResponseModel[]>(jsonResponse);

                var comandoCurl = new StringBuilder();

                foreach (var item in objetoDesserializado)
                {
                    string chave = item.Key;
                    Console.WriteLine($"{chave}");
                    if(item.Value != null)
                    {
                        string valor = DecodificarBase64(item.Value);

                        comandoCurl.AppendLine("curl --request PUT \\");
                        comandoCurl.AppendLine($"  --url \"{urlDestino}{chave}?=\" \\");
                        comandoCurl.AppendLine("  --header 'Content-Type: application/json' \\");
                        comandoCurl.AppendLine($"  --data '{valor}'");
                        comandoCurl.AppendLine("");
                    }
                }

                System.IO.File.AppendAllText(nomeDoArquivo, comandoCurl.ToString());
            }
            else
            {
                Console.WriteLine($"Falha ao acionar a API. Status Code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Exception: {ex.Message}");
        }

    }

    static string DecodificarBase64(string base64)
    {
        byte[] data = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(data);
    }

    public class ResponseModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}