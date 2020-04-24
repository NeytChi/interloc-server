using Serilog;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace common
{
    public class Program
    {
        public static bool requestView = false;    
        public static IConfigurationRoot serverConfig;  
        
        public static void Main(string[] args)
        {
            if (args != null)
            {                
                if (args.Length >= 1)
                {
                    if (args[0] == "-c") {
                        using (Context context = new Context(false))
                            context.Database.EnsureDeleted();
                        Console.WriteLine("Database was deleted.");
                        return;
                    }
                    if (args[0] == "-v")
                        requestView = true;
                     if (args[0] == "-a") {
                        if (args.Length >= 2)
                            addAdmin(args[1], args[2]);
                        return;
                    }
                }
            }
            createDatabase();
            serverConfig = serverConfiguration();

            string IP = serverConfig.GetValue<string>("ip");
            int portHttp = serverConfig.GetValue<int>("port_http");
            int portHttps = serverConfig.GetValue<int>("port_https");
            
            var certificateSettings = serverConfig.GetSection("certificateSettings");
            string certificateFileName = certificateSettings.GetValue<string>("filename");
            string certificatePassword = certificateSettings.GetValue<string>("password");

            var certificate = new X509Certificate2(certificateFileName, certificatePassword);

            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options 
                =>  {
                        options.AddServerHeader = false;
                        options.Listen(IPAddress.Parse(IP), portHttp);
                        options.Listen(IPAddress.Parse(IP), portHttps, listenOptions 
                        => {
                            listenOptions.UseHttps(certificate);
                        });
                    })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://" + IP + ":" + portHttp + "/",
                    "https://" + IP + ":" + portHttps + "/")
                .Build()
                .Run();   
        }
        public static void createDatabase()
        {
            Context context = new Context(false);
            context.Database.EnsureCreated();
        }
        public static IConfigurationRoot serverConfiguration()
        {
            if (serverConfig == null) {
                serverConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("server.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"server.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", 
                    optional: true, reloadOnChange: true)
                .Build();
            }
            return serverConfig;
        }
        public static void addAdmin(string adminEmail, string adminPassword)
        {
            Admin admin;
            string message = string.Empty;
            AdminModule module = new AdminModule(new LoggerConfiguration()
                .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
                .CreateLogger(), new Context(false));
            if ((admin = module.CreateAdmin(adminEmail, adminPassword, 0, ref message)) != null) 
                Console.WriteLine("Admin with email -> '" + adminEmail + "' was created.");
            else
                Console.WriteLine(message);
        }
    }
}
