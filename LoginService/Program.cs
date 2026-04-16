using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/* MongoDB Connection */
var client = new MongoClient("mongodb://172.31.17.237:27017");
var database = client.GetDatabase("usersdb");
var users = database.GetCollection<User>("users");

/* LOGIN API */
app.MapPost("/login", async (User user) =>
{
    var result = await users.Find(x =>
        x.username == user.username &&
        x.password == user.password
    ).FirstOrDefaultAsync();

    if (result != null)
    {
        var filter = Builders<User>.Filter.Eq(x => x.username, result.username);
        var update = Builders<User>.Update.Inc(x => x.loginCount, 1);
        await users.UpdateOneAsync(filter, update);

        return Results.Ok(new { success = true, username = result.username });
    }

    return Results.Ok(new { success = false });
});

/* Health check */
app.MapGet("/health", () => Results.Ok("LoginService is running"));

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