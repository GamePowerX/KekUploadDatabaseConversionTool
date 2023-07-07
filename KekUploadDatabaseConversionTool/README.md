```
██╗  ██╗███████╗██╗  ██╗██╗   ██╗██████╗ ██╗      ██████╗  █████╗ ██████╗ 
██║ ██╔╝██╔════╝██║ ██╔╝██║   ██║██╔══██╗██║     ██╔═══██╗██╔══██╗██╔══██╗
█████╔╝ █████╗  █████╔╝ ██║   ██║██████╔╝██║     ██║   ██║███████║██║  ██║
██╔═██╗ ██╔══╝  ██╔═██╗ ██║   ██║██╔═══╝ ██║     ██║   ██║██╔══██║██║  ██║
██║  ██╗███████╗██║  ██╗╚██████╔╝██║     ███████╗╚██████╔╝██║  ██║██████╔╝
╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚══════╝ ╚═════╝ ╚═╝  ╚═╝╚═════╝ 
```
# KekUploadDatabaseConversionTool
A tool to convert the database format of the old KekUploadServer to format of the new KekUploadServer using EntityFrameworkCore.

## Getting Started
### Building
To build the project you need to have the .NET Core SDK installed. You can download it [here](https://dotnet.microsoft.com/download).

To build the project run the following command in the root directory of the project:
```sh
dotnet build
```

### Downloading
You can download the latest release [here](https://github.com/GamePowerX/KekUploadDatabaseConversionTool/releases/latest).

### Running
When you have built the project change to the directory of the built project and run the following command:
```sh
cd KekUploadDatabaseConversionTool/bin/Debug/net7.0/
dotnet KekUploadDatabaseConversionTool.dll
```
If you have downloaded the release version change to the directory of the downloaded release and run the following command:
```sh
./KekUploadDatabaseConversionTool
```

### Usage
To use the tool you need to pass the connection string to the old database as the first argument. You can also use the program to generate a connection string for you.
<br>
To generate a connection string run the following command:
```sh
./KekUploadDatabaseConversionTool ConnectionStringBuilder -h localhost -d test -W "" -U postgres -t false
```
This will generate a connection string for a local database named "test" with the user "postgres" and no password. The connection string will be printed to the console.

To migrate the database to the new format run the following command:
```sh
./KekUploadDatabaseConversionTool MigrateToEntityFramework -c "Host=localhost;Port=5432;Database=uploads;Username=postgres;Password=password;Trust Server Certificate=True"
```
If it was successful the tool will print "The Database was successfully migrated to Entity Framework!" to the console.

## Contributing
If you want to contribute to the project you can fork the project and create a pull request.
If you have any questions you can join us on [Discord](https://discord.com/invite/czeW6HnRvz).

## License
This project is licensed under the GPL 3.0 License - see the [LICENSE](LICENSE) file for details