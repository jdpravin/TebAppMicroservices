using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/* =====================================================
   HttpClient for internal service communication
   ===================================================== */

var httpClient = new HttpClient();

/*
 Docker service names are used as hostnames:
 - login  → LoginService container
 - user   → UserService container
 */

/* =====================================================
   LOGIN VIA GATEWAY
   ===================================================== */
app.MapPost("/api/login", async (LoginRequest request) =>
{
    try
    {
        var response = await httpClient.PostAsJsonAsync(
            "http://login:5001/login",
            request
        );

        return Results.Ok(await response.Content.ReadFromJsonAsync<object>());
    }
    catch
    {
        return Results.Problem("LoginService is unavailable");
    }
});

/* =====================================================
   SIGNUP VIA GATEWAY
   ===================================================== */
app.MapPost("/api/signup", async (SignupRequest request) =>
{
    try
    {
        var response = await httpClient.PostAsJsonAsync(
            "http://user:5002/signup",
            request
        );

        return Results.Ok(await response.Content.ReadFromJsonAsync<object>());
    }
    catch
    {
        return Results.Problem("UserService is unavailable");
    }
});

/* =====================================================
   DASHBOARD DATA (AGGREGATION)
   ===================================================== */
app.MapGet("/api/dashboard-data", async () =>
{
    try
    {
        var users = await httpClient.GetFromJsonAsync<List<UserDto>>(
            "http://user:5002/users"
        );

        var totalUsers = users?.Count ?? 0;
        var totalLogins = users?.Sum(u => u.loginCount) ?? 0;

        return Results.Ok(new
        {
            totalUsers,
            totalLogins
        });
    }
    catch
    {
        return Results.Problem("Dashboard data unavailable");
    }
});

/* =====================================================
   HEALTH CHECK
   ===================================================== */
app.MapGet("/health", () =>
{
    return Results.Ok("GatewayService is running");
});

app.Run();

/* =====================================================
   MODELS
   ===================================================== */

record LoginRequest(string username, string password);
record SignupRequest(string username, string password);
record UserDto(string username, int loginCount);

