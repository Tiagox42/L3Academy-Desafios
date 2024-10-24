using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.ConsultaCep
{
    public class ConsultaCep : IPlugin
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3),
        };

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity entity = (Entity)context.InputParameters["Target"];

            string cep = entity["address1_postalcode"].ToString();
            if (!ValidaCep(cep, out cep))
            {
                return;
            }

            Endereco endereco = BuscarCep(cep).GetAwaiter().GetResult();
            if (endereco == null)
            {
                tracingService.Trace($"[Erro][Plugin - ConsultaCep] {cep} não encontrado nas apis.");
                return;
            }

            try
            {
                if (entity.LogicalName == "lead" || entity.LogicalName == "contact" || entity.LogicalName == "account")
                {
                    entity["address1_postalcode"] = endereco.Cep;
                    entity["address1_line1"] = endereco.Rua;
                    entity["address1_line3"] = endereco.Bairro;
                    entity["address1_city"] = endereco.Cidade;
                    entity["address1_stateorprovince"] = endereco.Estado;
                    entity["address1_country"] = endereco.Regiao;

                    service.Update(entity);
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"[Erro][Plugin - ConsultaCep] Não foi possível atualizar o registro {entity.LogicalName}," +
                    $" ID:{entity.Id}. " +
                    $"Detalhes: " + ex);
                throw;
            }

        }

        public bool ValidaCep(string cep, out string cepFormatado)
        {
            cepFormatado = cep.Replace("-", "").Replace(" ", "");
            if (cepFormatado.Length == 8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Endereco> BuscarCep(string cep)
        {
            Func<string, Task<Endereco>>[] apis = { ViaCep, OpenCep, BrasilApi };

            foreach (var buscar in apis)
            {
                Endereco endereco = await buscar(cep);
                if (endereco != null)
                {
                    return endereco;
                }
            }

            return null;
        }

        //Prioridade
        public async Task<Endereco> ViaCep(string cep)
        {
            try
            {
                string url = $"https://viacep.com.br/ws/{cep}/json/";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();

                    Endereco_ViaCep objeto = JsonConvert.DeserializeObject<Endereco_ViaCep>(resultado);

                    if (objeto.erro == "true") {
                        return null;
                    } else
                    {
                        Endereco endereco = new Endereco();
                        endereco.Cep = objeto.cep;
                        endereco.Rua = objeto.logradouro;
                        endereco.Bairro = objeto.bairro;
                        endereco.Cidade = objeto.localidade;
                        endereco.Estado = objeto.estado;
                        endereco.Regiao = objeto.regiao;

                        return endereco;
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        //Consulta todos, ficar por último
        public async Task<Endereco> BrasilApi(string cep)
        {
            try
            {
                string url = $"https://brasilapi.com.br/api/cep/v1/{cep}";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();

                    Endereco_BrasilApi objeto = JsonConvert.DeserializeObject<Endereco_BrasilApi>(resultado);
                    Endereco endereco = new Endereco();
                    endereco.Cep = objeto.cep;
                    endereco.Rua = objeto.street;
                    endereco.Bairro = objeto.neighborhood;
                    endereco.Cidade = objeto.city;
                    string[] partes = Regiao(objeto.state).Split(',');
                    endereco.Estado = partes[0];
                    endereco.Regiao = partes[1];

                    return endereco;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        //Do github
        public async Task<Endereco> OpenCep(string cep)
        {
            try
            {
                string url = $"https://opencep.com/v1/{cep}";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string resultado = await response.Content.ReadAsStringAsync();

                    Endereco_OpenCep objeto = JsonConvert.DeserializeObject<Endereco_OpenCep>(resultado);
                    Endereco endereco = new Endereco();
                    endereco.Cep = objeto.cep;
                    endereco.Rua = objeto.logradouro;
                    endereco.Bairro = objeto.bairro;
                    endereco.Cidade = objeto.localidade;
                    string[] partes = Regiao(objeto.uf).Split(',');
                    endereco.Estado = partes[0];
                    endereco.Regiao = partes[1];

                    return endereco;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public string Regiao(string uf)
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
            { "TO", Tuple.Create("Tocantins", "Norte") },
            };

            if (estados.TryGetValue(uf, out var estado))
            {
                return $"{estado.Item1},{estado.Item2}";
            }
            else
            {
                return $"{uf},Desconhecido";
            }
        }
    }

    #region Classes Usadas

    public class Endereco
    {
        public string Cep { get; set; } //address1_postalcode
        public string Rua { get; set; } //address1_line1
        public string Bairro { get; set; } //address1_line3
        public string Cidade { get; set; } //address1_city
        public string Estado { get; set; } //address1_stateorprovince
        public string Regiao { get; set; } //address1_country ex: Sudeste ou Brasil
    }
    public class Endereco_ViaCep
    {
        public string cep;
        public string logradouro;
        public string complemento { get; set; }
        public string unidade { get; set; }
        public string bairro { get; set; }
        public string localidade { get; set; }
        public string uf { get; set; }
        public string estado { get; set; }
        public string regiao { get; set; }
        public string ibge { get; set; }
        public string gia { get; set; }
        public string ddd { get; set; }
        public string siafi { get; set; }
        public string erro { get; set; }

    }
    public class Endereco_OpenCep
    {
        public string cep;
        public string logradouro;
        public string complemento { get; set; }
        public string bairro { get; set; }
        public string localidade { get; set; }
        public string uf { get; set; }
        public string ibge { get; set; }
    }
    public class Endereco_BrasilApi
    {
        public string cep;
        public string state;
        public string city { get; set; }
        public string neighborhood { get; set; }
        public string street { get; set; }
        public string service { get; set; }
    }
    #endregion
}
