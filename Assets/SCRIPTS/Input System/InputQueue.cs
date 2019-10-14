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
			public float priority { get; set; }
		}

		private List<InputMethod> methods;

		public bool isStopped { get; private set; }

		public InputQueue()
		{
			this.methods = new List<InputMethod>();
		}

		public void Add( System.Action<InputQueue> method, float priority, bool isEnabled )
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
				this.methods.Add( new InputMethod() { method = method, priority = priority, isEnabled = isEnabled } );
			}
			else
			{
				if( lastBelowIndex == this.methods.Count - 1 )
				{
					this.methods.Add( new InputMethod() { method = method, priority = priority, isEnabled = isEnabled } );
				}
				else
				{
					this.methods.Insert( lastBelowIndex + 1, new InputMethod() { method = method, priority = priority, isEnabled = isEnabled } );
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
				this.methods[i].method.Invoke( this );
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