using Banking.Application.Abstractions;
using Banking.Application.Accounts;
using Banking.Application.Customers;
using Banking.Infrastructure.Data;
using Banking.Infrastructure.Repositories;
using Banking.Infrastructure.Repositories.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Services.AddLogging();

var useDatabase = builder.Configuration["UseDatabase"];

if (useDatabase == "EFCore")
{
    builder.Services.AddDbContext<BankingDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
}
else if (useDatabase == "Mongo")
{
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    var mongoSettings = builder.Configuration.GetSection("Mongo");
    var client = new MongoClient(mongoSettings["ConnectionString"]);
    var database = client.GetDatabase(mongoSettings["DatabaseName"]);

    builder.Services.AddSingleton<IMongoDatabase>(database);

    builder.Services.AddScoped<IAccountRepository, MongoAccountRepository>();
    builder.Services.AddScoped<ITransactionRepository, MongoTransactionRepository>();
    builder.Services.AddScoped<ICustomerRepository, MongoCustomerRepository>();
}

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("Banking.Application"))
);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transactions API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();


if (useDatabase == "EFCore")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
    await DbSeeder.SeedAsync(db);
}
else if (useDatabase == "Mongo")
{
    using var scope = app.Services.CreateScope();
    var mongo = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    await MongoSeeder.SeedAsync(mongo);
}

app.Run();
