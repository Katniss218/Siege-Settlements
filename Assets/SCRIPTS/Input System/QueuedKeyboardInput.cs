using System.Collections.Generic;
using UnityEngine;

namespace SS.InputSystem
{
	public class QueuedKeyboardInput : MonoBehaviour
	{
		private Dictionary<KeyCode, InputQueue> press = new Dictionary<KeyCode, InputQueue>();
		private Dictionary<KeyCode, InputQueue> hold = new Dictionary<KeyCode, InputQueue>();
		private Dictionary<KeyCode, InputQueue> release = new Dictionary<KeyCode, InputQueue>();
		
		public void ClearOnPress( KeyCode key )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( key, out inputQueue ) )
			{
				inputQueue = new InputQueue();
				return;
			}
		}
		
		public void ClearOnHold( KeyCode key )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( key, out inputQueue ) )
			{
				inputQueue = new InputQueue();
				return;
			}
		}
		
		public void ClearOnRelease( KeyCode key )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( key, out inputQueue ) )
			{
				inputQueue = new InputQueue();
				return;
			}
		}

		public void ClearInputSources()
		{
			this.press.Clear();
			this.hold.Clear();
			this.release.Clear();
		}

		public void RegisterOnPress( KeyCode key, System.Action<InputQueue> method, bool enabled )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
			this.press.Add( key, inputQueue );
		}

		public void RegisterOnHold( KeyCode key, System.Action<InputQueue> method, bool enabled )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
			this.hold.Add( key, inputQueue );
		}

		public void RegisterOnRelease( KeyCode key, System.Action<InputQueue> method, bool enabled )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( key, out inputQueue ) )
			{
				inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
			this.release.Add( key, inputQueue );
		}
		
		void Start()
		{

		}
		
		void Update()
		{
			foreach( var kvp in this.press )
			{
				if( Input.GetKeyDown( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
			foreach( var kvp in this.hold )
			{
				if( Input.GetKey( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
			foreach( var kvp in this.release )
			{
				if( Input.GetKeyUp( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
		}
	}
}