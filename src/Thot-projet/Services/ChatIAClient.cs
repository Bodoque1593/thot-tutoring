using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thot_projet.Models;

namespace Thot_projet.Services
{
    public class ChatIAClient
    {
        private readonly string _baseUrl = "http://localhost:8008";

        public async Task<string> AskAsync(string question)
        {
            using (var client = new HttpClient())
            {
                var requestObj = new ChatIARequest { question = question };

                var json = JsonConvert.SerializeObject(requestObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_baseUrl}/chat", content);

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();

                var responseObj = JsonConvert.DeserializeObject<ChatIAResponse>(responseJson);

                return responseObj?.answer ?? "Sin respuesta del modelo.";
            }
        }
    }
}