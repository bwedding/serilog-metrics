
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace SerilogMetrics.Samples.Console
{
    using System;

    class Program
	{
        static Task ExampleMethodAsync()
        {
            Task t1 = Task.Run(() => LongRunningTask("Bruce"));
            return t1;
        }

        static void LongRunningTask(String s)
        {
            Thread.Sleep(1500);
            Console.WriteLine("{0} thread ID: {1}", s, Thread.CurrentThread.ManagedThreadId);
        }

        static void Main()
		{
			var logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
                .WriteTo.Async(c => c.File("c:/temp/async.log"))
                .WriteTo.ColoredConsole(
					outputTemplate: "{Timestamp:HH:mm:ss} ({ThreadId}) [{Level}] {Message}{NewLine}{Exception}")
				.CreateLogger();

            logger.BeginUnscopedTimedOperation("No scope test", "no-scope");  // Time entire operation

            using (logger.BeginTimedOperation("Time a thread sleep for 2 seconds."))
            {
                Thread.Sleep(1000);
                using (logger.BeginTimedOperation("And inside we try a Task.Delay for 2 seconds."))
                {
                    Task.Delay(2000).Wait();
                }
                Thread.Sleep(1000);
            }

            using (logger.BeginTimedOperation("Using a passed in identifier (b)", "test-loop"))
            {
                // ReSharper disable once NotAccessedVariable
                var b = "";
                for (var i = 0; i < 1000; i++)
                {
                    b += "b";
                }
            }

            logger.BeginUnscopedTimedOperation("No scope test", "no-scope2");  // time a short section of code without scoped braces
            // ReSharper disable once NotAccessedVariable
            var a = "";
			for (var i = 0; i < 1000; i++)
			{
				a += "b";
			}
            Thread.Sleep(1000);
            logger.EndTimedOperation("No scope test", "no-scope2");

            // Exceed a limit
            using (logger.BeginTimedOperation("This should execute within 1 second.", null, LogEventLevel.Debug, TimeSpan.FromSeconds(1)))
            {
                Thread.Sleep(1100);
            }

            //Gauge
            var queue = new Queue<int>();
            var gauge = logger.GaugeOperation("queue", "item(s)", () => queue.Count());
            gauge.Write();
            queue.Enqueue(20);
            gauge.Write();
            queue.Dequeue();
            gauge.Write();

            // Counter
            var counter = logger.CountOperation("counter", "operation(s)", true, LogEventLevel.Debug, resolution: 2);
            counter.Increment();
            counter.Increment();
            counter.Increment();
            counter.Decrement();
            counter.Add(10);
            counter.Add(-5);

            Task t = Program.ExampleMethodAsync();
            logger.LogTaskExecutionTime(t, "ExampleMethodAsync", "myid1");  // will log when above task completes

            logger.EndTimedOperation("No scope test", "no-scope");
            Log.CloseAndFlush();

            // Wait for the task to complete by just waiting for an enter key
            System.Console.WriteLine("Press a key to exit.");
			System.Console.ReadKey(true);
		}
	}
}