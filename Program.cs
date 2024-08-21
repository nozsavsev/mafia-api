using _Mafia_API;
using _Mafia_API.Helpers;
using _Mafia_API.Hubs;
using _Mafia_API.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class Program
{

    static Program()
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            DotEnv.Load("dev.env");
    }

    //class Replica
    //{
    //    public Replica(string File, string Text)
    //    {
    //        text = Text;
    //        file = File;
    //    }

    //    public string file;
    //    public string text;
    //}

    private static void Main(string[] args)
    {

        //Replica[] mp3Files =
        //{
        //  new Replica( "city_down_0" ,"Город засыпает" ),
        //  new Replica( "city_down_1" ,"А теперь, спать кожанные" ),


        //  new Replica( "slut_on_0" ,"Ну что шлюха? Открываем глазки, ротик, начинаем сосать" ),
        //  new Replica( "slut_on_1" ,"Время сосать шалава!" ),
        //  new Replica( "slut_on_2" ,"Шлюха открывает глаза." ),

        //  new Replica( "slut_off_0" ,"Ну что, насосалась? А теперь обратно спать." ),
        //  new Replica( "slut_off_1" ,"Шлюха закрывает глаза" ),
        //  new Replica( "slut_off_2" ,"Теперь шлюха спит" ),


        //  new Replica( "mafia_on_0" ,"Мафия просыпается" ),
        //  new Replica( "mafia_on_1" ,"Вечер в хату, мафия просыпается" ),
        //  new Replica( "mafia_on_2" ,"Подъем мафиози" ),

        //  new Replica( "mafia_off_0" ,"Мафия отправляется спать" ),
        //  new Replica( "mafia_off_1" ,"Мафиози засыпают" ),
        //  new Replica( "mafia_off_2" ,"Бандитам спасибо" ),


        //  new Replica( "doctor_on_0" ,"Доктор просыпается" ),
        //  new Replica( "doctor_on_1" ,"Доктор приехал!" ),
        //  new Replica( "doctor_on_2" ,"Вот беда, доктору не спится" ),

        //  new Replica( "doctor_off_0" ,"Доктор хочет спать" ),
        //  new Replica( "doctor_off_1" ,"Доктор, съебывает" ),
        //  new Replica( "doctor_off_2" ,"Доктора лишают лицензии на остаток ночи, за неподобающее обращение с трупами!" ),


        //  new Replica( "sherif_on_0" ,"Шериф, Шерииииф, вставай. ты обосрался!" ),
        //  new Replica( "sherif_on_1" ,"Wee-Woo, полиция приехала" ),
        //  new Replica( "sherif_on_2" ,"Я - Бэтмэн. Это мой город. тфу ты, не тот скрипт, шериф, это тебя" ),

        //  new Replica( "sherif_off_0" ,"Шериф пошел спать" ),
        //  new Replica( "sherif_off_1" ,"Коммисар уебывает" ),
        //  new Replica( "sherif_off_2" ,"Полиция наелась и спит" ),


        //  new Replica( "city_up_0" ,"Подъем кожанные." ),
        //  new Replica( "city_up_1" ,"Город встает" ),
        //  new Replica( "city_up_2" ,"Доброго времени суток" ),


        //  new Replica( "player_killed_pre_0" ,"Игрока" ),
        //  //{name}
        //  new Replica( "player_killed_pst_0" ,"убила мафия" ),


        //  new Replica( "player_killed_pre_1" ,"Игрок" ),
        //  //{name}
        //  new Replica( "player_killed_pst_1" ,"был убит мафией" ),


        //  new Replica( "player_killed_pre_2" ,"Игрок" ),
        //  //{name}
        //  new Replica( "player_killed_pst_3" ,"трагически погиб" ),


        //  new Replica( "player_killed_cmnt_0" ,"Womp Womp кожанный" ),
        //  new Replica( "player_killed_cmnt_1" ,"Womp Womp" ),
        //  new Replica( "player_killed_cmnt_2" ,"Какая жалость" ),
        //  new Replica( "player_killed_cmnt_3" ,"плаки плаки" ),


        //  new Replica( "player_healed_pre_0" ,"Доктор вылечил игрока" ),
        //  //{name}

        //  new Replica( "player_healed_cmnt_0" ,"жаль, я желал ему смерти" ),
        //  new Replica( "player_healed_cmnt_1" ,"" ),


        //  new Replica( "vote_killed_0" ,"По результатам голосования убили игрока" ),//{name}
        //  new Replica( "vote_killed_1" ,"Почему вы так не любите" ),                //{name}
        //  new Replica( "vote_killed_2" ,"По результатам голосования убили" ),       //{name}

        //};


        //foreach (var mp3 in mp3Files)
        //{
        //    GenerateText(mp3.text, mp3.file);
        //    Console.WriteLine($"Generated {mp3.file}");
        //}

        Thread cleanupThread = new Thread(() => CleanUpDirectory("voice_dynamic"));
        cleanupThread.IsBackground = true;
        cleanupThread.Start();

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

    static void CleanUpDirectory(string directoryPath)
    {

        bool dev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        TimeSpan maxFileAge = dev ? TimeSpan.FromSeconds(120) : TimeSpan.FromDays(3);
        TimeSpan checkInterval = dev ? TimeSpan.FromSeconds(10) : TimeSpan.FromDays(1);
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