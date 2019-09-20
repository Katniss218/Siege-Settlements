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

		/// <summary>
		/// Called when mouse starts hovering over specific object.
		/// </summary>
		public static _UnityEvent_GameObject onMouseEnter = new _UnityEvent_GameObject();

		/// <summary>
		/// Called every frame, when while the mouse is hovering over specific object.
		/// </summary>
		public static _UnityEvent_GameObject onMouseStay = new _UnityEvent_GameObject();

		/// <summary>
		/// Called when mouse stops hovering over specific object.
		/// </summary>
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
					ToolTip.AddText( def.icon.Item2, kvp.Value.ToString() + "/" + deposit.inventory.GetMaxCapacity( kvp.Key ) );
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
			// If the object under mouse pointer has changed to null (stopped mouseovering).
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				// Only call the onMouseExit if the pointer leaves object that was there (!= null).
				if( currentObjectMouseOver != null )
				{
					onMouseExit?.Invoke( currentObjectMouseOver );

					currentObjectMouseOver = null; // newObjectMouseOver
				}
			}
			else
			{
				GameObject newObjectMouseOver = null;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
				{
					newObjectMouseOver = hitInfo.collider.gameObject;

					// If the object under mouse pointer has not changed (the same object or still nothing).
					if( newObjectMouseOver == currentObjectMouseOver )
					{
						// Only call the onMouseStay if the pointer hovers over object that is there (!= null).
						if( currentObjectMouseOver != null )
						{
							onMouseStay?.Invoke( currentObjectMouseOver );
						}
					}
					// If the object under mouse pointer has changed (either to another object or stopped mouseovering).
					else
					{
						// Only call the onMouseExit if the pointer leaves object that was there (!= null).
						if( currentObjectMouseOver != null )
						{
							onMouseExit?.Invoke( currentObjectMouseOver );
						}

						currentObjectMouseOver = newObjectMouseOver;

						// Only call the onMouseEnter if the pointer enters object that is there (!= null).
						if( currentObjectMouseOver != null )
						{
							onMouseEnter?.Invoke( currentObjectMouseOver );
						}
					}
				}
				else
				{
					// Only call the onMouseExit if the pointer leaves object that was there (!= null).
					if( currentObjectMouseOver != null )
					{
						onMouseExit?.Invoke( currentObjectMouseOver );

						currentObjectMouseOver = null;
					}
				}
			}
		}
	}
}