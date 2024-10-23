using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TesteEnderecoConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string cep = "36418200";  

            try
            {
                var endereco = await ConsultarApiEndereco(cep);

                if (endereco != null)
                {
                    Console.WriteLine("Consulta realizada com sucesso:");
                    Console.WriteLine($"CEP: {endereco.Cep}");
                    Console.WriteLine($"Logradouro: {endereco.Logradouro}");
                    Console.WriteLine($"Bairro: {endereco.Bairro}");
                    Console.WriteLine($"Cidade (localidade): {endereco.Localidade}");
                    Console.WriteLine($"Estado: {endereco.Estado}");
                }
                else
                {
                    Console.WriteLine("Endereço não encontrado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante a consulta: {ex.Message}");
            }

            Console.ReadLine();
        }

        private static async Task<Endereco> ConsultarApiEndereco(string cep)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://viacep.com.br/ws/{cep}/json/";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var endereco = JsonConvert.DeserializeObject<Endereco>(json);

                        if (endereco.Erro == "true")
                        {
                            throw new Exception("API retornou erro: CEP inválido.");
                        }

                        return endereco; 
                    }
                    else
                    {
                        throw new Exception("Falha ao consultar a API de endereço.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro na consulta à API: {ex.Message}");
                }
            }
        }
    }

    public class Endereco
    {
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Localidade { get; set; } 
        public string Estado { get; set; }
        public string Erro { get; set; }        
    }
}
