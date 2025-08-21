using _Mafia_API;
using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Models;
using _Mafia_API.Models.DTOs;
using _Mafia_API.Services;
using Macross.Json.Extensions;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

internal class Program
{

    static Program()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            DotEnv.Load("dev.env");
    }

    private static void Main(string[] args)
    {

        Thread cleanupThread = new Thread(() => CleanUpDirectory("voice_dynamic"));
        cleanupThread.IsBackground = true;
        cleanupThread.Start();

        Console.WriteLine("Starting Mafia api");
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                              policy =>
                              {
                                  var corsOrigins = builder.Configuration.GetSection("corsConfig").Value;
                                  var env = builder.Environment.EnvironmentName;
                                  if (!string.IsNullOrEmpty(corsOrigins))
                                  {
                                      var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(o => o.Trim())
                                                              .ToArray();
                                      policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                  }
                                  else
                                  {
                                      // No configured origins; in development, allow localhost defaults, otherwise deny by default
                                      if (builder.Environment.IsDevelopment())
                                      {
                                          policy.WithOrigins("http://localhost:3000", "http://localhost:5005").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                      }
                                  }
                              });
        });


        //register services
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<RoomService>();
        builder.Services.AddTransient<GameService>();
        builder.Services.AddTransient<GameHub>();
        builder.Services.AddTransient<Scheduler>();
        builder.Services.AddTransient<AnnouncementService>();


        {
            var announcementService = new AnnouncementService();
            announcementService.EnsureAllGeneratedAsync().Wait();
        }


        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());

        });
        ;

        //add swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Mafia-API",
                Description = "Mafia-API",
            });
        });
        Console.WriteLine("Swagger");



        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddHttpContextAccessor();

        //enable signalR with proper JSON serialization for enums
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        }).AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseCors(MyAllowSpecificOrigins);


        app.Use((Context, Next) =>
        {
            const string tokenCookiePrefix = "user=";

            string? authCookie = null;

            if (Context.Request.Headers.Cookie.Count > 0)
            {
                foreach (var cookie in Context.Request.Headers.Cookie.ToString().Split(';').Select(s => s.Trim()))
                {
                    if (cookie.StartsWith(tokenCookiePrefix))
                    {
                        authCookie = cookie.Substring(tokenCookiePrefix.Length);
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(authCookie))
            {
                Context.Items["UserID"] = authCookie;
            }

            return Next();
        });

        app.UseRouting();
        app.UseMiddleware<ErrorHandlerMiddleware>();
        app.MapControllers();
        app.MapHub<GameHub>("/gameRealtimeHub");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(Options =>
            {
                Options.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.Run();

    }

    static void CleanUpDirectory(string directoryPath)
    {

        bool dev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        TimeSpan maxFileAge = dev ? TimeSpan.FromMinutes(30) : TimeSpan.FromDays(1);
        TimeSpan checkInterval = dev ? TimeSpan.FromMinutes(30) : TimeSpan.FromDays(1);
        while (true)
        {
            try
            {
                var files = Directory.GetFiles(directoryPath);

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    DateTime lastWriteTime = File.GetLastWriteTime(file);

                    if (DateTime.Now - lastWriteTime > maxFileAge && !fileName.Contains(".gitkeep"))
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted old file: {fileName}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during cleanup: {ex.Message}");
            }

            Thread.Sleep(checkInterval);
        }
    }

}


public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {

            Console.WriteLine(error.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new ResponseWrapper<string>(WrResponseStatus.InternalError));

            await response.WriteAsync(result);
        }
    }
}

public class DateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString(Format));
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }
}