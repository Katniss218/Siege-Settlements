using SS.Objects.Extras;
using SS.Objects.Modules;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects
{
	public class SSObjectUtils
	{
		public static void ReDisplayDisplayed()
		{
			SSObjectDFSC displayed = Selection.displayedObject;
			SSModule displayedModule = Selection.displayedModule;
			if( displayed != null )
			{
				Selection.StopDisplaying();
				Selection.DisplayObject( displayed );
				if( displayedModule != null )
				{
					Selection.DisplayModule( displayed, displayedModule );
				}
			}
		}

		public static InventoryModule GetClosestInventoryContaining( Vector3 pos, int factionId, string resourceId )
		{
			SSObjectDFSC[] objects = SSObject.GetAllDFSC();

			InventoryModule ret = null;
			float dstSq = float.MaxValue;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( !objects[i].hasInventoryModule )
				{
					continue;
				}

				if( objects[i].factionId != factionId )
				{
					continue;
				}

				// If is in range.
				float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSq )
				{
					continue;
				}

				// discard any objects that are unusable.
				if( objects[i] is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)objects[i]).isUsable )
				{
					continue;
				}

				// If has resource deposit.
				InventoryModule[] inventories = objects[i].GetModules<InventoryModule>();

				// If inventory is storage & contains wanted resource.
				for( int j = 0; j < inventories.Length; j++ )
				{
					if( inventories[j].isStorage )
					{
						if( inventories[j].GetAll().ContainsKey( resourceId ) )
						{
							dstSq = newDstSq;
							ret = inventories[j];
							break; // break inner loop
						}
					}
				}
			}
			return ret;
		}

		public static ResourceDepositModule GetClosestInRangeContaining( Vector3 pos, float range, string resourceId )
		{
			if( NavMesh.SamplePosition( pos, out NavMeshHit hit, range, int.MaxValue ) )
			{
				pos = hit.position;

				Extra[] extras = SSObject.GetAllExtras();
				ResourceDepositModule ret = null;

				float dstSq = range * range;

				for( int i = 0; i < extras.Length; i++ )
				{
					// If is in range.
					float newDstSq = (pos - extras[i].transform.position).sqrMagnitude;
					if( newDstSq > dstSq )
					{
						continue;
					}

					// If has resource deposit.
					ResourceDepositModule[] resourceDeposits = extras[i].GetModules<ResourceDepositModule>();

					if( resourceDeposits.Length == 0 )
					{
						continue;
					}

					// If deposit contains wanted resource.
					for( int j = 0; j < resourceDeposits.Length; j++ )
					{
						if( resourceDeposits[j].GetAll().ContainsKey( resourceId ) )
						{
							dstSq = newDstSq;
							ret = resourceDeposits[j];
							break; // break inner loop
						}
					}
				}
				return ret;
			}
			else
			{
				// nothing can be reached.
				return null;
			}
		}


		public static IPaymentReceiver GetClosestWantingPayment( Vector3 pos, int factionId, string[] resourceIds )
		{
			SSObjectDFSC[] objects = SSObject.GetAllDFSC();

			IPaymentReceiver ret = null;
			float dstSqToLastValid = float.MaxValue;

			Dictionary<string, int> resourcesWanted;
			IPaymentReceiver[] paymentReceivers;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i].factionId != factionId )
				{
					continue;
				}

				if( !objects[i].HasPaymentReceivers() )
				{
					continue;
				}

				float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSqToLastValid )
				{
					continue;
				}


				paymentReceivers = objects[i].GetAvailableReceivers();


				for( int j = 0; j < paymentReceivers.Length; j++ )
				{
					resourcesWanted = paymentReceivers[j].GetWantedResources();
					if( resourcesWanted.Count == 0 )
					{
						break;
					}

					if( resourceIds != null )
					{
						for( int k = 0; k < resourceIds.Length; k++ )
						{
							if( resourcesWanted.ContainsKey( resourceIds[k] ) )
							{
								dstSqToLastValid = newDstSq;
								ret = paymentReceivers[j];
								break; // break inner loop
							}
						}
					}
				}
			}
			return ret;
		}


		public static InventoryModule GetClosestWithSpace( SSObject self, Vector3 pos, string resourceId, int factionId )
		{
			SSObjectDFSC[] objects = SSObject.GetAllDFSC();

			InventoryModule ret = null;
			float dstSqToLastValid = float.MaxValue;
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i] == self )
				{
					continue;
				}

				if( !objects[i].hasInventoryModule )
				{
					continue;
				}

				// If is in range.
				float newDstSq = (pos - objects[i].transform.position).sqrMagnitude;
				if( newDstSq > dstSqToLastValid )
				{
					continue;
				}

				if( objects[i].factionId != factionId )
				{
					continue;
				}

				// discard any objects that are unusable.
				if( objects[i] is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)objects[i]).isUsable )
				{
					continue;
				}

				InventoryModule[] inventories = objects[i].GetModules<InventoryModule>();

				for( int j = 0; j < inventories.Length; j++ )
				{
					if( inventories[j].isStorage )
					{
						if( inventories[j].GetSpaceLeft( resourceId ) > 0 )
						{
							dstSqToLastValid = newDstSq; // only mark distance to an actual valid objects.
							ret = inventories[j];
							break; // break inner loop
						}
					}
				}
			}
			return ret;
		}
	}
}