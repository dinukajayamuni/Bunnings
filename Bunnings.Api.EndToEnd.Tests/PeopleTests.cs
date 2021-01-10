using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bunnings.Api.Models;
using NSubstitute;
using TestStack.BDDfy;
using Xunit;

namespace Bunnings.Api.EndToEnd.Tests
{
    public class PeopleTests : IClassFixture<BunningsWebApplicationFactory<Startup>>
    {
        private readonly BunningsWebApplicationFactory<Startup> _factory;
        private readonly Person _person;
        private HttpResponseMessage _response;

        public PeopleTests(BunningsWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _person = new Person
            {
                Email = "john.snow@test.com",
                FirstName = "John",
                LastName = "Snow",
                Gender = Gender.Male,
                Pets = new[] { new Pet { Name = "Brody", Type = PetType.Dog } }
            };
        }

        [Fact]
        public void APersonIsSavedToThePeopleStoreWhenAValidCreatePersonRequestIsReceived()
        {
            this.Given(_ => JohnDoeNotExistInThePeopleStore())
                .When(_ => JohnIsRequestedToBeCreated())
                .Then(_ => JohnIsCreated());
        }

        [Fact]
        public void AConflictIsReturnedWhenAValidCreatePersonRequestIsReceivedForAnExistingPerson()
        {
            this.Given(_ => JohnExistsInThePeopleStore())
                .When(_ => JohnIsRequestedToBeCreated())
                .Then(_ => ConflictIsReturned());
        }

        [Fact]
        public void AValidationErrorIsReturnedWhenACreatePersonRequestIsReceivedWithoutEmail()
        {
            this.Given(_ => JohnDoeNotExistInThePeopleStore())
                .When(_ => JohnIsRequestedToBeCreatedWithoutEmail())
                .Then(_ => ValidationErrorIsReturned());
        }

        [Fact]
        public void AValidationErrorIsReturnedWhenACreatePersonRequestIsReceivedWithAnInvalidEmail()
        {
            this.Given(_ => JohnDoeNotExistInThePeopleStore())
                .When(_ => JohnIsRequestedToBeCreatedWithAnInvalidEmail())
                .Then(_ => ValidationErrorIsReturned());
        }

        // TODO: More validation tests

        [Fact]
        public void APersonIsSavedToThePeopleStoreWhenAValidCreatePersonRequestIsReceivedWithInvalidApiKey()
        {
            this.Given(_ => JohnDoeNotExistInThePeopleStore())
                .When(_ => JohnIsRequestedToBeCreatedWithInvalidApiKeyTask())
                .Then(_ => UnauthorizedIsReturned());
        }

        [Fact]
        public void APersonIsReturnedWhenThePersonExistsInThePeopleStore()
        {
            this.Given(_ => JohnExistsInThePeopleStore())
                .When(_ => JohnIsQueried())
                .Then(_ => JohnIsReturned());
        }

        [Fact]
        public void ResourceNotFoundIsReturnedWhenThePersonDoesNotExistInThePeopleStore()
        {
            this.Given(_ => JohnDoeNotExistInThePeopleStore())
                .When(_ => JohnIsQueried())
                .Then(_ => ResourceNotFoundIsReturned());
        }

        [Fact]
        public void UnauthorizedIsReturnedWhenThePersonDoesNotExistInThePeopleStore()
        {
            this.Given(_ => JohnDoeNotExistInThePeopleStore())
                .When(_ => JohnIsQueriedWithNoApiKey())
                .Then(_ => UnauthorizedIsReturned());
        }

        private void JohnExistsInThePeopleStore()
        {
            _factory.BlobClient.ExistsAsync().Returns(Task.FromResult(Azure.Response.FromValue(true, null)));

            _factory.BlobClient.DownloadToAsync(Arg.Any<Stream>()).Returns(info =>
            {
                var stream = info.Arg<Stream>();
                using var personStream = new MemoryStream();
                JsonSerializer.SerializeAsync(personStream, _person).GetAwaiter().GetResult();
                personStream.Position = 0;
                personStream.CopyTo(stream);
                return Task.FromResult(Substitute.For<Azure.Response>());
            });
        }

        private void JohnDoeNotExistInThePeopleStore()
        {
            _factory.BlobClient.ExistsAsync().Returns(Task.FromResult(Azure.Response.FromValue(true, null)));
        }

        private async Task JohnIsQueried()
        {
            await JohnIsQueried("DummyKey");
        }

        private async Task JohnIsQueriedWithNoApiKey()
        {
            await JohnIsQueried("DummyKey");
        }

        private async Task JohnIsRequestedToBeCreated()
        {
            await JohnIsRequestedToBeCreated("DummyKey");
        }

        private async Task JohnIsRequestedToBeCreatedWithInvalidApiKeyTask()
        {
            await JohnIsRequestedToBeCreated("InvalidKey");
        }

        private async Task JohnIsRequestedToBeCreatedWithoutEmail()
        {
            await JohnIsRequestedToBeCreated("DummyKey", person => person.Email = null);
        }

        private async Task JohnIsRequestedToBeCreatedWithAnInvalidEmail()
        {
            await JohnIsRequestedToBeCreated("DummyKey", person => person.Email = "invalid.email");
        }

        private async Task JohnIsRequestedToBeCreated(string apiKey, Action<Person> modify = null)
        {
            modify?.Invoke(_person);
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/People")
            {
                Content = new StringContent(JsonSerializer.Serialize(_person), Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Add("X-API-KEY", apiKey);
            var client = _factory.CreateClient();
            _response = await client.SendAsync(request);
        }

        private async Task JohnIsQueried(string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/People/{_person.Email}");
            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Add("X-API-KEY", apiKey);
            var client = _factory.CreateClient();
            _response = await client.SendAsync(request);
        }

        private async Task JohnIsReturned()
        {
            Assert.Equal(HttpStatusCode.OK, _response.StatusCode);
            var actualPerson = await _response.Content.ReadAsAsync<Person>();
            Assert.Equal(JsonSerializer.Serialize(_person), JsonSerializer.Serialize(actualPerson));
        }

        private async Task JohnIsCreated()
        {
            Assert.Equal(HttpStatusCode.Created, _response.StatusCode);
            var actualPerson = await _response.Content.ReadAsAsync<Person>();
            Assert.Equal(JsonSerializer.Serialize(_person), JsonSerializer.Serialize(actualPerson));
        }

        private void ResourceNotFoundIsReturned()
        {
            Assert.Equal(HttpStatusCode.NotFound, _response.StatusCode);
        }

        private void UnauthorizedIsReturned()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, _response.StatusCode);
        }

        private void ConflictIsReturned()
        {
            Assert.Equal(HttpStatusCode.Conflict, _response.StatusCode);
        }

        private void ValidationErrorIsReturned()
        {
            Assert.Equal(HttpStatusCode.BadRequest, _response.StatusCode);
        }
    }
}
