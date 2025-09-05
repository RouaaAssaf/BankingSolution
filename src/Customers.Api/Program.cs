using Banking.Application.Abstractions;
using Banking.Application.Customers;
using Banking.Infrastructure.Data;
using Banking.Infrastructure.Repositories;
using Banking.Infrastructure.Repositories.Mongo;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Services.AddLogging();

var useDatabase = builder.Configuration["UseDatabase"];

if (useDatabase == "EFCore")
{
    // ---- EF Core setup ----
    builder.Services.AddDbContext<BankingDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
}
else if (useDatabase == "Mongo")
{
    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    var mongoSettings = builder.Configuration.GetSection("Mongo");
    var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoSettings["ConnectionString"]);
    var client = new MongoClient(mongoClientSettings);
    var database = client.GetDatabase(mongoSettings["DatabaseName"]);

    builder.Services.AddSingleton<IMongoDatabase>(database);

    builder.Services.AddScoped<ICustomerRepository, MongoCustomerRepository>();
    builder.Services.AddScoped<IAccountRepository, MongoAccountRepository>();
    builder.Services.AddScoped<ITransactionRepository, MongoTransactionRepository>();
}

// MediatR setup
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("Banking.Application"))
);

var app = builder.Build();

// Development only tools
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// ---- Seed DB ----
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
