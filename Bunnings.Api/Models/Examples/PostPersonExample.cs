using Swashbuckle.AspNetCore.Filters;

namespace Bunnings.Api.Models.Examples
{
    public class PostPersonExample : IExamplesProvider<Person>
    {
        public Person GetExamples()
        {
            return new Person
            {
                Email = "john.snow@test.com",
                FirstName = "John",
                LastName = "Snow",
                Gender = Gender.Male,
                Pets = new[] { new Pet { Type = PetType.Dog, Name = "Brody" } }
            };
        }
    }
}
