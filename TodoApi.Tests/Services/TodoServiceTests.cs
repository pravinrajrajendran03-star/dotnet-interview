using Microsoft.Data.Sqlite;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services
{
    /// <summary>
    /// Each test gets its own isolated SQLite file database, created fresh and deleted on dispose.
    /// This prevents test pollution and makes each test fully independent.
    /// </summary>
    public class TodoServiceTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly TodoService _service;

        public TodoServiceTests()
        {
            _dbPath = Path.GetTempFileName();
            _connectionString = $"Data Source={_dbPath}";
            TodoService.InitializeDatabase(_connectionString);
            _service = new TodoService(_connectionString);
        }

        public void Dispose()
        {
            // Release all pooled connections so SQLite frees the file handle
            SqliteConnection.ClearAllPools();
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        // ---- Positive cases ----

        [Fact]
        public async Task CreateTodoAsync_ReturnsCreatedTodo_WithGeneratedId()
        {
            var request = new CreateTodoRequest { Title = "Buy milk", Description = "2% please" };

            var result = await _service.CreateTodoAsync(request);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Buy milk", result.Title);
            Assert.Equal("2% please", result.Description);
            Assert.False(result.IsCompleted);
        }

        [Fact]
        public async Task CreateTodoAsync_WithNullDescription_Succeeds()
        {
            var request = new CreateTodoRequest { Title = "No description" };

            var result = await _service.CreateTodoAsync(request);

            Assert.NotNull(result);
            Assert.Null(result.Description);
        }

        [Fact]
        public async Task GetAllTodosAsync_ReturnsAllCreatedTodos()
        {
            await _service.CreateTodoAsync(new CreateTodoRequest { Title = "Todo A" });
            await _service.CreateTodoAsync(new CreateTodoRequest { Title = "Todo B" });

            var paged = await _service.GetAllTodosAsync(1, 20);

            Assert.Equal(2, paged.TotalCount);
            Assert.Equal(2, paged.Items.Count);
            Assert.Contains(paged.Items, t => t.Title == "Todo A");
            Assert.Contains(paged.Items, t => t.Title == "Todo B");
        }

        [Fact]
        public async Task GetAllTodosAsync_ReturnsEmptyList_WhenNoTodos()
        {
            var paged = await _service.GetAllTodosAsync(1, 20);

            Assert.Equal(0, paged.TotalCount);
            Assert.Empty(paged.Items);
        }

        [Fact]
        public async Task GetAllTodosAsync_Paginates_Correctly()
        {
            for (int i = 1; i <= 5; i++)
                await _service.CreateTodoAsync(new CreateTodoRequest { Title = $"Todo {i}" });

            var page1 = await _service.GetAllTodosAsync(1, 3);
            var page2 = await _service.GetAllTodosAsync(2, 3);

            Assert.Equal(5, page1.TotalCount);
            Assert.Equal(3, page1.Items.Count);
            Assert.Equal(2, page2.Items.Count);
            Assert.Equal(2, page1.TotalPages);
        }

        [Fact]
        public async Task GetTodoByIdAsync_ReturnsCorrectTodo()
        {
            var created = await _service.CreateTodoAsync(new CreateTodoRequest { Title = "Find me" });

            var result = await _service.GetTodoByIdAsync(created.Id);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal("Find me", result.Title);
        }

        [Fact]
        public async Task UpdateTodoAsync_ChangesFieldsCorrectly()
        {
            var created = await _service.CreateTodoAsync(new CreateTodoRequest { Title = "Original", IsCompleted = false });

            var updated = await _service.UpdateTodoAsync(created.Id, new UpdateTodoRequest
            {
                Title = "Updated",
                Description = "New desc",
                IsCompleted = true
            });

            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated!.Id);
            Assert.Equal("Updated", updated.Title);
            Assert.Equal("New desc", updated.Description);
            Assert.True(updated.IsCompleted);
        }

        [Fact]
        public async Task DeleteTodoAsync_RemovesTodo_ReturnsTrue()
        {
            var created = await _service.CreateTodoAsync(new CreateTodoRequest { Title = "Delete me" });

            var deleted = await _service.DeleteTodoAsync(created.Id);

            Assert.True(deleted);
            Assert.Null(await _service.GetTodoByIdAsync(created.Id));
        }

        // ---- Negative cases ----

        [Fact]
        public async Task GetTodoByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _service.GetTodoByIdAsync(99999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTodoAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _service.UpdateTodoAsync(99999, new UpdateTodoRequest { Title = "Ghost" });

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTodoAsync_ReturnsFalse_WhenNotFound()
        {
            var result = await _service.DeleteTodoAsync(99999);

            Assert.False(result);
        }
    }
}