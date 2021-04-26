﻿using System;
using Broadcast.EventSourcing;
using Broadcast.Server;

namespace Broadcast.Processing
{
	/// <summary>
	/// BackgroundDispatcher that processes a task.
	/// If the task is a <see cref="INotification"/> and has registered handlers, the output is passed to all handlers
	/// </summary>
	public class TaskExecutionDispatcher : IBackgroundDispatcher<IProcessorContext>
	{
		private readonly ITask _task;
		private readonly ILogger _logger;

		/// <summary>
		/// Creates a new instance of a TaskExecutionDispatcher
		/// </summary>
		/// <param name="task"></param>
		public TaskExecutionDispatcher(ITask task)
		{
			_task = task ?? throw new ArgumentNullException(nameof(task));

			_logger = LoggerFactory.Create();
			_logger.Write($"Starting new TaskExecutionDispatcher for {task.Id}");
		}

		/// <summary>
		/// Execute the dispatcher and process the task
		/// </summary>
		/// <param name="context"></param>
		public void Execute(IProcessorContext context)
		{
			// Stopwatch is the only object allowed outside the try..catch to ensure no errors occure outside the executionblock
			var sw = new Stopwatch();
			sw.Start();

			try
			{
				_logger.Write($"Start processing task {_task.Id}");
				_task.SetInprocess();

				//TODO: INotification is bad design. any object should be useable
				var invocation = new TaskInvocation();
				var output = _task.Invoke(invocation) as INotification;

				// try to find the handlers
				if (output != null && context.NotificationHandlers.TryGetHandlers(output.GetType(), out var handlers))
				{
					//// it could be that T is of a base/inherited type but the handler is of a object type
					//if (!Handlers.Handlers.TryGetValue(output.GetType(), out handlers))
					//{
					//	return;
					//}

					// run all handlers with the value
					foreach (var handler in handlers)
					{
						handler(output);
					}
				}
			}
			catch (Exception ex)
			{
				//TODO: set taskt to faulted
				//TODO: log exception
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);
			}

			_task.SetProcessed();
		}
	}
}
