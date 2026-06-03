using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.DTOs;
using PROJECT_PRN232_.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            var classes = await _classService.GetAllClassesAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var classDto = await _classService.GetClassByIdAsync(id);
            if (classDto == null)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }
            return Ok(classDto);
        }

        [HttpPost]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _classService.CreateClassAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID in URL and body must match" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _classService.UpdateClassAsync(dto);
            if (!success)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Center")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var success = await _classService.DeleteClassAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Class with ID {id} not found" });
            }

            return NoContent();
        }
    }
}
