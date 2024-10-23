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
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity entity = (Entity)context.InputParameters["Target"];

            if (entity.LogicalName == "lead")
            {
                string cep = entity["new_cep"].ToString();
                if (!ValidaCep(cep, out cep))
                {
                    return;
                }
                else
                {
                    Endereco endereco = BuscarCep(cep);

                    if (endereco != null)
                    {

                    }
                    else
                    {
                        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                        tracingService.Trace($"[Erro][Plugin - ConsultaCep] {cep} não encontrado nas apis.");
                    }
                }


            }

            if (entity.LogicalName == "contact")
            {

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

        public Endereco BuscarCep(string cep)
        {
            Func<string, Endereco>[] apis = { ViaCep, OpenCep, BrasilApi };

            foreach (var buscar in apis)
            {
                Endereco endereco = buscar(cep);
                if (endereco != null)
                {
                    return endereco;
                }
            }

            return null;
        }

        //Prioridade
        public Endereco ViaCep(string cep)
        {
            string url = $"https://viacep.com.br/ws/{cep}/json/";

            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                string resultado = response.Content.ReadAsStringAsync().Result;

                Endereco_ViaCep objeto = JsonConvert.DeserializeObject<Endereco_ViaCep>(resultado);
                Endereco endereco = new Endereco();
                endereco.Cep = objeto.cep;
                endereco.Rua = objeto.logradouro;
                endereco.Bairro = objeto.bairro;
                endereco.Cidade = objeto.localidade;
                endereco.Estado = objeto.estado;
                endereco.Regiao = objeto.regiao;

                return endereco;
            }
            else
            {
                return null;
            }
        }

        //Consulta todos, ficar por último
        public Endereco BrasilApi(string cep)
        {
            string url = $"https://brasilapi.com.br/api/cep/v1/{cep}";

            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                string resultado = response.Content.ReadAsStringAsync().Result;

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
            else
            {
                return null;
            }
        }

        //Do github
        public Endereco OpenCep(string cep)
        {
            string url = $"https://opencep.com/v1/{cep}";

            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                string resultado = response.Content.ReadAsStringAsync().Result;

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
            else
            {
                return null;
            }
        }

        public string Regiao(string uf)
        {
            uf = uf.Trim().ToUpper();
            var estados = new Dictionary<string, (string NomeEstado, string Regiao)>
            {
            { "AC", ("Acre", "Norte") },
            { "AL", ("Alagoas", "Nordeste") },
            { "AP", ("Amapá", "Norte") },
            { "AM", ("Amazonas", "Norte") },
            { "BA", ("Bahia", "Nordeste") },
            { "CE", ("Ceará", "Nordeste") },
            { "DF", ("Distrito Federal", "Centro-Oeste") },
            { "ES", ("Espírito Santo", "Sudeste") },
            { "GO", ("Goiás", "Centro-Oeste") },
            { "MA", ("Maranhão", "Nordeste") },
            { "MT", ("Mato Grosso", "Centro-Oeste") },
            { "MS", ("Mato Grosso do Sul", "Centro-Oeste") },
            { "MG", ("Minas Gerais", "Sudeste") },
            { "PA", ("Pará", "Norte") },
            { "PB", ("Paraíba", "Nordeste") },
            { "PR", ("Paraná", "Sul") },
            { "PE", ("Pernambuco", "Nordeste") },
            { "PI", ("Piauí", "Nordeste") },
            { "RJ", ("Rio de Janeiro", "Sudeste") },
            { "RN", ("Rio Grande do Norte", "Nordeste") },
            { "RS", ("Rio Grande do Sul", "Sul") },
            { "RO", ("Rondônia", "Norte") },
            { "RR", ("Roraima", "Norte") },
            { "SC", ("Santa Catarina", "Sul") },
            { "SP", ("São Paulo", "Sudeste") },
            { "SE", ("Sergipe", "Nordeste") },
            { "TO", ("Tocantins", "Norte") },
            };

            if (estados.TryGetValue(uf, out var estado))
            {
                return $"{estado.NomeEstado},{estado.Regiao}";
            } else
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
