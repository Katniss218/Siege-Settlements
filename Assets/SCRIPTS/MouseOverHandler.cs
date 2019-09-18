using SS.Content;
using SS.Extras;
using SS.ResourceSystem;
using SS.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SS
{
	public class MouseOverHandler : MonoBehaviour
	{
		public class _UnityEvent_GameObject : UnityEvent<GameObject> { }

		public static GameObject currentObjectMouseOver { get; private set; }

		public static _UnityEvent_GameObject onMouseEnter = new _UnityEvent_GameObject();
		public static _UnityEvent_GameObject onMouseStay = new _UnityEvent_GameObject();
		public static _UnityEvent_GameObject onMouseExit = new _UnityEvent_GameObject();
		
		void Start()
		{
			onMouseEnter.AddListener( ( GameObject gameObject ) =>
			{
				ResourceDeposit deposit = gameObject.GetComponent<ResourceDeposit>();
				if( deposit == null )
				{
					return;
				}

				ResourceDepositDefinition def2 = DataManager.Get<ResourceDepositDefinition>( deposit.id );
				Dictionary<string, int> itemsInDeposit = deposit.inventory.GetAll();

				ToolTip.Create( 200, def2.displayName );
				
				foreach( var kvp in itemsInDeposit )
				{
					ResourceDefinition def = DataManager.Get<ResourceDefinition>( kvp.Key );
					ToolTip.AddText( def.icon.Item2, kvp.Value.ToString() ); // TODO ----- method for inventories to check how much resource can be added/removed.
				}
				ToolTip.ShowAt( Input.mousePosition );
			} );
			onMouseStay.AddListener( ( GameObject gameObject ) =>
			{
				ToolTip.MoveTo( Input.mousePosition, true );
			} );
			onMouseExit.AddListener( ( GameObject gameObject ) =>
			{
				ToolTip.Hide();
			} );
		}

		void Update()
		{
			// If the pointer is over raycastable UI elements - stop mouseovering.
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				onMouseExit?.Invoke( currentObjectMouseOver );
				currentObjectMouseOver = null;
			}
			else
			{
				GameObject pointerOver = null;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
				{
					pointerOver = hitInfo.collider.gameObject;
					if( pointerOver != currentObjectMouseOver )
					{
						onMouseExit?.Invoke( currentObjectMouseOver );
						onMouseEnter?.Invoke( pointerOver );
					}
					else
					{
						if( currentObjectMouseOver != null )
						{
							onMouseStay?.Invoke( currentObjectMouseOver );
						}
					}
				}
				else
				{
					onMouseExit?.Invoke( null );
				}
				currentObjectMouseOver = pointerOver;
			}
		}
	}
}