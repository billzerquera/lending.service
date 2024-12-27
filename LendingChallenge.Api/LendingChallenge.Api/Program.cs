using LendingChallenge.Api.Endpoints;
using LendingChallenge.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ILendingRepository, InMemoryLendingRepository>();


var app = builder.Build();


app.MapLendingEndpoints();

app.Run();
