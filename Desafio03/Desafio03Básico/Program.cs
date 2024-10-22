// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;

Console.WriteLine("-- CONSULTA VIA CEP --");
Console.Write("Digite seu cep: ");

string cep = Console.ReadLine();

string url = $"https://viacep.com.br/ws/{cep}/json/";

HttpClient client = new HttpClient();

HttpResponseMessage response = client.GetAsync(url).Result;

if (response.IsSuccessStatusCode)
{
    string resultado = response.Content.ReadAsStringAsync().Result;

    var objeto = JsonConvert.DeserializeObject<Endereco>(resultado);

    Console.WriteLine(objeto);
}

public class Endereco
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

    public override string ToString()
    {
        return $"CEP Consultado: {cep}\nLogradouro: {logradouro}\nComplemento: {complemento}\nUnidade: {unidade}" +
            $"\nBairro: {bairro}\nLocalidade: {localidade}\nUF: {uf}\nEstado: {estado}\nRegião: {regiao}\nIBGE: {ibge}\nGIA: {gia}" +
            $"\nDDD: {ddd}\nSiafi: {siafi}";

    }
}