using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var httpClient = new HttpClient();

/* ---------- LOGIN VIA GATEWAY ---------- */
app.MapPost("/api/login", async (LoginRequest request) =>
{
    try
    {
        var response = await httpClient.PostAsJsonAsync(
            "http://localhost/login/login",
            request
        );

        var result = await response.Content.ReadFromJsonAsync<object>();
        return Results.Ok(result);
    }
    catch
    {
        return Results.Problem("LoginService is unavailable");
    }
});

/* ---------- SIGNUP VIA GATEWAY ---------- */
app.MapPost("/api/signup", async (SignupRequest request) =>
{
    try
    {
        var response = await httpClient.PostAsJsonAsync(
            "http://localhost/user/signup",
            request
        );

        var result = await response.Content.ReadFromJsonAsync<object>();
        return Results.Ok(result);
    }
    catch
    {
        return Results.Problem("UserService is unavailable");
    }
});

/* ---------- DASHBOARD DATA API ---------- */
app.MapGet("/api/dashboard-data", async () =>
{
    try
    {
        var users = await httpClient.GetFromJsonAsync<List<UserDto>>(
            "http://localhost/user/users"
        );

        var totalUsers = users.Count;
        var totalLogins = users.Sum(u => u.loginCount);

        return Results.Ok(new
        {
            totalUsers,
            totalLogins,
            status = "Active"
        });
    }
    catch
    {
        return Results.Problem("Dashboard data unavailable");
    }
});

/* ---------- HEALTH ---------- */
app.MapGet("/health", () =>
{
    return Results.Ok("GatewayService is running");
});

app.Run();

/* ---------- MODELS ---------- */
record LoginRequest(string username, string password);
record SignupRequest(string username, string password);
record UserDto(string username, int loginCount);