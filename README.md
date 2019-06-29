# EFCoreChangeReport ![Nuget](https://img.shields.io/nuget/v/EFCoreChangeReport.svg?style=flat) ![GitHub last commit](https://img.shields.io/github/last-commit/Sulexa/EFCoreChangeReport.svg)

<!-- ![Nuget](https://img.shields.io/nuget/dt/EFCoreChangeReport.svg) -->


## Description

EFCoreChangeReport is a plugin for Microsoft.EntityFrameworkCore.
It creates a reporting system of all changes made on your base with EF core and with the customizable sink system, you can store these data anywhere you want.

With updates, the plugin will offer others possibilities of customization, see the [Roadmap/Patch notes](##Roadmap/Patch-notes) part if you want to know more.

## Installations

Install the nuget package :

```
Install-Package EFCoreChangeReport
```

And at least one sink :

#### EFCoreChangeReport Sinks made by me :

* ```Install-Package EFCoreChangeReport.Sink.ToConsole```


## Usage

First add EFCoreChangeReport in your startup services (You need a sink, here i use the ToConsole from [EFCoreChangeReport.Sink.ToConsole](https://github.com/Sulexa/EFCoreChangeReport.Sink.ToConsole/))
```csharp 
services.AddEFCoreChangeReport(efCoreChangeReportConfiguration => efCoreChangeReportConfiguration.ToConsole());
```
> **If you don't have any sink the library will throw an error**

EFCoreChangeReportService is added by dependency injection :
```csharp
public class Example
{
    private readonly IEFCoreChangeReportService _eFCoreChangeReportService;

    public Example(IEFCoreChangeReportService _eFCoreChangeReportService)
    {
        _eFCoreChangeReportService = _eFCoreChangeReportService;
    }
}
```

With any SaveChanges and SaveChangesAsync of your db context you need to do :
```csharp
_eFCoreChangeReportService.PrepareChangeReports(dbContext);
await dbContext.SaveChangesAsync();
_eFCoreChangeReportService.SaveChangeReports(dbContext);
```
The PrepareChangeReports prepare the reports for updated and deleted elements, and keep track of all the added elements (at this point the sinks are still not used).
The SaveChangeReports prepare the reports for created elements and use the sinks.

A performance test project is also available [here](https://github.com/Sulexa/EFCoreChangeReport.PerformanceTest/) as exemple.

## Roadmap/Patch notes

### 1.0 (Released)
* Initial library working
* Implementing way to send all the databases saved changes to your sinks
* Sinks system for customisation

### 1.0-.NETCore3.0 (Coming Soon)
* Verified compatibility with .NET Core 3.0
* Use of the newly integrated JSON writer instead of newtonsoft

### 1.1 Configuration update

* Add configuration to disable Add, Update and Delete changes
* Support for custom process to Add, Update and Delete

### 1.2 Inclusion/Exclusion update

* Add configuration to include all changes or exclude all changes (Default : include all changes)
* Add attribute to include or to exclude class and properties from process
* Add another way to include or exclude class or properties from process (for user who don't like using attribute in database model class)

## Authors and acknowledgment

Made by Sulexa

Special thanks to the packages which inspired EFCoreChangeReport :

* [AutoHistory](https://github.com/Arch/AutoHistory/) which inspired me for the concept of auto-saving change on the entity system
* [Serilog](https://github.com/serilog/serilog) which inspired me with the modularity of sink system

## License

EFCoreChangeReport is licensed under the [MIT license](LICENSE.TXT).
<!-- ![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg) -->