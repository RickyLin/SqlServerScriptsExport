using System;
using System.Threading.Tasks;

namespace SqlServerScriptsExport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("SQL Server Database Script Generator");
            Console.WriteLine("====================================");
            Console.WriteLine($"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            try
            {
                // Parse command line arguments
                var (config, options) = ArgumentParser.ParseArguments(args);
                
                if (options?.ShowHelp == true || config == null)
                {
                    ArgumentParser.ShowUsage();
                    return;
                }

                // Apply connection timeout from options if provided
                if (options != null && options.ConnectionTimeout != 30)
                {
                    config.ConnectionTimeout = options.ConnectionTimeout;
                }

                // Initialize logger
                var logLevel = options?.Verbose == true ? LogLevel.Debug : LogLevel.Info;
                var logger = new Logger(logLevel, options?.Verbose ?? false, options?.Quiet ?? false, options?.LogFilePath);
                
                // Initialize progress reporter
                var showProgress = options?.NoProgress != true;
                var progressReporter = new ProgressReporter(showProgress, logger);

                // Initialize the script generator
                var generator = new SqlServerScriptsExport(config, logger, progressReporter, options ?? new AppOptions());
                
                // Generate all scripts
                await generator.GenerateAllScriptsAsync();
                
                logger.LogInfo("");
                logger.LogInfo("Script generation completed successfully!");
            }
            catch (Exception ex)
            {
                // Create a basic logger for error reporting if we don't have one
                var errorLogger = new Logger(LogLevel.Error, verbose: true, quiet: false);
                errorLogger.LogError("Application failed", ex);
                Environment.Exit(1);
            }

            if (args.Length == 0) // Only wait for keypress in interactive mode
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
