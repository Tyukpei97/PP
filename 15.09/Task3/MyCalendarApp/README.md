# MyCalendarApp

MyCalendarApp is an interactive event calendar application built using C# and .NET 8.0 with WinForms. This application allows users to manage their events efficiently, providing features for adding, editing, and deleting events.

## Features

- User-friendly interface for managing events.
- Ability to add, edit, and delete events.
- Events are stored and retrieved in JSON format.
- Supports viewing events by date.

## Project Structure

```
MyCalendarApp
├── MyCalendarApp.sln
├── global.json
├── .gitignore
├── src
│   └── MyCalendarApp
│       ├── MyCalendarApp.csproj
│       ├── Program.cs
│       ├── MainForm.cs
│       ├── MainForm.Designer.cs
│       ├── MainForm.resx
│       ├── Models
│       │   ├── EventItem.cs
│       │   └── CalendarModel.cs
│       ├── Services
│       │   ├── EventService.cs
│       │   └── PersistenceService.cs
│       ├── Controllers
│       │   └── CalendarController.cs
│       ├── Views
│       │   └── EventEditorForm.cs
│       └── Properties
│           ├── Resources.resx
│           └── AssemblyInfo.cs
├── tests
│   └── MyCalendarApp.Tests
│       ├── MyCalendarApp.Tests.csproj
│       └── EventServiceTests.cs
└── README.md
```

## Installation

1. Clone the repository:
   ```
   git clone <repository-url>
   ```
2. Navigate to the project directory:
   ```
   cd MyCalendarApp
   ```
3. Restore the dependencies:
   ```
   dotnet restore
   ```
4. Build the project:
   ```
   dotnet build
   ```

## Usage

To run the application, use the following command:
```
dotnet run --project src/MyCalendarApp/MyCalendarApp.csproj
```

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue for any suggestions or improvements.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.