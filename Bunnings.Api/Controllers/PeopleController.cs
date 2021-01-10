using System.Net.Mime;
using System.Threading.Tasks;
using Bunnings.Api.Models;
using Bunnings.Api.Models.Examples;
using Bunnings.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Bunnings.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : Controller
    {
        private readonly IPeopleService _peopleService;

        public PeopleController(IPeopleService peopleService)
        {
            _peopleService = peopleService;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerRequestExample(typeof(Person), typeof(PostPersonExample))]
        public async Task<IActionResult> CreateAsync(Person person)
        {
            var savedPerson = await _peopleService.CreateAsync(person);
            if (savedPerson == null)
            {
                return Conflict();
            }

            return CreatedAtAction(nameof(GetByIdAsync), new { id = person.Email }, person);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var person = await _peopleService.GetByIdAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }
    }
}
