using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Data.Context;
using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders().AddConsole();
builder.Services.AddDbContext<DatabaseContext>(opt =>
{
    opt.UseInMemoryDatabase("PaymentGatewayDb");
    opt.EnableSensitiveDataLogging();
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
builder.Services.Configure<MounteBankConfig>(builder.Configuration.GetSection("MounteBank"));
builder.Services.AddScoped<IValidator<PostPaymentRequest>, PostPaymentValidator>();
builder.Services.AddScoped<IPaymentsService, PaymentsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }
