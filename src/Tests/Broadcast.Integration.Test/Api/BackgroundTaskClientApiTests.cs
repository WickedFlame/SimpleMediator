﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Broadcast.EventSourcing;
using Broadcast.Storage;
using NUnit.Framework;

namespace Broadcast.Integration.Test.Api
{
	[SingleThreaded]
	[Explicit]
	[Category("Integration")]
	public class BackgroundTaskClientApiTests
	{
		[SetUp]
		public void Setup()
		{
			TaskStore.Default.Clear();
			BroadcastServer.Setup(s => { });
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			TaskStore.Default.Clear();
			BroadcastServer.Setup(s => { });
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_StaticTrace()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Send(() => Trace.WriteLine("test"));

			BroadcastServer.Server.WaitAll();
			Assert.AreEqual(1, BroadcastServer.Server.GetProcessedTasks().Count());
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_Method()
		{
			// execute a local method
			// serializeable
			BackgroundTaskClient.Send(() => TestMethod(1));

			BroadcastServer.Server.WaitAll();
			Assert.AreEqual(1, BroadcastServer.Server.GetProcessedTasks().Count());
		}

		[Test]
		public void BackgroundTaskClient_Api_Send_GenericMethod()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Send(() => GenericMethod(1));

			BroadcastServer.Server.WaitAll();
			Assert.AreEqual(1, BroadcastServer.Server.GetProcessedTasks().Count());
		}




		[Test]
		public void BackgroundTaskClient_Api_Schedule_StaticTrace()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Schedule(() => Trace.WriteLine("test"), TimeSpan.FromSeconds(1));

			Task.Delay(1500).Wait();

			Assert.GreaterOrEqual(BroadcastServer.Server.GetProcessedTasks().Count(), 1);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_Method()
		{
			// execute a local method
			// serializeable
			BackgroundTaskClient.Schedule(() => TestMethod(1), TimeSpan.FromSeconds(1));

			Task.Delay(1500).Wait();

			Assert.GreaterOrEqual(BroadcastServer.Server.GetProcessedTasks().Count(), 1);
		}

		[Test]
		public void BackgroundTaskClient_Api_Schedule_GenericMethod()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Schedule(() => GenericMethod(1), TimeSpan.FromSeconds(1));

			Task.Delay(1500).Wait();

			Assert.GreaterOrEqual(BroadcastServer.Server.GetProcessedTasks().Count(), 1);
		}



		[Test]
		public void BackgroundTaskClient_Api_Recurring_StaticTrace()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring(() => Trace.WriteLine("test"), TimeSpan.FromSeconds(0.5));

			Task.Delay(2000).Wait();

			Assert.GreaterOrEqual(BroadcastServer.Server.GetProcessedTasks().Count(), 2);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Method()
		{
			// execute a local method
			// serializeable
			BackgroundTaskClient.Recurring(() => TestMethod(1), TimeSpan.FromSeconds(0.5));

			Task.Delay(2000).Wait();

			Assert.GreaterOrEqual(BroadcastServer.Server.GetProcessedTasks().Count(), 2);
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_GenericMethod()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Recurring(() => GenericMethod(1), TimeSpan.FromSeconds(0.5));

			Task.Delay(2000).Wait();

			Assert.GreaterOrEqual(BroadcastServer.Server.GetProcessedTasks().Count(), 2);
		}
		
		[Test]
		public void BackgroundTaskClient_Api_Recurring_Name()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring("BackgroundTaskClient_Api_Recurring", () => Trace.WriteLine("test"), TimeSpan.FromSeconds(0.5));

			Task.Delay(2000).Wait();

			Assert.IsTrue(BroadcastServer.Server.GetProcessedTasks().All(t => t.Name == "BackgroundTaskClient_Api_Recurring"));
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Update()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("test"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var originalRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			var originalTask = BroadcastServer.Server.Store.FirstOrDefault(t => t.Id == originalRecurring["ReferenceId"].ToString());

			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("succeeded"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var updatedRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			var updatedTask = BroadcastServer.Server.Store.FirstOrDefault(t => t.Id == updatedRecurring["ReferenceId"].ToString()) as BroadcastTask;

			Assert.That(updatedTask.Args.Single().ToString(), Is.EqualTo("succeeded"));
		}


		[Test]
		public void BackgroundTaskClient_Api_Recurring_Update_MethodChanged()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("test"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var originalRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			var originalTask = BroadcastServer.Server.Store.FirstOrDefault(t => t.Id == originalRecurring["ReferenceId"].ToString()) as BroadcastTask;

			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("succeeded"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var updatedRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			var updatedTask = BroadcastServer.Server.Store.FirstOrDefault(t => t.Id == updatedRecurring["ReferenceId"].ToString()) as BroadcastTask;

			Assert.That(originalTask.Args.Single(), Is.Not.EqualTo(updatedTask.Args.Single()));
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Update_SameId_Task()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("test"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var originalRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			var originalTask = BroadcastServer.Server.Store.FirstOrDefault(t => t.Id == originalRecurring["ReferenceId"].ToString());

			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("succeeded"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var updatedRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			var updatedTask = BroadcastServer.Server.Store.FirstOrDefault(t => t.Id == updatedRecurring["ReferenceId"].ToString()) as BroadcastTask;

			Assert.That(originalTask.Id, Is.EqualTo(updatedTask.Id));
		}

		[Test]
		public void BackgroundTaskClient_Api_Recurring_Update_SameId_Reference()
		{
			// execute a static method
			// serializeable
			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("test"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var originalRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			
			BackgroundTaskClient.Recurring("Updateable", () => Trace.WriteLine("succeeded"), TimeSpan.FromSeconds(30));
			Task.Delay(1000).Wait();

			var updatedRecurring = BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:Updateable")));
			
			Assert.That(originalRecurring["ReferenceId"].ToString(), Is.EqualTo(updatedRecurring["ReferenceId"].ToString()));
		}


		[Test]
		public void BackgroundTaskClient_Api_Delete()
		{
			// execute a generic method
			// serializeable
			var id = BackgroundTaskClient.Schedule(() => GenericMethod(1), TimeSpan.FromSeconds(15));
			BackgroundTaskClient.DeleteTask(id);

			Assert.That(BroadcastServer.Server.Store.Count(t => t.State == TaskState.Deleted), Is.GreaterThan(0));
			Assert.That(BroadcastServer.Server.Store.All(t => t.State == TaskState.Deleted));
		}

		[Test]
		public void BackgroundTaskClient_Api_DeleteRecurring()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Recurring("recurring_delete", () => GenericMethod(1), TimeSpan.FromSeconds(15));
			BackgroundTaskClient.DeleteRecurringTask("recurring_delete");

			Assert.IsNull(BroadcastServer.Server.Store.Storage(s => s.Get<DataObject>(new StorageKey($"tasks:recurring:recurring_delete"))));
		}

		[Test]
		public void BackgroundTaskClient_Api_DeleteRecurring_ReferencedTask_Deleted()
		{
			// execute a generic method
			// serializeable
			BackgroundTaskClient.Recurring("recurring_delete", () => GenericMethod(1), TimeSpan.FromSeconds(0.5));
			Task.Delay(2000).Wait();

			BackgroundTaskClient.DeleteRecurringTask("recurring_delete");

			Assert.That(BroadcastServer.Server.Store.Count(t => t.State == TaskState.Deleted), Is.GreaterThanOrEqualTo(1));
		}


		public void TestMethod(int i) { }

		public void GenericMethod<T>(T value) { }
	}
}
