using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<APITarefasContext>(options => options.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/tarefas", async (APITarefasContext db) => await db.Tarefas.ToListAsync());

app.MapGet("/tarefas/concluida", async (APITarefasContext db) => await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

app.MapGet("/tarefas/{id}", async (int id, APITarefasContext db) => await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

app.MapPost("/tarefas", async (Tarefa tarefa, APITarefasContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (int id, Tarefa inputTarefa, APITarefasContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if (tarefa is null) Results.NotFound();

    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/tarefas/{id}", async (int id, APITarefasContext db) => 
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }

    return Results.NotFound();
});

app.Run();

class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluida { get; set; }
}

class APITarefasContext : DbContext
{
    public APITarefasContext(DbContextOptions<APITarefasContext> options) : base(options)
    {}

    public DbSet<Tarefa>? Tarefas => Set<Tarefa>();
}