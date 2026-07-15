using Microsoft.Data.Sqlite;
using TodoApi.Models;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly string _connectionString;

        public TodoService(string connectionString)
        {
            _connectionString = connectionString;
            
        }

        public static void InitializeDatabase(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Todos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Description TEXT,
            IsCompleted INTEGER NOT NULL DEFAULT 0,
            CreatedAt TEXT NOT NULL
        )
           ";
            command.ExecuteNonQuery();

        }

        public async  Task<Todo> CreateTodoAsync(CreateTodoRequest request)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = 
                @"
                INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
                VALUES ($title,$description,$isCompleted,$createdAt);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$title", request.Title);
            command.Parameters.AddWithValue("$description", request.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$isCompleted", request.IsCompleted ? 1: 0);
            var createdAt = DateTime.UtcNow;
            command.Parameters.AddWithValue("$createdAt", createdAt.ToString("o"));

            var id = (long)(await command.ExecuteScalarAsync())!;
            return new Todo
            {
                Id = (int)id,
                Title = request.Title,
                Description = request.Description,
                IsCompleted = request.IsCompleted,
                CreatedAt = createdAt
            };
        }

        public async Task<PagedResult<Todo>> GetAllTodosAsync(int page, int pageSize)
        {
            var todos = new List<Todo>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM Todos";
            var totalCount = (int)(long)(await countCmd.ExecuteScalarAsync())!;
            var dataCmd = connection.CreateCommand();
            dataCmd.CommandText =
                @"
                   SELECT * from Todos
                   ORDER BY Id
                   LIMIT $pageSize OFFSET $offset
                ";
            dataCmd.Parameters.AddWithValue("$pageSize", pageSize);
            dataCmd.Parameters.AddWithValue("$offset", (page-1) * pageSize);

            var items = new List<Todo>();
            using var reader = await dataCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(MapRow(reader));
            }
            return new PagedResult<Todo>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Todo?> GetTodoByIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM Todos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            return await  reader.ReadAsync() ? MapRow(reader) : null;
        }

        public async Task<Todo?> UpdateTodoAsync(int id, UpdateTodoRequest todo)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = 
                @"
                UPDATE Todos
                SET Title = $title, Description = $description, IsCompleted = $isCompleted
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$title", todo.Title);
            command.Parameters.AddWithValue("$description", todo.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$isCompleted", todo.IsCompleted ? 1 : 0);
            command.Parameters.AddWithValue("$id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected == 0 ? null :await  GetTodoByIdAsync(id);

        }

        public async Task<bool> DeleteTodoAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM Todos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static Todo MapRow(SqliteDataReader reader)
        {
            return new Todo{
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsCompleted = reader.GetBoolean(3),
                CreatedAt = DateTime.Parse(reader.GetString(4))
            };
        }
    }
}
