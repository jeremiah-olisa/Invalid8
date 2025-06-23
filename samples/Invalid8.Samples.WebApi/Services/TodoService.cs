using Invalid8.Samples.WebApi.Models;

namespace Invalid8.Samples.WebApi.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TodoService(TodoContext context, ILogger<TodoService> logger)
{
    private Task SimulateHeavyDb(ushort time = 2500) => Task.Delay(time);

    public async Task<IEnumerable<Todo>> GetTodosAsync()
    {
        logger.LogInformation("Fetching all todos from database...");
        await SimulateHeavyDb(); // simulate slow DB call
        var todos = await context.Todos.ToListAsync();
        logger.LogInformation("Retrieved {Count} todos from database.", todos.Count);
        return todos;
    }

    public async Task<Todo?> GetTodoAsync(int id)
    {
        logger.LogInformation("Fetching todo with Id={Id} from database...", id);
        await SimulateHeavyDb(); // simulate slow DB call
        var todo = await context.Todos.FindAsync(id);
        if (todo == null)
            logger.LogWarning("Todo with Id={Id} not found.", id);
        else
            logger.LogInformation("Todo with Id={Id} retrieved.", id);

        return todo;
    }

    public async Task<Todo> AddTodoAsync(Todo todo)
    {
        logger.LogInformation("Adding new todo: {@Todo}", todo);
        await SimulateHeavyDb(); // simulate slow DB call
        context.Todos.Add(todo);
        await context.SaveChangesAsync();
        logger.LogInformation("Todo added with Id={Id}.", todo.Id);
        return todo;
    }

    public async Task<bool> UpdateTodoAsync(int id, Todo todo)
    {
        if (id != todo.Id)
        {
            logger.LogWarning("Update failed. URL id={Id} does not match todo.Id={TodoId}.", id, todo.Id);
            return false;
        }

        logger.LogInformation("Updating todo with Id={Id}.", id);
        await SimulateHeavyDb(); // simulate slow DB call
        context.Entry(todo).State = EntityState.Modified;
        await context.SaveChangesAsync();
        logger.LogInformation("Todo with Id={Id} updated.", id);
        return true;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        logger.LogInformation("Deleting todo with Id={Id}.", id);
        await SimulateHeavyDb(); // simulate slow DB call
        var todo = await context.Todos.FindAsync(id);
        if (todo == null)
        {
            logger.LogWarning("Todo with Id={Id} not found. Delete aborted.", id);
            return false;
        }

        context.Todos.Remove(todo);
        await context.SaveChangesAsync();
        logger.LogInformation("Todo with Id={Id} deleted.", id);
        return true;
    }
}