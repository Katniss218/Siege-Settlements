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
		}

		public List<InputMethod> methods { get; private set; }

		public bool isStopped { get; private set; }

		public InputQueue()
		{
			this.methods = new List<InputMethod>();
		}

		public void StopExecution()
		{
			isStopped = true;
		}

		public void Execute()
		{
			isStopped = false;

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