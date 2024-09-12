// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace CodeSmile.Components.Registry
{
	/// <summary>
	///     Keeps references to components by type to avoid making each individual type a singleton of its own.
	///     Due to this, ComponentsRegistry does not handle multiple components of the same type.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class ComponentsRegistry : MonoBehaviour
	{
		public static event Action<Type, Component> OnComponentAssigned;

		private static Dictionary<Type, Component> s_Components;
		private static Dictionary<Type, List<TaskCompletionSource<Object>>> s_AssignmentAwaitables;

		/// <summary>
		///     Get a component from the registry. Returns null if no such component is currently registered.
		/// </summary>
		/// <remarks>
		///     CAUTION: Do NOT call Get<T> from Awake to avoid execution order issues. Get in OnEnable or Start instead!
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T Get<T>() where T : Component =>
			s_Components.ContainsKey(typeof(T)) ? s_Components[typeof(T)] as T : null;

		/// <summary>
		///     Gets a component from the registry, awaitable.
		/// </summary>
		/// <remarks>
		///     Caution: If the component is never assigned the awaitables will wait forever!
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static async Task<T> GetAsync<T>() where T : Component
		{
			// if already assigned, return instantly
			var component = Get<T>();
			if (component != null)
				return component;

			// create entry for type in awaitables
			if (s_AssignmentAwaitables.ContainsKey(typeof(T)) == false)
				s_AssignmentAwaitables[typeof(T)] = new List<TaskCompletionSource<Object>>();

			// add a new completion source
			var tcs = new TaskCompletionSource<Object>();
			s_AssignmentAwaitables[typeof(T)].Add(tcs);

			// await here so the result can be cast to T
			var result = await tcs.Task.ConfigureAwait(false);
			return result as T;
		}

		/// <summary>
		///     Registers or replaces the currently registered component of the same type.
		/// </summary>
		/// <remarks>
		///     CAUTION: Register components preferably in Awake to avoid execution order issues. Get in OnEnable or Start.
		/// </remarks>
		/// <param name="component"></param>
		/// <typeparam name="T"></typeparam>
		public static void Set<T>(T component) where T : Component
		{
			s_Components[typeof(T)] = component;
			OnComponentAssigned?.Invoke(typeof(T), component);
			ProcessAssignmentAwaitables(component);
		}

		private static void ProcessAssignmentAwaitables<T>(T component) where T : Component
		{
			if (s_AssignmentAwaitables.TryGetValue(typeof(T), out var awaitables))
			{
				foreach (var completionSource in awaitables)
					completionSource.SetResult(component);

				s_AssignmentAwaitables.Remove(typeof(T));
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticFields()
		{
			s_Components = new Dictionary<Type, Component>();
			s_AssignmentAwaitables = new Dictionary<Type, List<TaskCompletionSource<Object>>>();
			OnComponentAssigned = null;
		}

#if UNITY_EDITOR
		[SerializeField] private Boolean m_ClickToLogComponents;

		private void OnValidate()
		{
			if (m_ClickToLogComponents)
			{
				m_ClickToLogComponents = false;
				LogComponents();
			}
		}

		private void LogComponents()
		{
			var sb = new StringBuilder("ComponentsRegistry contains:\n");
			if (s_Components != null)
			{
				sb.AppendLine($"{s_Components.Count} components:");
				foreach (var kvp in s_Components)
				{
					var type = kvp.Key;
					var component = kvp.Value;
					sb.AppendLine($"<b>{type.Name}</b> (GO: '{component?.name}', ID: {component?.GetInstanceID()})");
				}
			}
			else
			{
				sb.AppendLine("<no components>");
			}

			Debug.Log(sb.ToString());
		}
#endif
	}
}
