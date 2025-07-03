# SQL Server Database Script Generator

A comprehensive .NET 9 console application that generates SQL definition scripts for all database objects from a SQL Server database with advanced error handling, logging, and progress reporting.

## Project Structure

```
SqlServerScriptsExport/
├── Program.cs                      # Main entry point with enhanced error handling
├── DatabaseConfig.cs               # Database configuration with connection timeout support
├── ArgumentParser.cs               # Enhanced command line and interactive argument parsing
├── SqlServerScriptsExport.cs       # Main application class with complete script generation logic
├── DatabaseObject.cs               # Data model for database objects
├── Logger.cs                       # Comprehensive logging infrastructure
├── ProgressReporter.cs             # Progress tracking and reporting
├── FileHelper.cs                   # File operations and script generation utilities
├── AppOptions.cs                   # Application options configuration
├── SqlServerScriptsExport.csproj   # Project file
└── README.md                       # This file
```

## Features

### Core Functionality
- **Complete Object Extraction**: Extracts definitions for Views, Stored Procedures, Functions (Scalar and Table-valued), and Triggers
- **Intelligent Organization**: Organizes scripts into category-specific folders with proper naming conventions
- **Configurable Script Headers**: Optional professional headers with metadata for generated scripts
- **Connection Testing**: Validates database connectivity before processing begins
- **Atomic File Operations**: Uses temporary files with atomic moves to prevent data corruption

### Advanced Error Handling
- **Specific SQL Error Handling**: Provides meaningful error messages for common SQL Server errors
- **File System Error Management**: Handles disk space, permissions, and path-related issues
- **Graceful Degradation**: Continues processing when possible, with detailed error reporting
- **Retry Mechanisms**: Built-in handling for transient failures

### Logging and Progress
- **Structured Logging**: Multiple log levels (Debug, Info, Warning, Error) with timestamps
- **File and Console Output**: Dual logging support with optional file output
- **Progress Reporting**: Real-time progress bars with ETA calculations
- **Comprehensive Summary**: Detailed statistics upon completion

### Output Organization
- `Views/` - All view definitions
- `StoredProcedures/` - All stored procedure definitions  
- `Functions_TableValued/` - Table-valued function definitions
- `Functions_ScalarValued/` - Scalar-valued function definitions
- `[TableName]/Triggers/` - Trigger definitions with table name as top-level folder, then "Triggers" subfolder

## Usage

### Interactive Mode
Run the application without arguments to use enhanced interactive mode:

```bash
dotnet run
```

### Command Line Mode
Use command line arguments for automated execution with advanced options:

```bash
# Basic usage with Windows Authentication
dotnet run -- -s localhost -d MyDatabase -t -o ./OutputScripts

# Advanced usage with verbose logging and file output
dotnet run -- -s .\SQLEXPRESS -d MyDatabase -u sa -p mypassword --verbose --log-file export.log

# Quiet mode for automation
dotnet run -- -s localhost -d MyDatabase -t --quiet --no-progress
```

### Command Line Options

#### Connection Options
- `-s, --server <server>` - SQL Server name or instance (required)
- `-d, --database <database>` - Database name (required)
- `-o, --output <path>` - Output directory (default: ./Scripts)
- `-t, --trusted` - Use Windows Authentication
- `-u, --username <username>` - SQL Server username (required if not using -t)
- `-p, --password <password>` - SQL Server password (required if not using -t)
- `--timeout <seconds>` - Connection timeout in seconds (default: 30)

#### Script Generation Options
- `--header` - Include script headers in generated files (disabled by default)

#### Logging and Output Options
- `--verbose` - Enable detailed logging and debug information
- `--quiet` - Minimize console output (errors and warnings still shown)
- `--no-progress` - Disable progress reporting
- `--log-file <path>` - Write logs to specified file
- `-h, --help` - Show comprehensive help message
## Requirements

- .NET 9.0 or later
- Access to SQL Server database (SQL Server 2012 or later recommended)
- Appropriate permissions to read database schema and system views
- Sufficient disk space for generated script files

## Dependencies

- **Microsoft.Data.SqlClient 5.2.2** - Modern SQL Server client library with enhanced security and performance

## Building

```bash
dotnet build
```

## Running

```bash
# Interactive mode
dotnet run

# Command line mode
dotnet run -- [options]
```

## Output Structure

The generated scripts are organized with professional headers and metadata:

```
Scripts/
├── Views/
│   ├── CustomerView.sql           # Each file contains object definition with metadata header
│   └── ProductView.sql
├── StoredProcedures/
│   ├── GetCustomers.sql
│   └── UpdateProduct.sql
├── Functions_TableValued/
│   ├── GetOrdersByDate.sql
│   └── CustomerOrderHistory.sql
├── Functions_ScalarValued/
│   ├── CalculateTotal.sql
│   └── FormatCurrency.sql
├── Customers/                     # Table name as top-level folder
│   └── Triggers/                  # Hard-coded "Triggers" folder
│       └── CustomerAuditTrigger.sql
└── Orders/                        # Table name as top-level folder
    └── Triggers/                  # Hard-coded "Triggers" folder
        ├── OrderInsertTrigger.sql
        └── OrderUpdateTrigger.sql
```

### Generated Script Format

Each generated script can optionally include a professional header with metadata (when using the `--header` option):

```sql
------------------------------------------------------------------------------
-- SQL Server Script Export
-- Object Name: CustomerView
-- Object Type: View
-- Source Database: MyDatabase
-- Generated: 2025-06-25 12:30:15 UTC
-- 
-- This script was automatically generated by SqlServerScriptsExport
------------------------------------------------------------------------------

CREATE VIEW [dbo].[CustomerView] AS
SELECT CustomerID, CustomerName, Email
FROM Customers
WHERE IsActive = 1
```

## Error Handling

The application provides comprehensive error handling with specific guidance:

- **Connection Errors**: Clear messages for server not found, authentication failures, database access issues
- **Permission Errors**: Detailed information about required database permissions
- **File System Errors**: Guidance for disk space, directory permissions, and path issues
- **SQL Errors**: Specific error codes and recommended solutions

## Logging Levels

- **Debug** (`--verbose`): Detailed execution information, SQL queries, file operations
- **Info** (default): General progress and summary information
- **Warning**: Non-critical issues that don't prevent execution
- **Error**: Critical failures with detailed context

## Performance Considerations

- **Async Operations**: All database and file operations use async/await patterns
- **Connection Pooling**: Efficient database connection management
- **Memory Management**: Proper disposal of resources and minimal memory footprint  
- **Batch Processing**: Optimized queries for large databases

## Security Features

- **Secure Connection Strings**: Uses SqlConnectionStringBuilder for safe connection string construction
- **SQL Injection Protection**: Parameterized queries throughout
- **Authentication Support**: Both Windows Authentication and SQL Server Authentication
- **Error Sanitization**: Sensitive information excluded from error messages

## Project Configuration

- **Namespace**: SqlServerScriptsExport
- **ImplicitUsings**: Disabled - All using statements are explicitly declared for clarity
- **SQL Client**: Uses Microsoft.Data.SqlClient for enhanced security and modern features
- **Target Framework**: .NET 9.0 for latest performance improvements
- **Architecture**: Clean separation of concerns with SOLID principles

## Class Responsibilities

- **Program**: Entry point, argument processing, and top-level error handling
- **DatabaseConfig**: Connection configuration with timeout and authentication options
- **ArgumentParser**: Comprehensive command-line and interactive argument processing
- **SqlServerScriptsExport**: Core script generation orchestration and database operations
- **DatabaseObject**: Strongly-typed model for database object metadata
- **Logger**: Multi-level logging with console and file output support
- **ProgressReporter**: Real-time progress tracking with ETA calculations
- **FileHelper**: File naming, sanitization, and atomic write operations
- **AppOptions**: Application configuration and feature flags

## Examples

### Basic Database Export
```bash
dotnet run -- -s localhost -d AdventureWorks -t
```

### Export with Script Headers
```bash
dotnet run -- -s localhost -d AdventureWorks -t --header
```

### Production-Ready Export with Logging and Headers
```bash
dotnet run -- -s PROD-SQL01 -d MyApp_Production -u service_account -p secret123 --verbose --log-file "export_$(date +%Y%m%d).log" --timeout 60 --header
```

### Automated Export (CI/CD)
```bash
dotnet run -- -s localhost -d TestDB -t --quiet --no-progress -o ./database-scripts
```

This implementation successfully addresses all requirements from GitHub issues #2, #3, #4, and #5, providing a professional-grade database script export tool with comprehensive error handling, logging, progress reporting, and robust file generation capabilities.