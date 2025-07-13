using Invalid8.Core;
using Microsoft.AspNetCore.Mvc;

namespace Invalid8.Samples.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IQueryClient queryClient) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Swelterin", "Scorching"
    ];

    private readonly IQueryClient _queryClient = queryClient;
    private static readonly string[] item = ["weather", "forecast"];

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<QueryResult<WeatherForecast[]>> Get()
    {
        var result = await _queryClient.UseCachedQueryAsync(
            key: item,
            queryMethod: GetWeatherForcastFromDb()
        );

        return result;
    }

    [HttpPost("invalidate", Name = "InvalidateWeatherForecast")]
    public async Task<IActionResult> InvalidateCache()
    {
        await _queryClient.UseMutateQueryAsync(
            key: item,
            mutationFunc: async () =>
            {
                // Simulate mutation (e.g., updating data source)
                await Task.Delay(50);
                return true; // Mutation result (not used in this case)
            },
            options: new MutationOptions
            {
                InvalidationKeys = [item]
            });

        return Ok("Cache invalidated");
    }

    private static Func<Task<WeatherForecast[]>> GetWeatherForcastFromDb()
    {
        return async () =>
        {
            // Simulate a heavy data fetch
            await Task.Delay(10000);
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();
        };
    }

}

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}