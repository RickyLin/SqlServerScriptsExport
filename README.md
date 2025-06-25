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