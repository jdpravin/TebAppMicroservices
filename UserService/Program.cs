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

/* SIGNUP API */
app.MapPost("/signup", async (SignupRequest request) =>
{
    var existing = await users
        .Find(x => x.username == request.username)
        .FirstOrDefaultAsync();

    if (existing != null)
    {
        return Results.Ok(new
        {
            success = false,
            message = "User already exists"
        });
    }

    var newUser = new User
    {
        username = request.username,
        password = request.password,
        loginCount = 0
    };

    await users.InsertOneAsync(newUser);

    return Results.Ok(new { success = true });
});

/* GET ALL USERS (safe response) */
app.MapGet("/users", async () =>
{
    var result = await users
        .Find(FilterDefinition<User>.Empty)
        .Project(u => new
        {
            u.username,
            u.loginCount
        })
        .ToListAsync();

    return Results.Ok(result);
});

/* HEALTH CHECK */
app.MapGet("/health", () =>
{
    return Results.Ok("UserService is running");
});

app.Run();

/* =====================================================
   Models
   ===================================================== */

record SignupRequest(string username, string password);

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

