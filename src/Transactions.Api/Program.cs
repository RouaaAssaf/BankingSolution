using Banking.Application.Abstractions;
using Banking.Infrastructure.Data;
using Banking.Infrastructure.Repositories.Mongo;
using Banking.Messaging;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;
using Transactions.Api.Consumers; 

var builder = WebApplication.CreateBuilder(args);

// --- Controllers & JSON options ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Logging ---
builder.Services.AddLogging();

// --- Database selection ---
var useDatabase = builder.Configuration["UseDatabase"];


 if (useDatabase == "Mongo")
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

// --- MediatR ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("Banking.Application"))
);

// --- Messaging ---
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// --- Register consumers (correct namespaces) ---
builder.Services.AddHostedService<CustomerCreatedConsumer>();


// --- Build app ---
var app = builder.Build();

// --- Middleware ---
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

// --- Seed database ---

 if (useDatabase == "Mongo")
{
    using var scope = app.Services.CreateScope();
    var mongo = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    await MongoSeeder.SeedAsync(mongo);
}

app.Run();
