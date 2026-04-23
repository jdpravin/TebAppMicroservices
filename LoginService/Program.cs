using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/* =====================================================
   MongoDB Configuration (Environment-based)
   ===================================================== */

// Read MongoDB connection string from environment
var mongoConn = Environment.GetEnvironmentVariable("MONGO_CONN");

if (string.IsNullOrWhiteSpace(mongoConn))
{
    throw new Exception("MongoDB connection string not found. Set MONGO_CONN environment variable.");
}

// Create MongoDB client
var client = new MongoClient(mongoConn);
var database = client.GetDatabase("usersdb");
var users = database.GetCollection<User>("users");

/* =====================================================
   APIs
   ===================================================== */

/* LOGIN API */
app.MapPost("/login", async (LoginRequest request) =>
{
    var user = await users.Find(x =>
        x.username == request.username &&
        x.password == request.password
    ).FirstOrDefaultAsync();

    if (user != null)
    {
        // Increment login count
        var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);
        var update = Builders<User>.Update.Inc(x => x.loginCount, 1);
        await users.UpdateOneAsync(filter, update);

        return Results.Ok(new
        {
            success = true,
            username = user.username
        });
    }

    return Results.Ok(new { success = false });
});

/* Health Check API */
app.MapGet("/health", () =>
{
    return Results.Ok("LoginService is running");
});

app.Run();

/* =====================================================
   Models
   ===================================================== */

record LoginRequest(string username, string password);

[BsonIgnoreExtraElements]
class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string username { get; set; } = null!;
    public string password { get; set; } = null!;
    public int loginCount { get; set; }
}
