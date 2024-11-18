using Minimal.Domain.Target.Books;
using Minmal.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Configure(builder.Configuration);

var books = new List<Book>
{
    new Book { Id = 1, Title = "1984", Author = "George Orwell" },
    new Book { Id = 2, Title = "The Martian", Author = "Andy Weir" },
    new Book { Id = 3, Title = "Ready Player One", Author = "Ernist Cline" },
};

app.MapGet("/books", () =>
{
    return books;
});

app.MapGet("/books/{id}", (int id) =>
{
    var book = books.Find(b => b.Id == id);
    if (book is null)
        return Results.NotFound("Book Not Found");

    return Results.Ok(book);
});

app.MapPost("/books", (Book book) =>
{
    books.Add(book);
    return book;
});

app.MapPut("/books/{id}", (Book book) =>
{
    var bookToUpdate = books.Find(b => b.Id == book.Id);
    if (bookToUpdate is null)
        return Results.NotFound("Book Not Found");

    bookToUpdate.Title = book.Title;
    bookToUpdate.Author = book.Author;


    return Results.Ok(book);
});

app.MapDelete("/books/{id}", (int id) =>
{
    books.RemoveAll(b => b.Id == id);

    return books;
});

app.Run();
