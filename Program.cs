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

var todoItems = app.MapGroup("/todoitems");


todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompletedTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

// list of todoitems
static async Task<IResult> GetAllTodos(TodoDb db) {
  return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

// list of todoitems completed
static async Task<IResult> GetCompletedTodos(TodoDb db) {
  return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

// get todoitems by id
static async Task<IResult> GetTodo(int id, TodoDb db) {
  return await db.Todos.FindAsync(id) is Todo todo ? TypedResults.Ok(new TodoItemDTO(todo)) : TypedResults.NotFound();
}

// create todoitems
static async Task<IResult> CreateTodo(TodoItemDTO todo, TodoDb db) {
  var todoItem = new Todo {
    IsComplete = todo.IsComplete,
    Name = todo.Name,
  };

  db.Todos.Add(todoItem);
  await  db.SaveChangesAsync();

  todo = new TodoItemDTO(todoItem);

  return TypedResults.Created($"/{todo.Id}", todo);
}

// update todoitems by id
static async Task<IResult> UpdateTodo(int id, TodoItemDTO todo, TodoDb db) {
  var todoDb = await db.Todos.FindAsync(id);

  if (todoDb is null) return TypedResults.NotFound();

  todoDb.Name = todo.Name;
  todoDb.IsComplete = todo.IsComplete;

  await db.SaveChangesAsync();
  return TypedResults.NoContent();
}

// delete todoitems by id
static async Task<IResult> DeleteTodo(int id, TodoDb db) {
  if (await db.Todos.FindAsync(id) is Todo todo) {
    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
  }

  return TypedResults.NotFound();
}