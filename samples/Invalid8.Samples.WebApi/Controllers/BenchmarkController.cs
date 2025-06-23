using Invalid8.Samples.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Invalid8.Samples.WebApi.Controllers;

[ApiController]
[Route("api/invalid8/benchmark")]
public class BenchmarkController(BenchmarkService benchmarkService) : ControllerBase
{
    [HttpGet("todo-service")]
    public async Task<ActionResult<BenchmarkResult>> BenchmarkTodoService(int id = 1)
    {
        var result = await benchmarkService.BenchmarkTodoServiceAsync(id);
        return Ok(result);
    }

    [HttpGet("cached-todo-service")]
    public async Task<ActionResult<BenchmarkResult>> BenchmarkCachedTodoService(int id = 1)
    {
        var result = await benchmarkService.BenchmarkCachedTodoServiceAsync(id);
        return Ok(result);
    }

    [HttpGet("compare")]
    public async Task<ActionResult<BenchmarkResult>> CompareBothServices(int id = 1)
    {
        var result = await benchmarkService.BenchmarkBothServicesAsync(id);
        return Ok(result);
    }
}