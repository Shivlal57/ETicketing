using ETicketing.Api.BackgroundServices;
using ETicketing.Api.Data;
using ETicketing.Api.Entities;
using ETicketing.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentHandler, CreditCardHandler>();
builder.Services.AddScoped<IPaymentHandler, QRHandler>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();

builder.Services.AddHostedService<QrPaymentConfirmationWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.MapControllers();

app.Run();