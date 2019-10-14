﻿using System.Collections.Generic;
using UnityEngine;

namespace SS.InputSystem
{
	public class QueuedMouseInput : MonoBehaviour
	{
		private Dictionary<MouseCode, InputQueue> press = new Dictionary<MouseCode, InputQueue>();
		private Dictionary<MouseCode, InputQueue> hold = new Dictionary<MouseCode, InputQueue>();
		private Dictionary<MouseCode, InputQueue> release = new Dictionary<MouseCode, InputQueue>();

		public void ClearOnPress( MouseCode button, System.Action<InputQueue> method )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Remove( method );
				return;
			}
		}

		public void ClearOnHold( MouseCode button, System.Action<InputQueue> method )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Remove( method );
				return;
			}
		}

		public void ClearOnRelease( MouseCode button, System.Action<InputQueue> method )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Remove( method );
				return;
			}
		}

		public void ClearInputSources()
		{
			this.press.Clear();
			this.hold.Clear();
			this.release.Clear();
		}

		public void RegisterOnPress( MouseCode button, float priorityId, System.Action<InputQueue> method, bool isEnabled )
		{
			InputQueue inputQueue;
			if( this.press.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled );
			this.press.Add( button, inputQueue );
		}

		public void RegisterOnHold( MouseCode button, float priorityId, System.Action<InputQueue> method, bool isEnabled )
		{
			InputQueue inputQueue;
			if( this.hold.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled );
			this.hold.Add( button, inputQueue );
		}

		public void RegisterOnRelease( MouseCode button, float priorityId, System.Action<InputQueue> method, bool isEnabled )
		{
			InputQueue inputQueue;
			if( this.release.TryGetValue( button, out inputQueue ) )
			{
				inputQueue.Add( method, priorityId, isEnabled );
				return;
			}
			inputQueue = new InputQueue();
			inputQueue.Add( method, priorityId, isEnabled );
			this.release.Add( button, inputQueue );
		}

		void Start()
		{

		}

		void Update()
		{
			foreach( var kvp in this.press )
			{
				if( Input.GetMouseButtonDown( (int)kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
			foreach( var kvp in this.hold )
			{
				if( Input.GetMouseButton( (int)kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
			foreach( var kvp in this.release )
			{
				if( Input.GetMouseButtonUp( (int)kvp.Key ) )
				{
					kvp.Value.Execute();
				}
			}
		}
	}
}