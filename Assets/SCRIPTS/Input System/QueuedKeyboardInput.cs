using System.Collections.Generic;
using UnityEngine;

namespace SS.InputSystem
{
	public class QueuedKeyboardInput : MonoBehaviour
	{
		private Dictionary<KeyCode, InputQueue> press = new Dictionary<KeyCode, InputQueue>();
		private Dictionary<KeyCode, InputQueue> hold = new Dictionary<KeyCode, InputQueue>();
		private Dictionary<KeyCode, InputQueue> release = new Dictionary<KeyCode, InputQueue>();

		/// <summary>
		/// Registers a function to run when a key is pressed.
		/// </summary>
		public void RegisterOnPress( KeyCode key, float priorityId, System.Action<InputQueue> method, bool isEnabled = true, bool isOneShot = false )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled, isOneShot );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled, isOneShot );
			this.press.Add( key, inputQueue );
		}

		/// <summary>
		/// Registers a function to run when a key is held.
		/// </summary>
		public void RegisterOnHold( KeyCode key, float priorityId, System.Action<InputQueue> method, bool isEnabled = true, bool isOneShot = false )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled, isOneShot );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled, isOneShot );
			this.hold.Add( key, inputQueue );
		}

		/// <summary>
		/// Registers a function to run when a key is released.
		/// </summary>
		public void RegisterOnRelease( KeyCode key, float priorityId, System.Action<InputQueue> method, bool isEnabled = true, bool isOneShot = false )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled, isOneShot );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled, isOneShot );
			this.release.Add( key, inputQueue );
		}



		/// <summary>
		/// Unregisters a function from running.
		/// </summary>
		public void ClearOnPress( KeyCode key, System.Action<InputQueue> method )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.Remove( method );
				return;
			}
		}

		/// <summary>
		/// Unregisters a function from running.
		/// </summary>
		public void ClearOnHold( KeyCode key, System.Action<InputQueue> method )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.Remove( method );
				return;
			}
		}

		/// <summary>
		/// Unregisters a function from running.
		/// </summary>
		public void ClearOnRelease( KeyCode key, System.Action<InputQueue> method )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.Remove( method );
				return;
			}
		}

		/// <summary>
		/// Unregisters every input.
		/// </summary>
		public void ClearRegistries()
		{
			this.press.Clear();
			this.hold.Clear();
			this.release.Clear();
		}


		void Update()
		{
			// Copy the queue so if the function running in the keypress wants to register another keypress it won't throw that the dictionary was modified.
			Dictionary<KeyCode, InputQueue> pressQueue = new Dictionary<KeyCode, InputQueue>( this.press );

			foreach( var kvp in pressQueue )
			{
				if( Input.GetKeyDown( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}

			// Copy the queue so if the function running in the keypress wants to register another keypress it won't throw that the dictionary was modified.
			Dictionary<KeyCode, InputQueue> holdQueue = new Dictionary<KeyCode, InputQueue>( this.hold );

			foreach( var kvp in holdQueue )
			{
				if( Input.GetKey( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}

			// Copy the queue so if the function running in the keypress wants to register another keypress it won't throw that the dictionary was modified.
			Dictionary<KeyCode, InputQueue> releaseQueue = new Dictionary<KeyCode, InputQueue>( this.release );

			foreach( var kvp in releaseQueue )
			{
				if( Input.GetKeyUp( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
		}
	}
}