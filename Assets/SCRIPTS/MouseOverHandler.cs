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
		private static IMouseOverHandlerListener[] currentListeners;
		
		
		void Update()
		{
			// If the object under mouse pointer has changed to null (stopped mouseovering).
			if( EventSystem.current.IsPointerOverGameObject() )
			{
				// Only call the onMouseExit if the pointer leaves object that was there (!= null).
				if( currentObjectMousedOver != null )
				{
					for( int i = 0; i < currentListeners.Length; i++ )
					{
						currentListeners[i].OnMouseExitListener();
					}
					//onMouseExit?.Invoke( currentObjectMouseOver );

					currentObjectMousedOver = null; // newObjectMouseOver
				}
			}
			else
			{
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
				{
					GameObject newObjectMouseOver = hitInfo.collider.gameObject;
					IMouseOverHandlerListener[] newListeners = newObjectMouseOver.GetComponents<IMouseOverHandlerListener>();
					/*IMouseOverHandlerListener newObjectMouseOver = hitInfo.collider.GetComponent<IMouseOverHandlerListener>();
					if( newObjectMouseOver == null )
					{
						return;
					}*/

					// If the object under mouse pointer has not changed (the same object or still nothing).
					if( newObjectMouseOver == currentObjectMousedOver )
					{
						// Only call the onMouseStay if the pointer hovers over object that is there (!= null).
						if( currentObjectMousedOver != null )
						{
							for( int i = 0; i < currentListeners.Length; i++ )
							{
								currentListeners[i].OnMouseStayListener();
							}
							//onMouseStay?.Invoke( currentObjectMouseOver );
						}
					}
					// If the object under mouse pointer has changed (either to another object or stopped mouseovering).
					else
					{
						// Only call the onMouseExit if the pointer leaves object that was there (!= null).
						if( currentObjectMousedOver != null )
						{
							for( int i = 0; i < currentListeners.Length; i++ )
							{
								currentListeners[i].OnMouseExitListener();
							}
							//onMouseExit?.Invoke( currentObjectMouseOver );
						}

						currentObjectMousedOver = newObjectMouseOver;
						currentListeners = newListeners;

						// Only call the onMouseEnter if the pointer enters object that is there (!= null).
						if( currentObjectMousedOver != null )
						{
							for( int i = 0; i < currentListeners.Length; i++ )
							{
								currentListeners[i].OnMouseEnterListener();
							}
							//onMouseEnter?.Invoke( currentObjectMouseOver );
						}
					}
				}
				else
				{
					// Only call the onMouseExit if the pointer leaves object that was there (!= null).
					if( currentObjectMousedOver != null )
					{
						for( int i = 0; i < currentListeners.Length; i++ )
						{
							currentListeners[i].OnMouseExitListener();
						}
						//onMouseExit?.Invoke( currentObjectMouseOver );

						currentObjectMousedOver = null;
					}
				}
			}
		}
	}
}