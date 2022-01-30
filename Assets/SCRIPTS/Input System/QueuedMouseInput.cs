using System.Collections.Generic;
using UnityEngine;

namespace SS.InputSystem
{
	public class QueuedMouseInput : MonoBehaviour
	{
		private Dictionary<MouseCode, InputQueue> press = new Dictionary<MouseCode, InputQueue>();
		private Dictionary<MouseCode, InputQueue> hold = new Dictionary<MouseCode, InputQueue>();
		private Dictionary<MouseCode, InputQueue> release = new Dictionary<MouseCode, InputQueue>();



		public void EnableHold( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Enable( method );
			}
		}
		public void DisableHold( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Disable( method );
			}
		}


		public void EnableRelease( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Enable( method );
			}
		}
		public void DisableRelease( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Disable( method );
			}
		}


		public void EnablePress( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Enable( method );
			}
		}
		public void DisablePress( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Disable( method );
			}
		}

		public void RegisterOnPress( MouseCode button, float priorityId, System.Action<InputQueue> method, bool isEnabled = true, bool isOneShot = false )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled, isOneShot );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled, isOneShot );
			this.press.Add( button, inputQueue );
		}

		public void RegisterOnHold( MouseCode button, float priorityId, System.Action<InputQueue> method, bool isEnabled = true, bool isOneShot = false )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled, isOneShot );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled, isOneShot );
			this.hold.Add( button, inputQueue );
		}

		public void RegisterOnRelease( MouseCode button, float priorityId, System.Action<InputQueue> method, bool isEnabled = true, bool isOneShot = false )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled, isOneShot );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled, isOneShot );
			this.release.Add( button, inputQueue );
		}

		public void ClearOnPress( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.press.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Remove( method );
			}
		}

		public void ClearOnHold( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.hold.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Remove( method );
			}
		}

		public void ClearOnRelease( MouseCode button, System.Action<InputQueue> method )
		{
			if( this.release.TryGetValue( button, out InputQueue inputQueue ) )
			{
				inputQueue.Remove( method );
			}
		}

		public void ClearInputSources()
		{
			this.press.Clear();
			this.hold.Clear();
			this.release.Clear();
		}

		const float MAX_DOUBLE_CLICK_DELAY = 0.2f;
		const int MAX_CLICK_COUNT = 2;

		void Update()
		{
			// Copy the queue so if the function running in the keypress wants to register another keypress it won't throw that the dictionary was modified.
			Dictionary<MouseCode, InputQueue> pressQueue = new Dictionary<MouseCode, InputQueue>( this.press );

			foreach( var kvp in this.press )
			{
				if( Input.GetMouseButtonDown( (int)kvp.Key ) )
				{
					// count number of presses.
					if( 
						   (Time.time <= kvp.Value.pressTimestamp + MAX_DOUBLE_CLICK_DELAY)
						&& (kvp.Value.pressCount < MAX_CLICK_COUNT)
						&& (kvp.Value.lastControllerPosition - Input.mousePosition).sqrMagnitude <= (3.0f * 3.0f)
						)
					{
						kvp.Value.pressCount++;
					}
					else
					{
						kvp.Value.pressCount = 1;
					}
					kvp.Value.Execute();
					kvp.Value.lastControllerPosition = Input.mousePosition;
				}
				if( Input.GetMouseButtonUp( (int)kvp.Key ) )
				{
					kvp.Value.pressTimestamp = Time.time;
				}
			}

			// Copy the queue so if the function running in the keypress wants to register another keypress it won't throw that the dictionary was modified.
			Dictionary<MouseCode, InputQueue> holdQueue = new Dictionary<MouseCode, InputQueue>( this.hold );

			foreach( var kvp in holdQueue )
			{
				if( Input.GetMouseButton( (int)kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}

			// Copy the queue so if the function running in the keypress wants to register another keypress it won't throw that the dictionary was modified.
			Dictionary<MouseCode, InputQueue> releaseQueue = new Dictionary<MouseCode, InputQueue>( this.release );

			foreach( var kvp in releaseQueue )
			{
				if( Input.GetMouseButtonUp( (int)kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
		}
	}
}