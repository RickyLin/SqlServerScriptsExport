## Overview
Create the foundational structure for a .NET 9 console application that generates SQL definition scripts from SQL Server databases. The application should extract and organize SQL scripts for views, stored procedures, functions, and triggers into categorized folders.

## Files to Create

### 1. Program.cs
Main entry point with error handling and application orchestration.

```csharp
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
                var generator = new SqlScriptGenerator(config);
                
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
```

### 2. DatabaseConfig.cs
Configuration class for database connection settings.

```csharp
using Microsoft.Data.SqlClient;

namespace SqlServerScriptsExport
{
    public class DatabaseConfig
    {
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string OutputPath { get; set; } = "./Scripts";
        public bool UseTrustedConnection { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = ServerName,
                InitialCatalog = DatabaseName,
                IntegratedSecurity = UseTrustedConnection
            };

            if (!UseTrustedConnection)
            {
                builder.UserID = Username;
                builder.Password = Password;
            }

            return builder.ConnectionString;
        }
    }
}
```

### 3. ArgumentParser.cs
Command line argument parsing and interactive configuration.

```csharp
using System;

namespace SqlServerScriptsExport
{
    public static class ArgumentParser
    {
        public static DatabaseConfig? ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                // Interactive mode - prompt for connection details
                return GetConfigInteractively();
            }

            // Command line mode
            var config = new DatabaseConfig();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-s":
                    case "--server":
                        if (i + 1 < args.Length)
                            config.ServerName = args[++i];
                        break;
                    case "-d":
                    case "--database":
                        if (i + 1 < args.Length)
                            config.DatabaseName = args[++i];
                        break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                            config.OutputPath = args[++i];
                        break;
                    case "-t":
                    case "--trusted":
                        config.UseTrustedConnection = true;
                        break;
                    case "-u":
                    case "--username":
                        if (i + 1 < args.Length)
                            config.Username = args[++i];
                        break;
                    case "-p":
                    case "--password":
                        if (i + 1 < args.Length)
                            config.Password = args[++i];
                        break;
                    case "-h":
                    case "--help":
                        return null;
                }
            }

            return ValidateConfig(config) ? config : null;
        }

        private static DatabaseConfig GetConfigInteractively()
        {
            var config = new DatabaseConfig();

            Console.WriteLine("Enter database connection details:");
            Console.WriteLine();

            Console.Write("SQL Server name (e.g., localhost, .\\SQLEXPRESS): ");
            config.ServerName = Console.ReadLine()?.Trim();

            Console.Write("Database name: ");
            config.DatabaseName = Console.ReadLine()?.Trim();

            Console.Write("Use Windows Authentication? (y/n) [y]: ");
            var useWindows = Console.ReadLine()?.Trim().ToLower();
            config.UseTrustedConnection = string.IsNullOrEmpty(useWindows) || useWindows == "y" || useWindows == "yes";

            if (!config.UseTrustedConnection)
            {
                Console.Write("Username: ");
                config.Username = Console.ReadLine()?.Trim();

                Console.Write("Password: ");
                config.Password = ReadPassword();
            }

            Console.Write("Output directory [./Scripts]: ");
            var outputPath = Console.ReadLine()?.Trim();
            config.OutputPath = string.IsNullOrEmpty(outputPath) ? "./Scripts" : outputPath;

            Console.WriteLine();

            return ValidateConfig(config) ? config : throw new ArgumentException("Invalid configuration provided.");
        }

        private static string ReadPassword()
        {
            var password = string.Empty;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private static bool ValidateConfig(DatabaseConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ServerName))
            {
                Console.WriteLine("Error: Server name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.DatabaseName))
            {
                Console.WriteLine("Error: Database name is required.");
                return false;
            }

            if (!config.UseTrustedConnection)
            {
                if (string.IsNullOrWhiteSpace(config.Username))
                {
                    Console.WriteLine("Error: Username is required when not using Windows Authentication.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(config.Password))
                {
                    Console.WriteLine("Error: Password is required when not using Windows Authentication.");
                    return false;
                }
            }

            return true;
        }

        public static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  SqlScriptGenerator [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -s, --server <server>      SQL Server name or instance");
            Console.WriteLine("  -d, --database <database>  Database name");
            Console.WriteLine("  -o, --output <path>        Output directory (default: ./Scripts)");
            Console.WriteLine("  -t, --trusted              Use Windows Authentication");
            Console.WriteLine("  -u, --username <username>  SQL Server username");
            Console.WriteLine("  -p, --password <password>  SQL Server password");
            Console.WriteLine("  -h, --help                 Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SqlScriptGenerator -s localhost -d MyDatabase -t");
            Console.WriteLine("  SqlScriptGenerator -s .\\SQLEXPRESS -d MyDatabase -u sa -p password");
            Console.WriteLine();
            Console.WriteLine("If no arguments are provided, interactive mode will be used.");
        }
    }
}
```

### 4. SqlScriptGenerator.cs
Core script generation logic with directory structure creation.

```csharp
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlServerScriptsExport
{
    public class SqlScriptGenerator
    {
        private readonly DatabaseConfig _config;

        public SqlScriptGenerator(DatabaseConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task GenerateAllScriptsAsync()
        {
            Console.WriteLine($"Connecting to SQL Server: {_config.ServerName}");
            Console.WriteLine($"Database: {_config.DatabaseName}");
            Console.WriteLine($"Output directory: {_config.OutputPath}");
            Console.WriteLine();

            // Create output directory structure
            CreateDirectoryStructure();

            // TODO: Implement the actual script generation logic
            // This will include:
            // - Connecting to SQL Server
            // - Querying system views for object definitions
            // - Generating scripts for Views, StoredProcedures, Functions, Triggers
            // - Organizing files into appropriate folders

            Console.WriteLine("Script generation logic will be implemented in the next phase...");
            
            await Task.Delay(1000); // Placeholder for actual async operations
        }

        private void CreateDirectoryStructure()
        {
            var directories = new[]
            {
                _config.OutputPath,
                Path.Combine(_config.OutputPath, "Views"),
                Path.Combine(_config.OutputPath, "StoredProcedures"),
                Path.Combine(_config.OutputPath, "Functions_TableValued"),
                Path.Combine(_config.OutputPath, "Functions_ScalarValued"),
                Path.Combine(_config.OutputPath, "Triggers")
            };

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Console.WriteLine($"Created directory: {directory}");
                }
            }
        }
    }
}
```

### 5. SqlScriptGenerator.csproj
Project file with .NET 9 configuration and dependencies.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyTitle>SQL Server Database Script Generator</AssemblyTitle>
    <AssemblyDescription>A .NET 9 console application to generate SQL definition scripts from SQL Server databases</AssemblyDescription>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    <RootNamespace>SqlServerScriptsExport</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
  </ItemGroup>

</Project>
```

### 6. README.md
Comprehensive documentation for the project.

```markdown
# SQL Server Database Script Generator

A .NET 9 console application that generates SQL definition scripts for all database objects from a SQL Server database.

## Project Structure

```
SqlServerScriptsExport/
├── Program.cs                 # Main entry point
├── DatabaseConfig.cs          # Database configuration class
├── ArgumentParser.cs          # Command line and interactive argument parsing
├── SqlScriptGenerator.cs      # Core script generation logic
├── SqlScriptGenerator.csproj  # Project file
└── README.md                  # This file
```

## Features

- Extracts definitions for Views, Stored Procedures, Functions, and Triggers
- Organizes scripts into category-specific folders:
  - `Views/` - All view definitions
  - `StoredProcedures/` - All stored procedure definitions
  - `Functions_TableValued/` - All table-valued function definitions
  - `Functions_ScalarValued/` - All scalar-valued function definitions
  - `Triggers/` - All trigger definitions (organized by table name)

## Usage

### Interactive Mode
Run the application without arguments to use interactive mode:

```bash
dotnet run
```

### Command Line Mode
Use command line arguments for automated execution:

```bash
# Using Windows Authentication
dotnet run -- -s localhost -d MyDatabase -t -o ./OutputScripts

# Using SQL Server Authentication
dotnet run -- -s .\SQLEXPRESS -d MyDatabase -u sa -p mypassword -o ./OutputScripts
```

### Command Line Options

- `-s, --server <server>` - SQL Server name or instance (required)
- `-d, --database <database>` - Database name (required)
- `-o, --output <path>` - Output directory (default: ./Scripts)
- `-t, --trusted` - Use Windows Authentication
- `-u, --username <username>` - SQL Server username (required if not using -t)
- `-p, --password <password>` - SQL Server password (required if not using -t)
- `-h, --help` - Show help message

## Requirements

- .NET 9.0 or later
- Access to SQL Server database
- Appropriate permissions to read database schema

## Dependencies

- Microsoft.Data.SqlClient 5.2.2 - Modern SQL Server client library

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```

## Output Structure

The generated scripts will be organized as follows:

```
Scripts/
├── Views/
│   ├── view1.sql
│   └── view2.sql
├── StoredProcedures/
│   ├── proc1.sql
│   └── proc2.sql
├── Functions_TableValued/
│   ├── function1.sql
│   └── function2.sql
├── Functions_ScalarValued/
│   ├── function3.sql
│   └── function4.sql
└── Triggers/
    ├── Table1/
    │   └── trigger1.sql
    └── Table2/
        └── trigger2.sql
```

## Project Configuration

- **Namespace**: SqlServerScriptsExport
- **ImplicitUsings**: Disabled - All using statements are explicitly declared
- **SQL Client**: Uses Microsoft.Data.SqlClient instead of legacy System.Data.SqlClient
- **Target Framework**: .NET 9.0
- **Architecture**: Clean separation of concerns with dedicated classes for different responsibilities

## Class Responsibilities

- **Program**: Main entry point and error handling
- **DatabaseConfig**: Database connection configuration and connection string building
- **ArgumentParser**: Command line argument parsing and interactive configuration
- **SqlScriptGenerator**: Core logic for generating and organizing SQL scripts

## Next Steps

This is the initial foundation. The next phase will implement:
1. SQL Server connection and querying
2. Object definition extraction
3. Script file generation
4. Proper error handling and logging
```

## Technical Requirements

### Project Configuration
- **Target Framework**: .NET 9.0
- **Namespace**: SqlServerScriptsExport
- **ImplicitUsings**: Disabled (all using statements explicit)
- **Nullable**: Enabled
- **Dependencies**: Microsoft.Data.SqlClient 5.2.2

### Architecture
- Clean separation of concerns with single responsibility classes
- Support for both interactive and command-line modes
- Secure password handling in interactive mode
- Comprehensive error handling and validation

### Output Structure
The application will create the following directory structure:
- `Views/` - All view definitions
- `StoredProcedures/` - All stored procedure definitions  
- `Functions_TableValued/` - All table-valued function definitions
- `Functions_ScalarValued/` - All scalar-valued function definitions
- `Triggers/` - All trigger definitions (organized by table name)

## Acceptance Criteria

- [ ] All files are created with proper namespace and structure
- [ ] Project builds successfully with .NET 9
- [ ] Application runs and creates directory structure
- [ ] Command line arguments are properly parsed
- [ ] Interactive mode prompts for database connection details
- [ ] Connection string is properly constructed
- [ ] Error handling displays meaningful messages
- [ ] README.md provides comprehensive documentation

## Next Phase
After this foundation is complete, the next phase will implement the actual SQL Server connection, querying system views, and script generation logic.
