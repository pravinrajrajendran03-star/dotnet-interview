using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Controllers
{
    /// <summary>
    /// Controller tests use a Moq mock of ITodoService.
    /// No database involved — these tests verify HTTP status codes and response shapes only.
    /// </summary>
    public class TodoControllerTests
    {
        private readonly Mock<ITodoService> _mockService;
        private readonly TodoController _controller;

        public TodoControllerTests()
        {
            _mockService = new Mock<ITodoService>();
            _controller = new TodoController(_mockService.Object);
        }

        // ---- GET /api/todos ----

        [Fact]
        public async Task GetAll_Returns200_WithPagedResult()
        {
            var paged = new PagedResult<Todo>
            {
                Items = new List<Todo> { new Todo { Id = 1, Title = "A" }, new Todo { Id = 2, Title = "B" } },
                TotalCount = 2,
                Page = 1,
                PageSize = 20
            };
            _mockService.Setup(s => s.GetAllTodosAsync(1, 20)).ReturnsAsync(paged);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PagedResult<Todo>>(ok.Value);
            Assert.Equal(2, returned.Items.Count);
            Assert.Equal(2, returned.TotalCount);
        }

        [Fact]
        public async Task GetAll_Returns200_WithEmptyItems_WhenNoTodos()
        {
            _mockService.Setup(s => s.GetAllTodosAsync(1, 20))
                .ReturnsAsync(new PagedResult<Todo> { TotalCount = 0, Page = 1, PageSize = 20 });

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PagedResult<Todo>>(ok.Value);
            Assert.Empty(returned.Items);
        }

        [Fact]
        public async Task GetAll_Returns400_WhenPageIsZero()
        {
            var result = await _controller.GetAll(page: 0);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetAll_Returns400_WhenPageSizeExceedsMax()
        {
            var result = await _controller.GetAll(pageSize: 101);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ---- GET /api/todos/{id} ----

        [Fact]
        public async Task GetById_Returns200_WhenFound()
        {
            _mockService.Setup(s => s.GetTodoByIdAsync(1)).ReturnsAsync(new Todo { Id = 1, Title = "Found" });

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var todo = Assert.IsType<Todo>(ok.Value);
            Assert.Equal(1, todo.Id);
        }

        [Fact]
        public async Task GetById_Returns404_WhenNotFound()
        {
            _mockService.Setup(s => s.GetTodoByIdAsync(99)).ReturnsAsync((Todo?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result);
        }

        // ---- POST /api/todos ----

        [Fact]
        public async Task Create_Returns201Created_WithLocation()
        {
            var request = new CreateTodoRequest { Title = "New Todo" };
            var created = new Todo { Id = 5, Title = "New Todo" };
            _mockService.Setup(s => s.CreateTodoAsync(request)).ReturnsAsync(created);

            var result = await _controller.Create(request);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAt.StatusCode);
            Assert.Equal(nameof(_controller.GetById), createdAt.ActionName);
            Assert.Equal(5, ((Todo)createdAt.Value!).Id);
        }

        // ---- PUT /api/todos/{id} ----

        [Fact]
        public async Task Update_Returns200_WhenUpdated()
        {
            var request = new UpdateTodoRequest { Title = "Changed", IsCompleted = true };
            var updated = new Todo { Id = 1, Title = "Changed", IsCompleted = true };
            _mockService.Setup(s => s.UpdateTodoAsync(1, request)).ReturnsAsync(updated);

            var result = await _controller.Update(1, request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var todo = Assert.IsType<Todo>(ok.Value);
            Assert.True(todo.IsCompleted);
        }

        [Fact]
        public async Task Update_Returns404_WhenNotFound()
        {
            _mockService.Setup(s => s.UpdateTodoAsync(99, It.IsAny<UpdateTodoRequest>())).ReturnsAsync((Todo?)null);

            var result = await _controller.Update(99, new UpdateTodoRequest { Title = "Ghost" });

            Assert.IsType<NotFoundResult>(result);
        }

        // ---- DELETE /api/todos/{id} ----

        [Fact]
        public async Task Delete_Returns204NoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteTodoAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Returns404_WhenNotFound()
        {
            _mockService.Setup(s => s.DeleteTodoAsync(99)).ReturnsAsync(false);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}