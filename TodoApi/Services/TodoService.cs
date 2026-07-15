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

        public Todo CreateTodo(CreateTodoRequest request)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $@"
                INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
                VALUES ($title,$description,$isCompleted,$createdAt);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$title", request.Title);
            command.Parameters.AddWithValue("$description", request.Description);
            command.Parameters.AddWithValue("$isCompleted", request.IsCompleted ? 1: 0);
            var createdAt = DateTime.UtcNow;
            command.Parameters.AddWithValue("$createdAt", createdAt.ToString("o"));

            var id = Convert.ToInt32(command.ExecuteScalar());
            return new Todo
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                IsCompleted = request.IsCompleted,
                CreatedAt = createdAt
            };
        }

        public List<Todo> GetAllTodos()
        {
            var todos = new List<Todo>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Todos";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                todos.Add(MapRow(reader));
            }

            return todos;
        }

        public Todo? GetTodoById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM Todos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        public Todo? UpdateTodo(int id, UpdateTodoRequest todo)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $@"
                UPDATE Todos
                SET Title = $title, Description = $description, IsCompleted = $isCompleted
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$title", todo.Title);
            command.Parameters.AddWithValue("$description", todo.Description);
            command.Parameters.AddWithValue("$isCompleted", todo.IsCompleted ? 1 : 0);
            command.Parameters.AddWithValue("$id", id);

            var rowsAffected = command.ExecuteNonQuery();

            return rowsAffected == 0 ? null : GetTodoById(id);

        }

        public bool DeleteTodo(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM Todos WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            return command.ExecuteNonQuery() > 0;
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
