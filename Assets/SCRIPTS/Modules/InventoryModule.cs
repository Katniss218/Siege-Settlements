using SS.ResourceSystem;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Allows an object to hold resources in it's inventory.
	/// </summary>
	public class InventoryModule : MonoBehaviour
	{
		public ResourceStack resource = null;

		public int maxCapacity;


		public UnityEvent onPickup = new UnityEvent();
		public UnityEvent onDropOff = new UnityEvent();

		public void PickupResource( ResourceStack stack )
		{
			if( stack == null )
			{
				throw new System.Exception( "Can't pick up nothing" );
			}
			int currentAmount = 0;
			if( resource != null )
			{
				if( resource.id != stack.id )
				{
					throw new System.Exception( "Can't pick up resource. Already carrying different type." );
				}
				currentAmount = stack.amount;
			}
			if( currentAmount + stack.amount > maxCapacity )
			{
				throw new System.Exception( "Can't pick up resource. Not enough space." );
			}
			resource = new ResourceStack( stack.id, currentAmount + stack.amount );
			onPickup?.Invoke();
		}

		public void DropOffResource()
		{
			if( resource == null )
			{
				throw new System.Exception( "Can't drop off resource. Not carrying anything." );
			}

			onDropOff?.Invoke(); // Invoke it before so we can see what's being dropped off.
			resource = null;
		}

		private void Update()
		{
			if( Input.GetKeyDown( KeyCode.T ) )
			{
				PickupResource( new ResourceStack( "resource.wood", 5 ) );
			}
			if( Input.GetKeyDown( KeyCode.G ) )
			{
				DropOffResource();
			}
		}
	}
}