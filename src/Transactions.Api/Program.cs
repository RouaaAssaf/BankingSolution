using Banking.Application.Abstractions;
using Banking.Infrastructure.Repositories.Mongo;
using Banking.Messaging;
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

    // Register only the repositories needed for Transactions API
    builder.Services.AddScoped<IAccountRepository, MongoAccountRepository>();
    builder.Services.AddScoped<ITransactionRepository, MongoTransactionRepository>();
    builder.Services.AddScoped<ICustomerRepository, MongoCustomerRepository>();
   

}

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("Banking.Application"));
});


// --- Messaging ---
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// --- Register consumers ---
builder.Services.AddHostedService<CustomerCreatedConsumer>();

builder.Services.AddCors();
// --- Build app ---
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Enable CORS for all origins globally
app.UseCors(policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
);

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
app.MapFallbackToFile("index.html");
app.Run();
