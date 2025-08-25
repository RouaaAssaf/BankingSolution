// src/Banking.WebApi/Program.cs
using Banking.Application.Abstractions;
using Banking.Application.Accounts;
using Banking.Application.Customers;
using Banking.Infrastructure.Data;
using Banking.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore; // add this
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // allow lowercase JSON
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()); // enum as string
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core SQLite
builder.Services.AddDbContext<BankingDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=banking.db"));

// DI
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<OpenAccountService>();
builder.Services.AddScoped<GetCustomerSummaryService>();
builder.Services.AddScoped<AddTransactionService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Banking.Application.Accounts.OpenAccountRequestValidator>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Error handling for production
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
            var ex = feature?.Error;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                InvalidOperationException ioe when ioe.Message.Contains("Customer not found")
                    => StatusCodes.Status404NotFound,
                ArgumentException
                    => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(new { error = ex?.Message });
        });
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // allow CSS/JS
app.UseRouting();
app.UseAuthorization();

//  Default MVC route for views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//  Keep your API routes working
app.MapControllers();

app.Run();
