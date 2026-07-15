using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/todos")]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("page must be ?=1; page size must be between 1 to 100.");

            var result = await _todoService.GetAllTodosAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var todo = await _todoService.GetTodoByIdAsync(id);
            return todo is null ? NotFound() : Ok(todo);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTodoRequest requset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var todo = await _todoService.CreateTodoAsync(requset);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var updated = await _todoService.UpdateTodoAsync(id, request);
            return updated is null ? NotFound() : Ok(updated);

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _todoService.DeleteTodoAsync(id);
            return deleted ? NoContent() : NotFound();
        }

    }

}
