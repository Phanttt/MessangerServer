using MessangerServer.Controllers;
using MessangerServer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<MessangerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MessangerContext")));

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => builder
   .SetIsOriginAllowed(_ => true)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials());

app.UseHttpsRedirection();



app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<DataHub>("/hubs/chat");
});

app.Run();
