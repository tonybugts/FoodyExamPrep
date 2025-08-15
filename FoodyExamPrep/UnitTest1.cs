using FoodyExamPrep.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;


namespace FoodyExamPrep
{
    [TestFixture]
    public class FoodyTests
    {
        
        
        private RestClient client;
        private static string createdFoodId;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

       
        
        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("antrg", "e9e6751e");
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }
        private string GetJwtToken(string username, string password) 
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }


        [Test, Order(1)]
        public void CreateNewFoodWithValidCredentials()
        {
            var food = new FoodDTO()
            {
                Name = "New Food",
                Description = "Delicious Food",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdFoodId = json.GetProperty("foodId").GetString();

        }

        [Test,Order(2)]
        public void EditTitleOfTheCreatedFood()
        {
            var changes = new[]
            {
                new{path = "/name",
                op = "replace",
                value = "updatedNAme"
            }

            };

            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);
            request.AddJsonBody(changes);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));


        }

        [Test,Order(3)]
        public void GetAllFoods()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            var foods = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(foods, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteFood()
        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test, Order(5)]
        public void CreateNewFoodWithInvalidCredentials()
        {
            var food = new FoodDTO()
            {
                Name = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }
        [Test, Order(6)]
        public void EditNonExistingFood()
        {
            string fakeId = "000000";
            var changes = new[]
            {
            new { path = "/name" , op = "replace", value =  "new Tilte"}
            };
            var request = new RestRequest($"/api/Food/Edit/{fakeId}", Method.Patch);
            request.AddJsonBody(changes);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingFood()
        {
            string fakeId = "000000";
            var request = new RestRequest($"/api/Food/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));
        }

        [OneTimeTearDown]
        public void CleanUp() 
        {
            client?.Dispose();
        }
    }
}