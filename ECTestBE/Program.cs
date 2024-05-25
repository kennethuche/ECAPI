using AutoMapper;
using ECTest.Service.Abstract;
using ECTest.Service.Context;
using ECTest.Service.Interface;
using ECTestBE.Filters;
using ECTestBE.Mapper;
using ECTestBE.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
                {
                    options.Filters.Add(new ApiExceptionFilter());
                })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Use camelCase for property names
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Convert enums to string
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // Ignore null properties
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In production, modify this with the actual domains
builder.Services.AddCors(o => o.AddPolicy("default", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddLogging();
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseInMemoryDatabase("StudentCourseDB"));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();

Log.Information("Starting the application...");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthorization();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.MapControllers();

app.Run();

Log.Information("Application started successfully.");

var mapper = app.Services.GetRequiredService<IMapper>();
mapper.ConfigurationProvider.AssertConfigurationIsValid(); // Ensure mapping configuration is valid
mapper.ConfigurationProvider.CompileMappings();