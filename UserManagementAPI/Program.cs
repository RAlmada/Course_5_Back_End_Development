var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var users = new List<User>();

app.MapGet("/users/{id?}", (int? id) => 
{
    if (id.HasValue)
    {
        Console.WriteLine($"GET: has id: {id}");
        return Results.Ok(users.FirstOrDefault(u => u.Id == id));
    }

    return Results.Ok(users);
});

app.MapPost("/users", (User user) =>
{
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users/{id}", (int id, User user) =>
{
    if (id != user.Id)
    {
        return Results.BadRequest();
    }

    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser != null)
    {
        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
    }

    return Results.Ok(existingUser);
});

app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);

    if (user != null)
    {
        users.Remove(user);
    }

    return Results.Ok(new { message = "User deleted successfully" });
});

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
