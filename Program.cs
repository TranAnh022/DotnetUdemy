using DotnetAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors((option) =>
    {
        option.AddPolicy("DevCors", (corsBuilder) =>
        {
            corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
        option.AddPolicy("ProdCors", (corsBuilder) =>
        {
            corsBuilder.WithOrigins("https://myProductionSite.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

builder.Services.AddScoped<IUserRepository, UserRepository>(); //add scope call for repositoty

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
