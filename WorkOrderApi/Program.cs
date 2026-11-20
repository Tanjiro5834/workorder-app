using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WorkOrderApi.Data;
using WorkOrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o => 
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(o => 
        o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();

builder.Services.AddScoped<IBranchService, BranchService>();

builder.Services.AddScoped<IIncidentReportService, IncidentReportService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.UseCors("VueFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
