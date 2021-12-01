SerilogMetrics2 [![Build status](https://ci.appveyor.com/api/projects/status/ou1ofq2vvc0gd0jo/branch/master?svg=true)](https://ci.appveyor.com/project/mivano/serilog-metrics/branch/master) [![NuGet](https://img.shields.io/nuget/v/SerilogMetrics.svg)](https://www.nuget.org/packages/SerilogMetrics/)
=================================================================================================================================================

Serilog combines the best features of traditional and structured diagnostic logging in an easy-to-use package and Serilog.Metrics extends this logging framework with measure capabilities like counters, timers, meters and gauges.

* [Serilog Homepage](http://serilog.net)
* [Serilog Documentation](https://github.com/serilog/serilog/wiki)
* [Serilog Metrics Documentation](https://github.com/serilog-metrics/serilog-metrics/wiki)

## Get started
To quickly get started, add the SerilogMetrics2 package to your solution using the NuGet Package manager or run the following command in the Package Console Window:

```powershell
Install-Package SerilogMetrics2
```

The metrics method extensions are extending the ILogger interface of Serilog. So just reference the Serilog namespace and you can invoke the functionality from the logger.
SerilogMetrics2 extends functionality of SerilogMetrics which doesn't seem to be supported any longer.
The following extension methods are added:

public static IDisposable BeginUnscopedTimedOperation(string description, string id)
This method is exactly like BeginTimedOperation except it does not require that the scope of the timing be marked with braces.
The EndTimedOperation() method may be called from anywhere in the code as long as the logger is in scope. 

This method takes the same arguments as BeginTimedOperation but it requires a unique ID as a string,
which will be used when EndTimedOperation is called. See example below.

e.g.
```csharp
logger.BeginUnscopedTimedOperation("Time a thread sleep for 2 seconds.", "myID");
var t = Task.Run(() => MyLongRunningTask("Task") );
// Lots of other code executing while task is running
// ...
// ...
t.Wait();
// EndTimedOperation must be called with the same string ID used above.
Logger.EndTimedOperation("any message", "myID"); // Note: SAME ID as above

public static EndTimedOperation(string description, string id); // Note: ID must match the call to BeginUnscopedTimedOperation

public static IDisposable LogTaskExecutionTime(Task t, string description, string uniqueID)
```
This method takes the same arguments as BeginTimedOperation but also requires a Task task and string ID which is unique to this call.
This is a "set and forget" method which creates an awaiter for the task and when the task finishes, it will log the time spent performing the task.

e.g.
```csharp
static void LongRunningTask(String s)
{
    Thread.Sleep(2500);
}

Task t = Task.Run(() => LongRunningTask("SerilogMetrics2"));    // no await
logger.LogTaskExecutionTime(t, "ExampleMethodAsync", "myid1");  // will log when above task completes
```

For example;
```csharp
var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();

using (logger.BeginTimedOperation("Time a thread sleep for 2 seconds."))
{
     Thread.Sleep(2000);
}
```

See the [documentation](https://github.com/serilog-metrics/serilog-metrics/wiki) for more details.

Copyright &copy; 2016 Serilog Metrics Contributors - Provided under the [Apache License, Version 2.0](http://apache.org/licenses/LICENSE-2.0.html).
