using RoutingService.Application.Extenstion;
using RoutingService.Infrastructure.BackgroundJobs;
using RoutingService.Infrastructure.Extenstion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);



builder.Services.AddHostedService<RouteTrackingJob>();




builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});





var app = builder.Build();




app.UseCors("FrontendPolicy");





if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseAuthorization();

app.MapControllers();



app.Run();