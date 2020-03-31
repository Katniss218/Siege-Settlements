using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.UI
{
	public enum ActionButtonAlignment : byte
	{
		UpperLeft,
		UpperRight,
		MiddleLeft,
		MiddleRight,
		LowerLeft,
		LowerRight
	}

	public enum ActionButtonType : byte
	{
		Object,
		Module
	}

	[DisallowMultipleComponent]
	public class ActionPanel : MonoBehaviour
	{
		private Dictionary<string, Transform> objectButtons = new Dictionary<string, Transform>();
		private Dictionary<string, Transform> moduleButtons = new Dictionary<string, Transform>();


		public static ActionPanel instance { get; private set; }

		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another action panel active" );
			}
			instance = this;
		}

		private static Vector2 AlignmentToPosition( ActionButtonAlignment alignment, ActionButtonType offset )
		{
			Vector2 position = Vector2.zero;
			switch( alignment )
			{
				case ActionButtonAlignment.UpperLeft:
					position = new Vector2( 0, 104 + 20 );
					break;
				case ActionButtonAlignment.UpperRight:
					position = new Vector2( 47 + 10, 104 + 20 );
					break;
				case ActionButtonAlignment.MiddleLeft:
					position = new Vector2( 0, 57 + 10 );
					break;
				case ActionButtonAlignment.MiddleRight:
					position = new Vector2( 47 + 10, 57+10 );
					break;
				case ActionButtonAlignment.LowerLeft:
					position = new Vector2( 0, 10 );
					break;
				case ActionButtonAlignment.LowerRight:
					position = new Vector2( 47 + 10, 10 );
					break;
			}
			if( offset == ActionButtonType.Object )
			{
				position += new Vector2( 50, 0 );
			}
			if( offset == ActionButtonType.Module )
			{
				position += new Vector2( 184, 0 );
			}
			return position;
		}

		// if an action button wants to be created where another action button already is - throw an exception.

		public void CreateButton( string id, Sprite icon, string displayName, string description, ActionButtonAlignment alignment, ActionButtonType type, UnityAction onClick )
		{
			Vector2 position = AlignmentToPosition( alignment, type );
			if( type == ActionButtonType.Object )
			{
				if( this.objectButtons.ContainsKey( id ) )
				{
					throw new System.Exception( "A button with an id '" + id + "' already exists." );
				}

				GameObject button = UIUtils.InstantiateIconButton( this.transform, new GenericUIData( position, new Vector2( 47, 47 ), Vector2.zero, Vector2.zero, Vector2.zero ), icon, onClick );
				ToolTipUIHandler toolTipUIhandler = button.AddComponent<ToolTipUIHandler>();
				toolTipUIhandler.constructToolTip = () =>
				{
					ToolTip.Create( 450.0f, displayName );

					ToolTip.AddText( description );
					ToolTip.Style.SetPadding( 48, 48 );
				};

				// left
				this.objectButtons.Add( id, button.transform );
			}
			else if( type == ActionButtonType.Module )
			{
				if( this.moduleButtons.ContainsKey( id ) )
				{
					throw new System.Exception( "A button with an id '" + id + "' already exists." );
				}

				GameObject button = UIUtils.InstantiateIconButton( this.transform, new GenericUIData( position, new Vector2( 47, 47 ), Vector2.zero, Vector2.zero, Vector2.zero ), icon, onClick );
				ToolTipUIHandler toolTipUIhandler = button.AddComponent<ToolTipUIHandler>();
				toolTipUIhandler.constructToolTip = () =>
				{
					ToolTip.Create( 450.0f, displayName );

					ToolTip.AddText( description );
					ToolTip.Style.SetPadding( 48, 48 );
				};
				
				// right
				this.moduleButtons.Add( id, button.transform );
			}
		}
		
		public Transform GetActionButton( string id, ActionButtonType type )
		{
			if( type == ActionButtonType.Object )
			{
				if( this.objectButtons.TryGetValue( id, out Transform ret ) )
				{
					return ret;
				}
			}
			else if( type == ActionButtonType.Module )
			{
				if( this.moduleButtons.TryGetValue( id, out Transform ret ) )
				{
					return ret;
				}
			}
			return null;
		}

		public void ClearAll()
		{
			foreach( Transform obj in this.objectButtons.Values )
			{
				Object.Destroy( obj.gameObject );
			}
			foreach( Transform obj in this.moduleButtons.Values )
			{
				Object.Destroy( obj.gameObject );
			}
			this.objectButtons.Clear();
			this.moduleButtons.Clear();
		}

		public void ClearAll( ActionButtonType type )
		{
			if( type == ActionButtonType.Object )
			{
				foreach( Transform obj in this.objectButtons.Values )
				{
					Object.Destroy( obj.gameObject );
				}
				this.objectButtons.Clear();
			}
			else if( type == ActionButtonType.Module )
			{
				foreach( Transform obj in this.moduleButtons.Values )
				{
					Object.Destroy( obj.gameObject );
				}
				this.moduleButtons.Clear();
			}
		}
		
		public bool Clear( string id, ActionButtonType type )
		{
			Transform obj;
			if( type == ActionButtonType.Object )
			{
				if( this.objectButtons.TryGetValue( id, out obj ) )
				{
					Object.Destroy( obj.gameObject );
					this.objectButtons.Remove( id );
					return true;
				}
			}
			else if( type == ActionButtonType.Module )
			{
				if( this.moduleButtons.TryGetValue( id, out obj ) )
				{
					Object.Destroy( obj.gameObject );
					this.moduleButtons.Remove( id );
					return true;
				}
			}
			return false;
		}
	}
}