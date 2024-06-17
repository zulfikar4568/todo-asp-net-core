using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context to the dependency injection (DI)
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config => {
  config.DocumentName = "TodoApi";
  config.Title = "TodoApi v1";
  config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseOpenApi();
  app.UseSwaggerUi(config => {
    config.DocumentTitle = "TodoAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
  });
}

// list of todoitems
app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync());

// list of todoitems completed
app.MapGet("/todoitems/complete", async (TodoDb db) => await db.Todos.Where(t => t.IsComplete).ToListAsync());

// get todoitems by id
app.MapGet("/todoitems/{id}", async (int id, TodoDb db) => await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound());

// create todoitems
app.MapPost("/todoitems", async (Todo todo, TodoDb db) => {
  db.Todos.Add(todo);
  await  db.SaveChangesAsync();

  return Results.Created($"/todoitems/{todo.Id}", todo);
});

// update todoitems by id
app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) => {
  var todo = await db.Todos.FindAsync(id);

  if (todo is null) return Results.NotFound();

  todo.Name = inputTodo.Name;
  todo.IsComplete = inputTodo.IsComplete;

  await db.SaveChangesAsync();
  return Results.NoContent();
});


// delete todoitems by id
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) => {
  if (await db.Todos.FindAsync(id) is Todo todo) {
    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
  }

  return Results.NotFound();
});

app.Run();
