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
        public IActionResult GetAll()
        {
            var todos = _todoService.GetAllTodos();
            return Ok(todos);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var todo = _todoService.GetTodoById(id);
            return todo is null ? NotFound() : Ok(todo);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateTodoRequest requset)
        {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var todo = _todoService.CreateTodo(requset);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
        }

       

        [HttpPut("{id:int}")]
        public IActionResult Update(int id,[FromBody] UpdateTodoRequest request)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var updated = _todoService.UpdateTodo(id, request);
            return updated is null ? NotFound() : Ok(updated);

        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteTodo(int id)
        {
            var deleted = _todoService.DeleteTodo(id);
            return deleted ? NoContent() : NotFound();
        }
    }


}
