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
            Console.WriteLine($"Started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine();

            try
            {
                // Parse command line arguments
                var config = ArgumentParser.ParseArguments(args);
                
                if (config == null)
                {
                    ArgumentParser.ShowUsage();
                    return;
                }

                // Initialize the script generator
                var generator = new SqlServerScriptsExport(config);
                
                // Generate all scripts
                await generator.GenerateAllScriptsAsync();
                
                Console.WriteLine();
                Console.WriteLine("Script generation completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
