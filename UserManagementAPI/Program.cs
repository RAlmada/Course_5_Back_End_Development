using System.ComponentModel.DataAnnotations;

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
        var user = users.FirstOrDefault(u => u.Id == id);
        return user == null
            ? Results.NotFound(new { error = $"User with id {id} not found" })
            : Results.Ok(user);
    }

    return Results.Ok(users);
});

app.MapPost("/users", (User user) =>
{
    if (string.IsNullOrWhiteSpace(user.Name))
        return Results.BadRequest(new { error = "Name is required" });

    if (string.IsNullOrWhiteSpace(user.Email) || !new EmailAddressAttribute().IsValid(user.Email))
        return Results.BadRequest(new { error = "Email is required and must be valid" });

    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users/{id}", (int id, User user) =>
{
    if (id != user.Id)
        return Results.BadRequest(new { error = "ID mismatch" });

    if (string.IsNullOrWhiteSpace(user.Name))
        return Results.BadRequest(new { error = "Name is required" });

    if (string.IsNullOrWhiteSpace(user.Email) || !new EmailAddressAttribute().IsValid(user.Email))
        return Results.BadRequest(new { error = "Email is required and must be valid" });

    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser == null)
        return Results.NotFound(new { error = $"User with id {id} not found" });

    existingUser.Name = user.Name;
    existingUser.Email = user.Email;

    return Results.Ok(existingUser);
});

app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);

    if (user == null)
        return Results.NotFound(new { error = $"User with id {id} not found" });

    users.Remove(user);

    return Results.Ok(new { message = "User deleted successfully" });
});

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
