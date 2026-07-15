using TodoApi.Models;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Task<Todo> CreateTodoAsync(CreateTodoRequest request);
        Task<PagedResult<Todo>> GetAllTodosAsync( int page,int pageSize);
        Task<Todo?> GetTodoByIdAsync(int id);
        Task<Todo?> UpdateTodoAsync(int id, UpdateTodoRequest request);
        Task<bool> DeleteTodoAsync(int id);
    }
}
