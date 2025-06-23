using Invalid8.Samples.WebApi.Models;
using Invalid8.Samples.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Invalid8.Samples.WebApi.Controllers;

[ApiController]
[Route("api/invalid8/todos")]
public class TodoController(CachedTodoService service) : ControllerBase
{
    /// <summary>
    /// Get all todos.
    /// </summary>
    /// <returns>A list of todos.</returns>
    /// <response code="200">Returns the list of todos</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Todo>))]
    public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
    {
        var todos = await service.GetTodosAsync();
        return Ok(todos);
    }

    /// <summary>
    /// Get a specific todo by id.
    /// </summary>
    /// <param name="id">The todo id</param>
    /// <returns>A todo object.</returns>
    /// <response code="200">Returns the todo</response>
    /// <response code="404">If the todo was not found</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Todo))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Todo>> GetTodo(int id)
    {
        var todo = await service.GetTodoAsync(id);
        if (todo == null) return NotFound();
        return Ok(todo);
    }

    /// <summary>
    /// Create a new todo.
    /// </summary>
    /// <param name="todo">The todo payload</param>
    /// <returns>The created todo with Id</returns>
    /// <response code="201">Returns the newly created todo</response>
    /// <response code="400">If the request payload is invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Todo))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Todo>> PostTodo(Todo todo)
    {
        var created = await service.AddTodoAsync(todo);
        return CreatedAtAction(nameof(GetTodo), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing todo.
    /// </summary>
    /// <param name="id">The id of the todo to update</param>
    /// <param name="todo">The updated todo payload</param>
    /// <response code="204">If the update succeeded</response>
    /// <response code="400">If the id does not match the payload</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutTodo(int id, Todo todo)
    {
        var success = await service.UpdateTodoAsync(id, todo);
        if (!success) return BadRequest();
        return NoContent();
    }

    /// <summary>
    /// Delete a todo.
    /// </summary>
    /// <param name="id">The id of the todo to delete</param>
    /// <response code="204">If the delete succeeded</response>
    /// <response code="404">If the todo was not found</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var success = await service.DeleteTodoAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}