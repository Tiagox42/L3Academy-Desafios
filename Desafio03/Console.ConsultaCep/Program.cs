
using Newtonsoft.Json;

namespace ConsoleApp.ConsultaCep
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3),
        };

        static async Task Main(string[] args)
        {
            Console.WriteLine("Digite o CEP para consulta:");
            string cep = Console.ReadLine();

            if (!ValidaCep(cep, out string cepFormatado))
            {
                Console.WriteLine("CEP inválido!");
                return;
            }

            Console.WriteLine("\n--- Testando ViaCep ---");
            Endereco enderecoViaCep = await ViaCep(cepFormatado);
            ExibirEndereco(enderecoViaCep, "ViaCep");

            Console.WriteLine("\n--- Testando BrasilApi ---");
            Endereco enderecoBrasilApi = await BrasilApi(cepFormatado);
            ExibirEndereco(enderecoBrasilApi, "BrasilApi");

            Console.WriteLine("\n--- Testando OpenCep ---");
            Endereco enderecoOpenCep = await OpenCep(cepFormatado);
            ExibirEndereco(enderecoOpenCep, "OpenCep");
        }

        public static bool ValidaCep(string cep, out string cepFormatado)
        {
            cepFormatado = cep.Replace("-", "").Replace(" ", "");
            return cepFormatado.Length == 8;
        }

        public static void ExibirEndereco(Endereco endereco, string api)
        {
            if (endereco != null)
            {
                Console.WriteLine($"[{api}] CEP encontrado!");
                Console.WriteLine($"CEP: {endereco.Cep}");
                Console.WriteLine($"Rua: {endereco.Rua}");
                Console.WriteLine($"Bairro: {endereco.Bairro}");
                Console.WriteLine($"Cidade: {endereco.Cidade}");
                Console.WriteLine($"Estado: {endereco.Estado}");
                Console.WriteLine($"Região: {endereco.Regiao}");
            }
            else
            {
                Console.WriteLine($"[{api}] CEP não encontrado ou erro na consulta.");
            }
        }
        public static async Task<Endereco> ViaCep(string cep)
        {
            try
            {
                string url = $"https://viacep.com.br/ws/{cep}/json/";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    Endereco_ViaCep objeto = JsonConvert.DeserializeObject<Endereco_ViaCep>(resultado);

                    var regiaoInfo = Regiao(objeto.uf);

                    return new Endereco
                    {
                        Cep = objeto.cep,
                        Rua = objeto.logradouro,
                        Bairro = objeto.bairro,
                        Cidade = objeto.localidade,
                        Estado = regiaoInfo.Item1,
                        Regiao = regiaoInfo.Item2
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar a ViaCep: {ex.Message}");
            }
            return null;
        }


        public static async Task<Endereco> BrasilApi(string cep)
        {
            try
            {
                string url = $"https://brasilapi.com.br/api/cep/v1/{cep}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    Endereco_BrasilApi objeto = JsonConvert.DeserializeObject<Endereco_BrasilApi>(resultado);

                    var regiaoInfo = Regiao(objeto.state);

                    return new Endereco
                    {
                        Cep = objeto.cep,
                        Rua = objeto.street,
                        Bairro = objeto.neighborhood,
                        Cidade = objeto.city,
                        Estado = regiaoInfo.Item1,
                        Regiao = regiaoInfo.Item2
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar a BrasilApi: {ex.Message}");
            }
            return null;
        }

        public static async Task<Endereco> OpenCep(string cep)
        {
            try
            {
                string url = $"https://opencep.com/v1/{cep}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();
                    Endereco_OpenCep objeto = JsonConvert.DeserializeObject<Endereco_OpenCep>(resultado);

                    var regiaoInfo = Regiao(objeto.uf);

                    return new Endereco
                    {
                        Cep = objeto.cep,
                        Rua = objeto.logradouro,
                        Bairro = objeto.bairro,
                        Cidade = objeto.localidade,
                        Estado = regiaoInfo.Item1,
                        Regiao = regiaoInfo.Item2
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar a OpenCep: {ex.Message}");
            }
            return null;
        }

        public static Tuple<string, string> Regiao(string uf)
        {
            uf = uf.Trim().ToUpper();

            var estados = new Dictionary<string, Tuple<string, string>>
            {
                { "AC", Tuple.Create("Acre", "Norte") },
                { "AL", Tuple.Create("Alagoas", "Nordeste") },
                { "AP", Tuple.Create("Amapá", "Norte") },
                { "AM", Tuple.Create("Amazonas", "Norte") },
                { "BA", Tuple.Create("Bahia", "Nordeste") },
                { "CE", Tuple.Create("Ceará", "Nordeste") },
                { "DF", Tuple.Create("Distrito Federal", "Centro-Oeste") },
                { "ES", Tuple.Create("Espírito Santo", "Sudeste") },
                { "GO", Tuple.Create("Goiás", "Centro-Oeste") },
                { "MA", Tuple.Create("Maranhão", "Nordeste") },
                { "MT", Tuple.Create("Mato Grosso", "Centro-Oeste") },
                { "MS", Tuple.Create("Mato Grosso do Sul", "Centro-Oeste") },
                { "MG", Tuple.Create("Minas Gerais", "Sudeste") },
                { "PA", Tuple.Create("Pará", "Norte") },
                { "PB", Tuple.Create("Paraíba", "Nordeste") },
                { "PR", Tuple.Create("Paraná", "Sul") },
                { "PE", Tuple.Create("Pernambuco", "Nordeste") },
                { "PI", Tuple.Create("Piauí", "Nordeste") },
                { "RJ", Tuple.Create("Rio de Janeiro", "Sudeste") },
                { "RN", Tuple.Create("Rio Grande do Norte", "Nordeste") },
                { "RS", Tuple.Create("Rio Grande do Sul", "Sul") },
                { "RO", Tuple.Create("Rondônia", "Norte") },
                { "RR", Tuple.Create("Roraima", "Norte") },
                { "SC", Tuple.Create("Santa Catarina", "Sul") },
                { "SP", Tuple.Create("São Paulo", "Sudeste") },
                { "SE", Tuple.Create("Sergipe", "Nordeste") },
                { "TO", Tuple.Create("Tocantins", "Norte") }
            };

            return estados.ContainsKey(uf) ? estados[uf] : Tuple.Create(uf, "Desconhecido");
        }
    }

    public class Endereco
    {
        public string Cep { get; set; }
        public string Rua { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Regiao { get; set; }
    }

    public class Endereco_ViaCep
    {
        public string cep { get; set; }
        public string logradouro { get; set; }
        public string bairro { get; set; }
        public string localidade { get; set; }
        public string uf { get; set; }
    }

    public class Endereco_BrasilApi
    {
        public string cep { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string neighborhood { get; set; }
        public string street { get; set; }
    }

    public class Endereco_OpenCep
    {
        public string cep { get; set; }
        public string logradouro { get; set; }
        public string bairro { get; set; }
        public string localidade { get; set; }
        public string uf { get; set; }
    }
}

