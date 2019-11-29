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

		[SerializeField] private Transform buttonsParent = null;

		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another action panel active" );
			}
			instance = this;
		}


		public void CreateButton( string id, Sprite icon, string displayName, string displayDescription, UnityAction onClick )
		{
			if( this.actionButtons.Count >= 9 )
			{
				Debug.LogWarning( "Tried adding more than 9 action buttons." );
				return;
			}

			GameObject button = UIUtils.InstantiateIconButton( buttonsParent, new GenericUIData(), icon, onClick );
			ToolTipUIHandler toolTipUIhandler = button.AddComponent<ToolTipUIHandler>();
			toolTipUIhandler.constructToolTip = () =>
			{
				ToolTip.Create( 450.0f, displayName );

				ToolTip.AddText( displayDescription );
				ToolTip.Style.SetPadding( 48, 48 );
			};

			this.actionButtons.Add( id, button.transform );
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
		
		public bool Clear( string id )
		{
			if( this.actionButtons.TryGetValue( id, out Transform obj ) )
			{
				Object.Destroy( obj.gameObject );
				this.actionButtons.Remove( id );
				return true;
			}
			return false;
		}
	}
}