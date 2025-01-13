using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// הוספת ה-DbContext עם MySQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql")));


builder.Services.AddScoped<TodoService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});


// // הוספת שירותי CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// // הפעלת CORS
app.UseCors("AllowAll");

// הפעלת Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // זה יאפשר גישה ל-Swagger בכתובת הבסיס
    });
}

// app.MapGet("/", () => "Hello World!");

app.MapGet("/items", async (TodoService todoService) =>
{
    return await todoService.GetItemsAsync();
});

app.MapGet("/items/{id}", async (int id, TodoService todoService) =>
{
    var item = await todoService.GetItemByIdAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/items", async (Item item, TodoService todoService) =>
{
    await todoService.AddItemAsync(item.Name);
    return Results.Created($"Create new item Id:{item.Id}", item);
});

app.MapPut("/items/{id}", async (int id, Item updatedItem, TodoService todoService) =>
{
    var existingItem = await todoService.GetItemByIdAsync(id);
    if (existingItem is null)
    {
        return Results.NotFound();
    }

    if (!string.IsNullOrEmpty(updatedItem.Name))
    {
        existingItem.Name = updatedItem.Name;
    }
    
    if (updatedItem.IsComplete.HasValue)
    {
        existingItem.IsComplete = updatedItem.IsComplete.Value;
    }

    await todoService.UpdateItemAsync(existingItem);
    return Results.Ok(new { Message = $"Updated item {id}", Data = existingItem});

});

app.MapDelete("/items/{id}", async (int id, TodoService todoService) =>
{
    var existingItem = await todoService.GetItemByIdAsync(id);
    if (existingItem is null)
    {
        return Results.NotFound();
    }

    await todoService.DeleteItemAsync(existingItem);
    return Results.Ok($"The user {id} deleted");
});


app.Run();


public class TodoService
{
    private readonly ToDoDbContext _context;

    public TodoService(ToDoDbContext context)
    {
        _context = context;
    }

    // פעולות על מסד הנתונים
    public async Task<List<Item>> GetItemsAsync()
    {
        return await _context.Items.ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _context.Items.FindAsync(id);
    }

    public async Task AddItemAsync(string name)
    {
        var newItem = new Item
        {
            Name = name,
            IsComplete = false // הגדרת isComplete כ-false
        };

        _context.Items.Add(newItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(Item item)
    {
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }
}
