using System.Collections.Generic;

namespace Bunnings.Api.Models
{
    public class Person
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender? Gender { get; set; }
        public IEnumerable<Pet> Pets { get; set; }
    }
}