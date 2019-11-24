using SS.Modules;
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

		[SerializeField] private Image highlightedObjIcon = null;
		[SerializeField] private Transform _moduleUITransform = null;
		[SerializeField] private TextMeshProUGUI _displayNameText = null;

		public Transform moduleUITransform { get { return this._moduleUITransform; } }
		public TextMeshProUGUI displayNameText { get { return this._displayNameText; } }


		void Start()
		{
			this.displayNameText.text = "";
			this.ClearIcon();
			this.highlightedObjIcon.GetComponent<Button>().onClick.AddListener( () =>
			{
				ISelectDisplayHandler displayed = Selection.displayedObject;
				if( displayed is SSModule )
				{
					Selection.Display( (displayed as SSModule).ssObject as ISelectDisplayHandler );
				}
			} );
		}

		public void SetIcon( Sprite icon )
		{
			this.highlightedObjIcon.sprite = icon;
			if( !this.highlightedObjIcon.gameObject.activeSelf )
			{
				this.highlightedObjIcon.gameObject.SetActive( true );
			}
		}

		public void ClearIcon()
		{
			if( this.highlightedObjIcon.gameObject.activeSelf )
			{
				this.highlightedObjIcon.gameObject.SetActive( false );
			}
			this.highlightedObjIcon.sprite = null;
		}

		/// <summary>
		/// Registers an element 'element' using an id 'id'.
		/// </summary>
		/// <param name="id">The id to register the object with.</param>
		/// <param name="element">The object itself.</param>
		public void RegisterElement( string id, Transform element )
		{
			// If element already registered.
			if( this.elements.TryGetValue( id, out Transform obj ) )
			{
				if( obj.transform == element )
				{
					throw new System.Exception( "An element with id '" + id + "' is already registered as '" + element.gameObject.name + "'." );
				}
			}

			for( int i = 0; i < this.transform.childCount; i++ )
			{
				// if element is a child of SPObject.
				if( this.transform.GetChild( i ) == element )
				{
					elements.Add( id, element );
					return;
				}
			}
			// if element is NOT a child of SPObject.
			throw new System.Exception( "The element '" + element.gameObject.name + "' is not a child of SPObject." );
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

		public void ClearAllElements()
		{
			foreach( Transform obj in this.elements.Values )
			{
				Object.Destroy( obj.gameObject );
			}
			this.elements.Clear();
		}

		public void AddModuleButton( Modules.SSModule module )
		{
			if( !(module is ISelectDisplayHandler) )
			{
				throw new System.Exception( "Module must implement ISelectDisplayHandler" );
			}

			ISelectDisplayHandler moduleSelectDisplayHandler = (ISelectDisplayHandler)module;
			GameObject ui = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.moduleUITransform, new GenericUIData( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero ), module.icon, () =>
			{
				Selection.Display( moduleSelectDisplayHandler );
			} );
		}

		public void ClearModules()
		{
			for( int i = 0; i < this.moduleUITransform.childCount; i++ )
			{
				Object.Destroy( this.moduleUITransform.GetChild( i ).gameObject );
			}
		}
		
		/// <summary>
		/// Clears the UI element that was registered using the specified id.
		/// </summary>
		/// <param name="id">The id to check.</param>
		public void ClearElement( string id )
		{
			if( this.elements.TryGetValue( id, out Transform obj ) )
			{
				Object.Destroy( obj.gameObject );
				this.elements.Remove( id );
			}
		}
	}
}