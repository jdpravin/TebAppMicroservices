using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/* MongoDB Connection */
var client = new MongoClient("mongodb://172.31.17.237:27017");
var database = client.GetDatabase("usersdb");
var users = database.GetCollection<User>("users");

/* SIGNUP API */
app.MapPost("/signup", async (User user) =>
{
    var existing = await users
        .Find(x => x.username == user.username)
        .FirstOrDefaultAsync();

    if (existing != null)
        return Results.Ok(new { success = false, message = "User exists" });

    user.loginCount = 0;
    await users.InsertOneAsync(user);

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

/* Health check */
app.MapGet("/health", () => Results.Ok("UserService is running"));

app.Run();

/* MODEL */
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