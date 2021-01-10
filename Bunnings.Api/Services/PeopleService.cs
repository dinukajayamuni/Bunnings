using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Bunnings.Api.Models;

namespace Bunnings.Api.Services
{
    //public class PeopleService : IPeopleService
    //{
    //    private readonly CloudTable _table;

    //    public PeopleService(CloudTableClient tableClient)
    //    {
    //        _table = tableClient.GetTableReference("people");
    //    }
    //    public async Task<Person> CreateAsync(Person person)
    //    {
    //        if (person.Gender == null)
    //        {
    //            throw new ArgumentNullException(nameof(Person.Gender));
    //        }

    //        var personDomainModel = new DomainModels.Person
    //        {
    //            Email = person.Email,
    //            FirstName = person.FirstName,
    //            LastName = person.LastName,
    //            Gender = person.Gender.Value
    //        };

    //        var insertOrMergeOperation = TableOperation.InsertOrMerge(personDomainModel);

    //        await _table.ExecuteAsync(insertOrMergeOperation);
    //        return person;
    //    }

    //    public async Task<Person> GetByIdAsync(string id)
    //    {
    //        var query = new TableQuery<DomainModels.Person>().Where(
    //            TableQuery.GenerateFilterCondition(
    //                "PartitionKey",
    //                QueryComparisons.GreaterThanOrEqual,
    //                id
    //            )
    //        );
    //        TableContinuationToken token = null;
    //        var people = new List<DomainModels.Person>();
    //        do
    //        {

    //            var results = await _table.ExecuteQuerySegmentedAsync(query, token);
    //            token = results.ContinuationToken;
    //            people.AddRange(results);

    //        } while (token != null);


    //        return people.FirstOrDefault() != null;
    //    }
    //}
    public class PeopleService : IPeopleService
    {
        private static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly BlobContainerClient _blobContainerClient;

        public PeopleService(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
        }

        public async Task<Person> CreateAsync(Person person)
        {
            var client = _blobContainerClient.GetBlobClient($"{person.Email}.json");
            if (await client.ExistsAsync()) return null;
            await using var personStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(personStream, person, SerializerOptions);
            personStream.Position = 0;
            await client.UploadAsync(personStream);
            return person;
        }

        public async Task<Person> GetByIdAsync(string id)
        {
            var client = _blobContainerClient.GetBlobClient($"{id}.json");
            if (!await client.ExistsAsync()) return null;
            await using var personStream = new MemoryStream();
            await client.DownloadToAsync(personStream);
            personStream.Position = 0;
            var person = await JsonSerializer.DeserializeAsync<Person>(personStream, SerializerOptions);
            return person;
        }
    }
}