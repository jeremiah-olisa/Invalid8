using Invalid8.Samples.WebApi.Models;

namespace Invalid8.Samples.WebApi;

using Microsoft.EntityFrameworkCore;

public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos { get; set; } = null!;
}
