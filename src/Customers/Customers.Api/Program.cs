using Customers.Application.Interfaces;
using Banking.Messaging;
using Customers.Api.Consumers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;


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

    
    builder.Services.AddScoped<ICustomerRepository, MongoCustomerRepository>();
}

// --- MediatR ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("Customers.Application"))
);

// --- Messaging ---
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// --- Register consumers ---
builder.Services.AddHostedService<TransactionCreatedConsumer>();

builder.Services.AddCors();
// --- Build app ---
var app = builder.Build();

// Enable CORS for all origins globally
app.UseCors(policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
);

// --- Middleware ---

app.UseMiddleware<ExceptionMiddleware>();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customers API V1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

// --- Seed database ---
if (useDatabase == "Mongo")
{
    using var scope = app.Services.CreateScope();
    var mongo = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    //await MongoSeeder.SeedAsync(mongo);
}

app.Run();
