using System.Collections.Generic;
using UnityEngine;

namespace SS.InputSystem
{
	public class QueuedMouseInput : MonoBehaviour
	{
		private Dictionary<int, InputQueue> press = new Dictionary<int, InputQueue>();
		private Dictionary<int, InputQueue> hold = new Dictionary<int, InputQueue>();
		private Dictionary<int, InputQueue> release = new Dictionary<int, InputQueue>();

		public void ClearOnPress( int button )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( button, out inputQueue ) )
			{
				inputQueue = new InputQueue();
				return;
			}
		}

		public void ClearOnHold( int button )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( button, out inputQueue ) )
			{
				inputQueue = new InputQueue();
				return;
			}
		}

		public void ClearOnRelease( int button )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( button, out inputQueue ) )
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

		public void RegisterOnPress( int button, System.Action<InputQueue> method, bool enabled )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
			this.press.Add( button, inputQueue );
		}

		public void RegisterOnHold( int button, System.Action<InputQueue> method, bool enabled )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
			this.hold.Add( button, inputQueue );
		}

		public void RegisterOnRelease( int button, System.Action<InputQueue> method, bool enabled )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.methods.Add( new InputQueue.InputMethod() { isEnabled = enabled, method = method } );
			this.release.Add( button, inputQueue );
		}

		void Start()
		{

		}

		void Update()
		{
			foreach( var kvp in this.press )
			{
				if( Input.GetMouseButtonDown( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
			foreach( var kvp in this.hold )
			{
				if( Input.GetMouseButton( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
			foreach( var kvp in this.release )
			{
				if( Input.GetMouseButtonUp( kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
		}
	}
}