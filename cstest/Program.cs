namespace CommandName
{
    using System;
    using System.IO;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using McMaster.Extensions.CommandLineUtils; // alternative to abandonware Microsoft.Extensions.CommandLineUtils.

    using Contentstack.Core;
    using Contentstack.AspNetCore;

    using Deliverystack.Core;
    using Deliverystack.Core.Models.Repositories;
    using Deliverystack.Core.Stackcontent.Configuration;

    public class Program
    {
        static void Main(string[] args)
        {
            Message($"{typeof(Program)} : Main()", true);
            CommandLineApplication app = new CommandLineApplication();

            try
            {
                app.Description = ".NET Core console app to "; //TODO
                app.Name = typeof(Program).Namespace;
                CommandOption help = app.HelpOption("-?|-h|--help");
                CommandOption dir = app.Option("-d|--d<value>",
                    "Path to directory containing appconfig.json",
                    CommandOptionType.SingleValue);
                app.OnExecute(() =>
                {
                    if (help.HasValue())
                    {
                        ExitMessage(app, null, ExitCode.HelpRequested);
                    }

                    string directory = Directory.GetCurrentDirectory();

                    if (dir.HasValue())
                    {
                        directory = dir.Value();
                    }

                    if (!Directory.Exists(directory))
                    {
                        ExitMessage(app,
                            $"{directory} does not exist or is not a subdirectory.",
                            ExitCode.DirectoryDoesNotExist);
                    }

                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(
                        $"{directory}\\appsettings.json",
                        optional: false,
                        reloadOnChange: true).Build();
                    Startup startup = new Startup();
                    ServiceCollection
                        serviceCollection = new ServiceCollection(); // dependency injection configuration 
                    startup.ConfigureServices(serviceCollection, configuration); // configuration
                    IServiceProvider provider = serviceCollection.BuildServiceProvider(); // caution: may be disposable 
                    ContentstackClient client = provider.GetService<ContentstackClient>();
//                    client.SerializerSettings.Converters.Add(); //TODO:
                    IRepository repository = provider.GetService<IRepository>();
                    Console.WriteLine(Environment.NewLine);
                    Message(typeof(ThingDoer) + ".DoThing()", true);
                    new ThingDoer().DoThing(client, repository as ContentstackRepository);
                    Message(typeof(ThingDoer) + ".DoThing() complete.");
                    Console.WriteLine(Environment.NewLine);
                });

                app.Execute(args);

            }
            catch (Exception ex)
            {
                new Instrument().Exception(ex, ex, "exception in Main()");
///                ExitMessage(app, $"{ex} : {ex.Message}", ExitCode.Exception, ex);
                ExitMessage(app, $"{ex} : {ex.Message}", ExitCode.Exception);
            }
        }
        
        private enum ExitCode
        {
            Success,
            Exception,
            HelpRequested,
            DirectoryDoesNotExist
        }
        
        private class Startup
        {
            public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                services.AddContentstack(configuration);
                Root.Core.Models.Configuration.IServiceCollectionExtensions.AddSerializationConfigurator(services, configuration);
                services.AddRepository(configuration);
            }
        }

        private static void ExitMessage(CommandLineApplication app,
            string message,
            Exception ex = null)
        {
            ExitMessage(app, message, null, ex);
        }

        private static void ExitMessage(CommandLineApplication app,
            string message,
            ExitCode? exitCode = null,
            Exception ex = null)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Console.WriteLine(Environment.NewLine + app.Name + " : " + message + Environment.NewLine);
            }
            else if (ex != null)
            {
                Console.WriteLine(ex.Message);
            }

            if (exitCode != null)
            {
                app.ShowHelp();
            }

            if (ex != null)
            {
                Console.WriteLine(Environment.NewLine + ex.GetType() + " : " + ex.Message);// + " : " + ex.Message);

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Console.WriteLine(Environment.NewLine + ex.GetType() + " : " + ex.Message);
                }
            }
            
            if (exitCode != null)
            {
                Environment.Exit((int) exitCode);
            }
        }

        private static void Message(string message, bool addLine = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"{DateTime.Now:h:mm:ss.fffffff} : {typeof(Program)} : " + message);
            }

            if (addLine)
            {
                Line();
            }
        }

        private static void Line()
        {
            Message("========================================================================================");
        }
    }
}