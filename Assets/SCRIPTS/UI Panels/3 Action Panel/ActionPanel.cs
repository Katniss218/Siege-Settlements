using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class ActionPanel : MonoBehaviour
	{
		public static ActionPanel instance { get; private set; }

		private Dictionary<string, Transform> actionButtons = new Dictionary<string, Transform>();

		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another action panel active" );
			}
			instance = this;
		}


		public void CreateButton( string id, Sprite icon, UnityAction onClick )
		{
#warning incomplete.
		}

		public Transform GetActionButton( string id )
		{
			if( this.actionButtons.TryGetValue( id, out Transform ret ) )
			{
				return ret;
			}
			return null;
		}
		
		public void ClearAll()
		{
			foreach( Transform obj in this.actionButtons.Values )
			{
				Object.Destroy( obj.gameObject );
			}
			this.actionButtons.Clear();
		}
		
		public void Clear( string id )
		{
			if( this.actionButtons.TryGetValue( id, out Transform obj ) )
			{
				Object.Destroy( obj.gameObject );
				this.actionButtons.Remove( id );
			}
		}
	}
}