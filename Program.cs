
using Amazon.DynamoDBv2;
using webchat.Service;

namespace WsChatApi
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
            builder.Services.AddSingleton<MessageService>();
            builder.Services.AddSingleton<WebSocketService>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<UploadService>();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("*")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod();
                    });
            });
            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            var webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(2) };

            app.UseWebSockets(webSocketOptions);
            //app.UseHttpsRedirection();
            
            app.UseAuthorization();


            app.MapControllers();
            app.UseCors();

            app.Run();
        }
    }
}
