using Invalid8.InMemory.Extensions;
using Invalid8.MediatR.Extensions;
using Invalid8.Core.Extensions;
using Invalid8.Samples.WebApi;
using Invalid8.SignalR.Extensions;
using Invalid8.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Invalid8 with InMemory Cache + MediatR Events
builder.Services.AddInMemoryCacheProvider();
builder.Services.AddSignalREventProvider();
// builder.Services.AddMediatREventProvider();
builder.Services.AddInvalid8();

// Manual subscription to cache invalidation events
builder.Services.AddHostedService<CacheInvalidationSubscriber>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHub<Invalid8Hub>("/invalid8-hub");


app.Run();
