﻿using Broadcast.Composition;
using System;
using System.Linq.Expressions;
using Broadcast.Configuration;

namespace Broadcast
{
	/// <summary>
	/// Executes Tasks on a TaskServer or cluster
	/// </summary>
	public class TaskServerClient
	{
		private static readonly ItemFactory<IBroadcastingClient> ItemFactory = new ItemFactory<IBroadcastingClient>(() => new BroadcastingClient());

		/// <summary>
		/// Gets the default instance of the <see cref="IBroadcastingClient"/>
		/// </summary>
		public static IBroadcastingClient Client => ItemFactory.Factory();

		/// <summary>
		/// Setup a new instance for the default <see cref="IBroadcastingClient"/>.
		/// Setup with null to reset to the default
		/// </summary>
		/// <param name="setup"></param>
		public static void Setup(Func<IBroadcastingClient> setup)
		{
			ItemFactory.Factory = setup;
		}

		/// <summary>
		/// Adds a recurring task
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="time"></param>
		/// <returns>The Id of the task</returns>
		public static string Recurring(Expression<Action> expression, TimeSpan time) 
			=> Recurring(null, expression, time);

		/// <summary>
		/// Adds a recurring task
		/// </summary>
		/// <param name="name"></param>
		/// <param name="expression"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static string Recurring(string name, Expression<Action> expression, TimeSpan time)
		{
			var task = TaskFactory.CreateTask(expression);
			task.Time = time;
			task.IsRecurring = true;

			if (!string.IsNullOrEmpty(name))
			{
				task.Name = name;
			}

			Client.Enqueue(task);

			return task.Id;
		}

		/// <summary>
		/// Adds a scheduled task
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="time"></param>
		/// <returns>The Id of the task</returns>
		public static string Schedule(Expression<Action> expression, TimeSpan time)
		{
			var task = TaskFactory.CreateTask(expression);
			task.Time = time;

			Client.Enqueue(task);

			return task.Id;
		}

		/// <summary>
		/// Process a task
		/// </summary>
		/// <param name="expression"></param>
		/// <returns>The Id of the task</returns>
		public static string Send(Expression<Action> expression)
		{
			var task = TaskFactory.CreateTask(expression);

			Client.Enqueue(task);

			return task.Id;
		}
	}
}
