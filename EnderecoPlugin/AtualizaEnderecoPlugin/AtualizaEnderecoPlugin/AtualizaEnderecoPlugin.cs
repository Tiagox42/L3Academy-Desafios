using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace AtualizaEnderecoPlugin
{
    public class AtualizaEndereco : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
            {
 
                if (entity.Attributes.Contains("address1_postalcode"))
                {
                    string cep = entity.GetAttributeValue<string>("address1_postalcode");

                    Task.Run(async () =>
                    {
                        var endereco = await ConsultarApiEndereco(cep);
                        if (endereco != null)
                        {

                            if (!string.IsNullOrWhiteSpace(endereco.Bairro))
                                entity["address1_neighborhood"] = endereco.Bairro;

                            if (!string.IsNullOrWhiteSpace(endereco.Localidade))
                                entity["address1_city"] = endereco.Localidade;

                            if (!string.IsNullOrWhiteSpace(endereco.Estado))
                                entity["address1_stateorprovince"] = endereco.Estado;
                        }
                    }).Wait();
                }
            }
        }

        private async Task<Endereco> ConsultarApiEndereco(string cep)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://api.generica.com/endereco/{cep}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var endereco = JsonConvert.DeserializeObject<Endereco>(json);

                        if (endereco.Erro == "true")
                        {
                            throw new InvalidPluginExecutionException("Erro na consulta de endereço: API retornou erro.");
                        }

                        return endereco;
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("Falha ao consultar a API de endereço.");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException($"Erro na consulta à API de endereço: {ex.Message}");
                }
            }
        }
    }
}
