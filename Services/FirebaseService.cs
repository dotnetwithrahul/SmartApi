using FirebaseApiMain.Models;
using System.Text;
using System.Text.Json;

namespace FirebaseApiMain.Services
{
    public class FirebaseService
    {
        static string firebaseDatabaseUrl = "https://fir-db-2f8d5-default-rtdb.firebaseio.com/";
        static string firebaseDatabaseDocument = "products";
        static readonly HttpClient client = new HttpClient();


        public async Task<List<Rahul>> GetAll()
        {
            string url = $"{firebaseDatabaseUrl}" +
                         $"{firebaseDatabaseDocument}.json";

            var httpResponseMessage = await client.GetAsync(url);
            List<Rahul> entries = new List<Rahul>();

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(contentStream) && contentStream != "null")
                {
                    try
                    {
                        // Attempt to deserialize as a single Rahul object
                        var singleResult = JsonSerializer.Deserialize<Rahul>(contentStream);
                        if (singleResult != null)
                        {
                            entries.Add(singleResult);
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Handle or log the error as needed
                        throw new Exception("Deserialization failed", ex);
                    }
                }
            }

            return entries;
        }





        public async Task<Dictionary<string, Product>> GetAllProducts()
        {
            string url = $"{firebaseDatabaseUrl}{firebaseDatabaseDocument}.json";
            var httpResponseMessage = await client.GetAsync(url);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(contentStream) && contentStream != "null")
                {
                    try
                    {
                        // Deserialize the JSON response to a dictionary of products
                        var products = JsonSerializer.Deserialize<Dictionary<string, Product>>(contentStream);
                        return products ?? new Dictionary<string, Product>();
                    }
                    catch (JsonException ex)
                    {
                        // Handle or log the error as needed
                        throw new Exception("Deserialization failed", ex);
                    }
                }
            }

            return new Dictionary<string, Product>();
        }



        public async Task AddProductAsync(string productId, Product product)
        {
            string url = $"{firebaseDatabaseUrl}{firebaseDatabaseDocument}/{productId}.json";

            var json = JsonSerializer.Serialize(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                // Handle error response
                throw new Exception($"Failed to add product: {response.ReasonPhrase}");
            }
        }



        public async Task UpdateProductAsync(string productId, Product product)
        {
            string url = $"{firebaseDatabaseUrl}{firebaseDatabaseDocument}/{productId}.json";

            var json = JsonSerializer.Serialize(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                // Handle error response
                throw new Exception($"Failed to update product: {response.ReasonPhrase}");
            }
        }




        public async Task DeleteProductAsync(string productId)
        {
            string url = $"{firebaseDatabaseUrl}{firebaseDatabaseDocument}/{productId}.json";

            var response = await client.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Handle error response
                throw new Exception($"Failed to delete product: {response.ReasonPhrase}");
            }
        }

    }
}
