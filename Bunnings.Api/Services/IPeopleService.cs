using System.Threading.Tasks;
using Bunnings.Api.Models;

namespace Bunnings.Api.Services
{
    public interface IPeopleService
    {
        Task<Person> CreateAsync(Person person);
        Task<Person> GetByIdAsync(string id);
    }
}
