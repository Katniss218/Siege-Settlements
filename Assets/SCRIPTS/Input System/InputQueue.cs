using System.Collections.Generic;
using UnityEngine;

namespace SS.InputSystem
{
	public class InputQueue
	{
		public class InputMethod
		{
			public System.Action<InputQueue> method { get; set; }
			public bool isEnabled { get; set; }
			public bool isOneShot { get; set; }
			/// <summary>
			/// Lower priority runs first.
			/// </summary>
			public float priority { get; set; }
		}
		
		private List<InputMethod> methods;

		public Vector3 lastControllerPosition { get; set; }
		public int pressCount { get; set; }
		public float pressTimestamp { get; set; } = float.MinValue;
		public bool isStopped { get; private set; }

		public InputQueue()
		{
			this.methods = new List<InputMethod>();
		}

		public void Add( System.Action<InputQueue> method, float priority, bool isEnabled, bool isOneShot )
		{
			int lastBelowIndex = -1;
			for( int i = 0; i < methods.Count; i++ )
			{
				if( this.methods[i].priority <= priority )
				{
					lastBelowIndex = i;
				}
			}
			if( this.methods.Count == 0 )
			{
				this.methods.Add( new InputMethod() { method = method, priority = priority, isEnabled = isEnabled, isOneShot = isOneShot } );
			}
			else
			{
				if( lastBelowIndex == this.methods.Count - 1 )
				{
					this.methods.Add( new InputMethod() { method = method, priority = priority, isEnabled = isEnabled, isOneShot = isOneShot } );
				}
				else
				{
					this.methods.Insert( lastBelowIndex + 1, new InputMethod() { method = method, priority = priority, isEnabled = isEnabled, isOneShot = isOneShot } );
				}
			}
		}

		public bool Remove( System.Action<InputQueue> method )
		{
			for( int i = 0; i < methods.Count; i++ )
			{
				if( this.methods[i].method == method )
				{
					this.methods.RemoveAt( i );
					return true;
				}
			}
			return false;
		}

		public void StopExecution()
		{
			this.isStopped = true;
		}

		public void Execute()
		{
			this.isStopped = false;
			List<InputMethod> oneShots = new List<InputMethod>();

			for( int i = 0; i < this.methods.Count; i++ )
			{
				if( !this.methods[i].isEnabled )
				{
					continue;
				}
				if( this.isStopped )
				{
					return;
				}
				if( this.methods[i].isOneShot )
				{
					oneShots.Add( this.methods[i] );
				}
				this.methods[i].method.Invoke( this );
			}
			for( int i = 0; i < oneShots.Count; i++ )
			{
				this.methods.Remove( oneShots[i] );
			}
		}

		public void Enable( System.Action<InputQueue> method )
		{
			for( int i = 0; i < this.methods.Count; i++ )
			{
				if( this.methods[i].method != method )
				{
					continue;
				}
				if( this.methods[i].isEnabled )
				{
					throw new System.Exception( "The method is already enabled." );
				}

				this.methods[i].isEnabled = true;
				return;
			}
		}

		public void Disable( System.Action<InputQueue> method )
		{
			for( int i = 0; i < this.methods.Count; i++ )
			{
				if( this.methods[i].method != method )
				{
					continue;
				}
				if( !this.methods[i].isEnabled )
				{
					throw new System.Exception( "The method is already disabled." );
				}

				this.methods[i].isEnabled = false;
				return;
			}
		}
	}
}