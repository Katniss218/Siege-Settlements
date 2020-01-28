using UnityEngine;

namespace SS.Objects
{
	public abstract class SSObjectExtension<T> : MonoBehaviour where T : SSObject
	{
		private T __obj = null;
		/// <summary>
		/// Returns the base object.
		/// </summary>
		public T obj
		{
			get
			{
				if( __obj == null )
				{
					this.__obj = this.GetComponent<T>();
				}
				return this.__obj;
			}
		}

		// Force require base component.
		protected virtual void Awake()
		{
			if( this.obj == null )
			{
				this.__obj = this.gameObject.AddComponent<T>();
			}
		}

		/*
		protected virtual void OnEnable()
		{
			
		}

		protected virtual void OnDisable()
		{
			
		}
		*/
	}
}