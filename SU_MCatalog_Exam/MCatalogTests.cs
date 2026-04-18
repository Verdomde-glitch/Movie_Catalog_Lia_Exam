using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using SU_MCatalog_Exam.Models;




namespace SU_MCatalog_Exam
{
    [TestFixture]
    public class MCatalogTests
    {
        private RestClient client;

        private static string lastCreatedMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJjYTE5Y2Q5My00N2VhLTRmZWEtYTkyYy03MDhkNjBkNTQ3ZGEiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjIwOjE2IiwiVXNlcklkIjoiMjlkMDY3ZjEtOTI0YS00ZDhjLTYyNDctMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJleGFtTGlhQGV4YW1wbGUuY29tIiwiVXNlck5hbWUiOiJsaWFFeGFtIiwiZXhwIjoxNzc2NTE0ODE2LCJpc3MiOiJNb3ZpZUNhdGFsb2dfQXBwX1NvZnRVbmkiLCJhdWQiOiJNb3ZpZUNhdGFsb2dfV2ViQVBJX1NvZnRVbmkifQ.VP-TJXbecb9cDk2DMOq_O2mdaz9jJ1xxl9PTBH4a06Q";
        private const string LogInMail = "examLia@example.com";
        private const string LogInPassword = "liaExam";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrEmpty(StaticToken))
            {

                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LogInMail, LogInPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private static string GetJwtToken(string email, string password)
        {

            var temClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new {email, password });

            var response = temClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("JWT token is null or empty.");
                }
                return token;
            }
            else
            {
                throw new Exception($"Failed to get JWT token. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }


        [Order(1)]
            [Test]
        public void CreateNewIdea_WithRequiredData_ShouldReternSuccess()
        {
            var movieData = new MovieDTO
            {
                Title = "Test Movie",
                Description = "This is a test movie."
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),"Expected Status code 200 OK ");
            Assert.That(createResponse.Msg, Is.EqualTo("Movie created successfully!"));
            Assert.That(createResponse.Movie, Is.Not.Null);
            Assert.That(createResponse.Movie.Id, Is.Not.Null.And.Not.Empty);

            lastCreatedMovieId = createResponse.Movie.Id;
        }

        [Order(2)]
        [Test]
        public void EditExistingIdea_WithRequiredData_ShouldReternSuccess()
        {
            var editMovieData = new MovieDTO
            {
                Title = "Editted Movie",
                Description = "This is a Eddited movie."
            };
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastCreatedMovieId);
            request.AddJsonBody(editMovieData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status code 200 OK ");
            Assert.That(editResponse.Msg, Is.EqualTo("Movie edited successfully!"));
            
        }

        [Order(3)]
        [Test]
        public void GetAllIdeas_ShouldReternSuccess()
        {
           
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize <List< ApiResponseDTO >>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status code 200 OK ");
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);

        }

        [Order(4)]
        [Test]
        public void DeleteLastCreatedIdea_ShouldReternSuccess()
        {

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastCreatedMovieId);
            var response = this.client.Execute(request);
            var deletedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected Status code 200 OK ");
            Assert.That(deletedResponse.Msg, Is.EqualTo("Movie deleted successfully!"));


        }

        [Order(5)]
        [Test]

        public void CreateNewMovie_WithoutRequiredData_ShouldReternBadRequest()
        {
            var movieData = new MovieDTO
            {
                Title = "",
                Description = "This is a test movie without title."
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);
            var response = this.client.Execute(request);

            var badResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected Status code 400 Bad Request ");
        }


        [Order(6)]
        [Test]
        public void EditNonExistingMovie_WithRequiredData_ShouldReternSuccess()
        {
            var nonExistingMovieId = "non-existing-id";
            var editMovieData = new MovieDTO
            {
                Title = "Editted non-existing Movie",
                Description = "This is not a Eddited movie."
            };
            var request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(editMovieData);

            var response = this.client.Execute(request);

            var editBadResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected Status code 400 Bad Request");
            Assert.That(editBadResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

            [Order(7)]
        [Test]

        public void DeleteNonExistingMovie_ShouldReternBadRequest()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "non-existing-id");

            var response = this.client.Execute(request);

            var badResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected Status code 400 Bad Request ");
        }

        [OneTimeTearDown]
        
        public void TearDown()
        {
            this.client.Dispose();
        }
    }
 }