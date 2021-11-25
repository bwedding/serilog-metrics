// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Diagnostics;
using Serilog.Events;
using Serilog;
using System.Threading.Tasks;

namespace SerilogMetrics
{
	/// <summary>
	/// Timed operation.
	/// </summary>
	public class TimedTaskOperation : IDisposable
	{
		readonly ILogger _logger;
		readonly LogEventLevel _level;
		readonly LogEventLevel _levelExceeds;
		object[] _propertyValues;

		readonly TimeSpan? _warnIfExceeds;
		/// <summary>
		/// This is public so that EndtimedOperation can access it to see which TimedOperation to dispose
		/// </summary>
		public readonly object _identifier;
		readonly string _description;
		readonly Stopwatch _sw;

		/// <summary>
		/// The beginning operation template.
		/// </summary>
		public const string BeginningOperationTemplate = "Beginning operation {TimedOperationId}: {TimedOperationDescription}";

		/// <summary>
		/// The completed operation template.
		/// </summary>
		public const string CompletedOperationTemplate = "Completed operation {TimedOperationId}: {TimedOperationDescription} in {TimedOperationElapsed} ({TimedOperationElapsedInMs} ms)";

		/// <summary>
		/// The operation exceeded template.
		/// </summary>
		public const string OperationExceededTemplate = "Operation {TimedOperationId}: {TimedOperationDescription} exceeded the limit of {WarningLimit} by completing in {TimedOperationElapsed}  ({TimedOperationElapsedInMs} ms)";

		readonly string _completedOperationMessage;
		readonly string _exceededOperationMessage;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="level"></param>
		/// <param name="warnIfExceeds"></param>
		/// <param name="task"></param>
		/// <param name="identifier"></param>
		/// <param name="description"></param>
		/// <param name="levelExceeds"></param>
		/// <param name="completedMessage"></param>
		/// <param name="exceededOperationMessage"></param>
		/// <param name="propertyValues"></param>
		public TimedTaskOperation(ILogger logger, LogEventLevel level, TimeSpan? warnIfExceeds, Task task, object identifier, string description, 
		                      LogEventLevel levelExceeds = LogEventLevel.Warning,
		                      string completedMessage = CompletedOperationTemplate, string exceededOperationMessage = OperationExceededTemplate,
		                      params object[] propertyValues)
		{
			_logger = logger;
			_level = level;
			_levelExceeds = levelExceeds;
			_warnIfExceeds = warnIfExceeds;
			_identifier = identifier;
			_description = description;
			_propertyValues = propertyValues;

			// Messages
			_completedOperationMessage = completedMessage ?? CompletedOperationTemplate;
			_exceededOperationMessage = exceededOperationMessage ?? OperationExceededTemplate;

			_sw = Stopwatch.StartNew ();
			task.GetAwaiter().OnCompleted(() =>
			{
				Dispose();
			});
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose ()
		{
			_sw.Stop ();

			if (_warnIfExceeds.HasValue && _sw.Elapsed > _warnIfExceeds.Value)
				_logger.Write (_levelExceeds, _exceededOperationMessage, GeneratePropertyBag (_identifier, _description, _warnIfExceeds.Value, _sw.Elapsed, _sw.ElapsedMilliseconds));
			else
				_logger.Write (_level, _completedOperationMessage, GeneratePropertyBag (_identifier, _description, _sw.Elapsed, _sw.ElapsedMilliseconds));
		}

		/// <summary>
		/// Generates the property bag by combining parameter values with the timed operation values.
		/// </summary>
		/// <returns>The property bag with the combined values.</returns>
		/// <param name="values">Values.</param>
		protected virtual object[] GeneratePropertyBag (params object[] values)
		{
			if (_propertyValues != null)
				return values.Concat (_propertyValues).ToArray ();
			else
				return values;
		
		}
	}
}