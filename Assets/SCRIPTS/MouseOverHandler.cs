using SS.Data;
using SS.Extras;
using SS.ResourceSystem;
using SS.UI;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	public class MouseOverHandler : MonoBehaviour
	{
		public class _UnityEvent_GameObject : UnityEvent<GameObject> { }

		private GameObject curr;

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
				ResourceDefinition def = DataManager.Get<ResourceDefinition>( deposit.resourceId );
				ResourceDepositDefinition def2 = DataManager.Get<ResourceDepositDefinition>( deposit.id );
				ToolTip.Create( 200, def2.displayName );
				ToolTip.AddIcon( def.icon.Item2 );
				ToolTip.AddText( "Amount", deposit.amount + "/" + deposit.amountMax );
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
			GameObject pointerOver = null;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo ) )
			{
				pointerOver = hitInfo.collider.gameObject;
				if( pointerOver != curr )
				{
					onMouseExit?.Invoke( curr );
					onMouseEnter?.Invoke( pointerOver );
				}
				else
				{
					if( curr != null )
					{
						onMouseStay?.Invoke( curr );
					}
				}
			}
			else
			{
				onMouseExit?.Invoke( null );
			}
			curr = pointerOver;
		}
	}
}