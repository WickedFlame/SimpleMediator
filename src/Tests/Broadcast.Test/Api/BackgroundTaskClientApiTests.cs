﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Broadcast.EventSourcing;
using Broadcast.Processing;
using Moq;
using NUnit.Framework;

namespace Broadcast.Test.Api
{
	[SingleThreaded]
	public class BackgroundTaskClientApiTests
	{
		private Mock<ITaskProcessor> _processor;
		private Mock<IScheduler> _scheduler;
		private Mock<ITaskStore> _store;

		[SetUp]
		public void Setup()
		{
			_processor = new Mock<ITaskProcessor>();
			_scheduler = new Mock<IScheduler>();
			_store = new Mock<ITaskStore>();
			var ctx = new Mock<IProcessorContext>();
			ctx.Setup(exp => exp.Open()).Returns(_processor.Object);
			ctx.Setup(exp => exp.Store).Returns(_store.Object);

			Broadcaster.Setup(s =>
			{
				s.Context = ctx.Object;
				s.Scheduler = _scheduler.Object;
			});
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_StaticTrace()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Send(() => Trace.WriteLine("test"));

			_processor.Verify(exp => exp.Process(It.IsAny<ITask>()), Times.Once);
			_store.Verify(exp => exp.Add(It.IsAny<ITask>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_Method()
		{
			// execute a local method
			// serializeable
			BackgroundTaskClient.Send(() => TestMethod(1));

			_processor.Verify(exp => exp.Process(It.IsAny<ITask>()), Times.Once);
			_store.Verify(exp => exp.Add(It.IsAny<ITask>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_GenericMethod()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Send(() => GenericMethod(1));

			_processor.Verify(exp => exp.Process(It.IsAny<ITask>()), Times.Once);
			_store.Verify(exp => exp.Add(It.IsAny<ITask>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_Notification_Class()
		{
			// send a event to a handler
			// serializeable Func<TestClass>
			BackgroundTaskClient.Send<TestClass>(() => new TestClass(1));

			_processor.Verify(exp => exp.Process(It.IsAny<ITask>()), Times.Once);
			_store.Verify(exp => exp.Add(It.IsAny<ITask>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_Notification_Method()
		{
			// send a event to a handler
			// serializeable Func<TestClass>
			BackgroundTaskClient.Send<TestClass>(() => Returnable(1));

			_processor.Verify(exp => exp.Process(It.IsAny<ITask>()), Times.Once);
			_store.Verify(exp => exp.Add(It.IsAny<ITask>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_Notification_Local()
		{
			// send a local action
			// Nonserializeable
			BackgroundTaskClient.Send(() =>
			{
				Trace.WriteLine("test");
			});

			_processor.Verify(exp => exp.Process(It.IsAny<ITask>()), Times.Once);
			_store.Verify(exp => exp.Add(It.IsAny<ITask>()), Times.Once);
		}




		[Test]
		public void BackgroundTaskClient_Api_Schedule_StaticTrace()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Schedule(() => Trace.WriteLine("test"), TimeSpan.FromSeconds(1));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_Method()
		{
			// execute a local method
			// serializeable
			BackgroundTaskClient.Schedule(() => TestMethod(1), TimeSpan.FromSeconds(1));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_GenericMethod()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Schedule(() => GenericMethod(1), TimeSpan.FromSeconds(1));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_Notification_Class()
		{
			// send a event to a handler
			// Nonserializeable Func<TestClass>
			BackgroundTaskClient.Schedule<TestClass>(() => new TestClass(1), TimeSpan.FromSeconds(1));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_Notification_Method()
		{
			// send a event to a handler
			// Nonserializeable Func<TestClass>
			BackgroundTaskClient.Schedule<TestClass>(() => Returnable(1), TimeSpan.FromSeconds(1));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_Notification_Lopcal()
		{
			// send a local action
			// Nonserializeable
			BackgroundTaskClient.Schedule(() =>
			{
				Trace.WriteLine("test");
			}, TimeSpan.FromSeconds(1));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}




		[Test]
		public void BackgroundTaskClient_Api_Recurring_StaticTrace()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring(() => Trace.WriteLine("test"), TimeSpan.FromSeconds(0.5));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Method()
		{
			// execute a local method
			// serializeable
			BackgroundTaskClient.Recurring(() => TestMethod(1), TimeSpan.FromSeconds(0.5));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_GenericMethod()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Recurring(() => GenericMethod(1), TimeSpan.FromSeconds(0.5));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Notification_Class()
		{
			// send a event to a handler
			// Nonserializeable Func<TestClass>
			BackgroundTaskClient.Recurring<TestClass>(() => new TestClass(1), TimeSpan.FromSeconds(0.5));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Notification_Method()
		{
			// send a event to a handler
			// Nonserializeable Func<TestClass>
			BackgroundTaskClient.Recurring<TestClass>(() => Returnable(1), TimeSpan.FromSeconds(0.5));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Notification_Lopcal()
		{
			// send a local action
			// Nonserializeable
			BackgroundTaskClient.Recurring(() =>
			{
				Trace.WriteLine("test");
			}, TimeSpan.FromSeconds(0.5));

			_scheduler.Verify(exp => exp.Enqueue(It.IsAny<Action>(), It.IsAny<TimeSpan>()), Times.Once);
		}





		public void TestMethod(int i) { }

		public void GenericMethod<T>(T value) { }

		public TestClass Returnable(int i) => new TestClass(i);

		public class TestClass : INotification
		{
			public TestClass(int i) { }
		}
	}
}