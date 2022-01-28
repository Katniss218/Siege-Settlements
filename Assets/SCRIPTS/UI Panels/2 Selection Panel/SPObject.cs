using SS.Objects;
using SS.Objects.Modules;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class SPObject : MonoBehaviour
	{
		private Dictionary<string, Transform> elements = new Dictionary<string, Transform>();

		[SerializeField] private Image objIcon = null;
		[SerializeField] private Transform _moduleUITransform = null;
		[SerializeField] private TextMeshProUGUI _displayNameText = null;

		public Transform moduleUITransform { get { return this._moduleUITransform; } }
		public TextMeshProUGUI displayNameText { get { return this._displayNameText; } }

		private Dictionary<SSModule, Image> moduleIcons = new Dictionary<SSModule, Image>();

		private Image highlightedIcon = null;
		

		public void SetIcon( Sprite icon )
		{
			this.objIcon.sprite = icon;
			if( !this.objIcon.gameObject.activeSelf )
			{
				this.objIcon.gameObject.SetActive( true );
			}
		}

		public void ClearIcon()
		{
			if( this.objIcon.gameObject.activeSelf )
			{
				this.objIcon.gameObject.SetActive( false );
			}
			this.objIcon.sprite = null;
		}



		/// <summary>
		/// Registers an element with an id.
		/// </summary>
		/// <param name="id">The id to register the object with.</param>
		/// <param name="element">The object itself.</param>
		public void RegisterElement( string id, Transform element )
		{
			// Fail if the element is already registered
			if( this.elements.TryGetValue( id, out Transform obj ) )
			{
				if( obj.transform == element )
				{
					throw new System.Exception( $"An element with id '{id}' is already registered as '{element.gameObject.name}'." );
				}
			}

			// Register, if the element is a direct child of the selection panel
			for( int i = 0; i < this.transform.childCount; i++ )
			{
				if( this.transform.GetChild( i ) == element )
				{
					elements.Add( id, element );
					return;
				}
			}

			throw new System.Exception( $"The element '{element.gameObject.name}' is not a child of SPObject." );
		}

		/// <summary>
		/// Returns a registered element with a matched id 'id'.
		/// </summary>
		/// <param name="id">The id to check.</param>
		public Transform GetElement( string id )
		{
			if( this.elements.TryGetValue( id, out Transform ret ) )
			{
				return ret;
			}
			return null;
		}


		/// <summary>
		/// Clears the UI element that was registered using the specified id.
		/// </summary>
		/// <param name="id">The id to check.</param>
		public bool TryClearElement( string id )
		{
			if( this.elements.TryGetValue( id, out Transform obj ) )
			{
				Object.Destroy( obj.gameObject );
				this.elements.Remove( id );
				return true;
			}
			return false;
		}

		public void ClearAllElements()
		{
			foreach( Transform obj in this.elements.Values )
			{
				Object.Destroy( obj.gameObject );
			}
			this.elements.Clear();
		}




		public void CreateModuleButton( SSModule module )
		{
			if( !(module is ISelectDisplayHandler) )
			{
				throw new System.ArgumentException( "Module must implement ISelectDisplayHandler" );
			}
			
			GameObject moduleIconGameObject = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.moduleUITransform, new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), module.icon, () =>
			{
				Selection.DisplayModule( module.ssObject as SSObjectDFC, module );
			} );

			ToolTipUIHandler toolTipUIhandler = moduleIconGameObject.AddComponent<ToolTipUIHandler>();
			toolTipUIhandler.constructToolTip = () =>
			{
				ToolTip.Create( 270, module.displayName );

				ToolTip.AddText( "Click to view..." );
				ToolTip.Style.SetPadding( 60, 60 );
			};

			Image moduleIcon = moduleIconGameObject.GetComponent<Image>();
			this.moduleIcons.Add( module, moduleIcon );
			this.UnHighlight( moduleIcon );
		}

		public void ClearAllModules()
		{
			for( int i = 0; i < this.moduleUITransform.childCount; i++ )
			{
				Object.Destroy( this.moduleUITransform.GetChild( i ).gameObject );
			}
			this.moduleIcons.Clear();
		}

		private void Highlight( Image image )
		{
			image.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		}

		private void UnHighlight( Image image )
		{
			image.color = new Color( 0.53f, 0.5f, 0.5f, 1.0f );
		}
		
		/// <summary>
		/// Hightlights the module's icon. Unhighlights any other icons.
		/// </summary>
		public void HighlightIcon( SSModule module )
		{
			if( this.highlightedIcon != null )
			{
				this.UnHighlight( this.highlightedIcon );
			}
			if( this.moduleIcons.TryGetValue( module, out Image icon ) )
			{
				this.highlightedIcon = icon;
				this.Highlight( icon );
			}
		}
	}
}