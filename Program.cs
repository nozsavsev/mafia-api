using _Mafia_API;
using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Services;
using Bogus;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;


internal class Program
{
    static Program()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            DotEnv.Load("dev.env");
    }



    private static void Main(string[] args)
    {

        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        Console.WriteLine("Starting Mafia api");
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                              policy =>
                              {
                                  if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                                      policy.WithOrigins("http://localhost:3000",
                                                      "http://localhost:5000",
                                                      "https://mafia.nozsa.com",
                                                      "https://mafia-api.nozsa.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                  else

                                      policy.WithOrigins("https://mafia-api.nozsa.com",
                                                      "https://mafia.nozsa.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                              });
        });

        //register services
        builder.Services.AddTransient<UserService>();
        builder.Services.AddTransient<RoomService>();
        builder.Services.AddTransient<RoomService>();


        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());

        }); ;

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

        //enable signalR
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseCors(MyAllowSpecificOrigins);


        app.Use((Context, Next) =>
        {
            const string tokenCookiePrefix = "user=";

            string authCookie = null;

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

            Console.WriteLine(Context.Items["UserID"]);

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
            }); app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.Run();

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