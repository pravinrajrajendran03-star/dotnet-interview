using TodoApi.Models

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Todo CreateTodo(CreateTodoRequest request);
        List<TodoApi> GetAllTodos();
        Todo? UpdateTodo(int id, UpdateTodoRequest request);
        bool DeleteTodo(int id);
    }
}
