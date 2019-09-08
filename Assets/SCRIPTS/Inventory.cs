using SS.Data;
using SS.Extras;
using SS.ResourceSystem;
using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	/// <summary>
	/// Allows an object to hold resources in it's inventory.
	/// </summary>
	public class Inventory : MonoBehaviour
	{
		public ResourceStack resource { get; private set; }

		public int maxCapacity { get; set; }


		public UnityEvent onPickup = new UnityEvent();
		public UnityEvent onDropOff = new UnityEvent();

		/// <summary>
		/// Returns true, if the inventory is currently holding a resource.
		/// </summary>
		public bool isCarryingResource
		{
			get
			{
				return this.resource != null;
			}
		}

		public bool CanPickupResource( ResourceStack stack )
		{
			if( stack == null )
			{
				return false;
			}
			int currentAmount = 0;
			if( resource != null )
			{
				if( resource.id != stack.id )
				{
					return false;
				}
				currentAmount = stack.amount;
			}
			if( currentAmount + stack.amount > maxCapacity )
			{
				return false;
			}
			return true;
		}

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

		public bool DropOffResource()
		{
			if( resource == null )
			{
				throw new System.Exception( "Can't drop off resource. Not carrying anything." );
			}

			RaycastHit hitInfo;
			if( Physics.Raycast( this.gameObject.transform.position + this.gameObject.transform.forward + new Vector3( 0, 5, 0 ), Vector3.down, out hitInfo ) )
			{
				// Create the dropped deposit in the world.
				if( hitInfo.collider.gameObject.layer == LayerMask.NameToLayer( "Terrain" ) )
				{
					ResourceDeposit.Create( DataManager.Get<ResourceDepositDefinition>( DataManager.Get<ResourceDefinition>( this.resource.id ).defaultDeposit ), hitInfo.point, Quaternion.identity, this.resource.amount );
				}

				onDropOff?.Invoke(); // Invoke it before so we can see what's being dropped off.

				resource = null;
				return true;
			}
			return false;
		}
	}
}