# Event Office

## .NET8
https://learn.microsoft.com/en-us/dotnet/core/install/macosdo
https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code?utm_source=vs-code&amp;utm_medium=referral&amp;utm_campaign=sdk-install

### Project foundation
In .NET 8, templates have been introduced to provide pre-configured project structures and code scaffolding. Templates are used to bootstrap new projects with the necessary files and configurations.

.NET templates are based on the template engine and project templates concepts. The template engine is responsible for processing templates and generating code or project files based on the provided input. Project templates are the specific templates that define the structure and content of a project.

With .NET 8, you can create various types of projects, such as console applications, web applications, class libraries, and more, using the built-in project templates. These templates come with pre-defined file structures, configuration files, and default code templates.

To get a list of templates run `dotnet new --list` once you have installed .NET on your computer.

__This project was bootstrapped using `dotnet new webapi`__
__The .gitignore file was bootstrapped using `dotnet new gitignore`__

## Running
### Prerequisites

- .NET SDK (version X.X.X)

### Getting Started

1. Clone the repository:

2. Navigate to the project directory:

### Running Locally
```
To run a .NET API locally for testing, you can follow these steps:

Open a terminal or command prompt.
Navigate to the directory where your API project is located.
Build the project by running the following command:
Copy
Insert
dotnet build
Once the build is successful, navigate to the output directory. By default, the output directory is the bin folder within your project directory.
Run the API using the following command:
Copy
Insert
dotnet run
The API will start running, and you should see output indicating that the server is listening on a specific port (usually http://localhost:5000 or https://localhost:5001).
You can now test your API by making HTTP requests to the specified endpoint. For example, if you have an API endpoint /api/values, you can access it by opening a web browser and navigating to http://localhost:5000/api/values
```
```
dotnet add package Npgsql
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```