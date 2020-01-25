using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SS
{
	public interface IMouseOverHandlerListener
	{
		void OnMouseEnterListener();
		void OnMouseStayListener();
		void OnMouseExitListener();
	}

	public class MouseOverHandler : MonoBehaviour
	{

		public class _UnityEvent_GameObject : UnityEvent<GameObject> { }

		public static GameObject currentObjectMousedOver { get; private set; }
		private static IMouseOverHandlerListener[] currentListenersCache; // list of mouse over handler listeners on the mouseovered object.


		void Update()
		{
			// If pointer is over GUI elements, stop mouseovering object.
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				// Only call the onMouseExit if the pointer leaves object that was there (!= null).
				if( currentObjectMousedOver != null )
				{
					for( int i = 0; i < currentListenersCache.Length; i++ )
					{
						currentListenersCache[i].OnMouseExitListener();
					}

					currentObjectMousedOver = null;
				}
				return;
			}

			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
			{
				GameObject newObjectMouseOver = hitInfo.collider.gameObject;
				IMouseOverHandlerListener[] newListeners = newObjectMouseOver.GetComponents<IMouseOverHandlerListener>();

				// If the object under mouse pointer has not changed (the same object or still nothing).
				if( newObjectMouseOver == currentObjectMousedOver )
				{
					if( currentObjectMousedOver != null )
					{
						for( int i = 0; i < currentListenersCache.Length; i++ )
						{
							currentListenersCache[i].OnMouseStayListener();
						}
					}
				}

				// If the object under mouse pointer has changed.
				else
				{
					// Only call the onMouseExit if the pointer leaves object that was there (!= null).
					if( currentObjectMousedOver != null )
					{
						for( int i = 0; i < currentListenersCache.Length; i++ )
						{
							currentListenersCache[i].OnMouseExitListener();
						}
					}

					currentObjectMousedOver = newObjectMouseOver;
					currentListenersCache = newListeners;

					// Only call the onMouseEnter if the pointer enters object that is there (!= null).
					if( currentObjectMousedOver != null )
					{
						for( int i = 0; i < currentListenersCache.Length; i++ )
						{
							currentListenersCache[i].OnMouseEnterListener();
						}
					}
				}
			}

			// If the pointer is over empty space...
			else
			{
				// Only call the onMouseExit if the pointer leaves object that was there (!= null).
				if( currentObjectMousedOver != null )
				{
					for( int i = 0; i < currentListenersCache.Length; i++ )
					{
						currentListenersCache[i].OnMouseExitListener();
					}

					currentObjectMousedOver = null;
				}
			}
		}
	}
}