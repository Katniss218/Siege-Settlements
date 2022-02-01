using SS.Objects.Extras;
using SS.Objects.Modules;
using SS.ResourceSystem.Payment;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects
{
    public static class SSObjectUtils
    {
        public static void ReDisplayDisplayed()
        {
            ISelectDisplayHandler displayed = Selection.displayedObject;
            ISelectDisplayHandler displayedModule = Selection.displayedModule;
            if( displayed != null )
            {
                Selection.StopDisplaying();
                Selection.DisplayObject( (SSObject)displayed );
                if( displayedModule != null )
                {
                    Selection.DisplayModule( (SSObject)displayed, (SSModule)displayedModule );
                }
            }
        }

        public static InventoryModule GetClosestInventoryContaining( Vector3 pos, int factionId, string resourceId )
        {
            SSObjectDFC[] objects = SSObject.GetAllDFC();

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
                ResourceDepositModule closestDeposit = null;

                float dstSq = range * range;

                foreach( var extra in extras )
                {
                    // If is in range.
                    float newDstSq = (pos - extra.transform.position).sqrMagnitude;
                    if( newDstSq > dstSq )
                    {
                        continue;
                    }

                    // If has resource deposit.
                    ResourceDepositModule[] resourceDeposits = extra.GetModules<ResourceDepositModule>();

                    if( resourceDeposits.Length == 0 )
                    {
                        continue;
                    }

                    // If deposit contains wanted resource.
                    foreach( var resourceDeposit in resourceDeposits )
                    {
                        if( resourceDeposit.GetAll().ContainsKey( resourceId ) )
                        {
                            dstSq = newDstSq;
                            closestDeposit = resourceDeposit;
                            break; // break inner loop
                        }
                    }
                }
                return closestDeposit;
            }
            else
            {
                // nothing can be reached.
                return null;
            }
        }


        public static IPaymentReceiver GetClosestWantingPayment( Vector3 pos, int factionId, string[] resourceIds )
        {
            if( resourceIds == null )
            {
                throw new System.Exception( "Null resourceIds" );
            }

            SSObjectDFC[] objects = SSObject.GetAllDFC();

            IPaymentReceiver chosenReceiver = null;
            float dstSqToLastValid = float.MaxValue;

            foreach( var obj in objects )
            {
                if( obj.factionId != factionId )
                {
                    continue;
                }

                if( !obj.HasUsablePaymentReceivers() )
                {
                    continue;
                }

                float newDstSq = (pos - obj.transform.position).sqrMagnitude;
                if( newDstSq > dstSqToLastValid )
                {
                    continue;
                }

                IPaymentReceiver[] paymentReceivers = obj.GetAvailablePaymentReceivers();

                foreach( var paymentReceiver in paymentReceivers )
                {
                    Dictionary<string, int> resourcesWanted = paymentReceiver.GetWantedResources();
                    if( resourcesWanted.Count == 0 )
                    {
                        break;
                    }

                    foreach( var resourceId in resourceIds )
                    {
                        if( resourcesWanted.ContainsKey( resourceId ) )
                        {
                            dstSqToLastValid = newDstSq;
                            chosenReceiver = paymentReceiver;
                            break; // break resource loop
                        }
                    }
                }
            }
            return chosenReceiver;
        }

        /// <summary>
        /// Returns the inventory module of the closest object of a given faction that has space for a given resource
        /// </summary>
        public static InventoryModule GetClosestWithSpace( SSObject self, Vector3 pos, string resourceId, int factionId )
        {
            SSObjectDFC[] objects = SSObject.GetAllDFC();

            InventoryModule ret = null;
            float dstSqToLastValid = float.MaxValue;

            foreach( var obj in objects )
            {
                if( obj == self )
                {
                    continue;
                }

                if( !obj.hasInventoryModule )
                {
                    continue;
                }

                // If is in range.
                float newDstSq = (pos - obj.transform.position).sqrMagnitude;
                if( newDstSq > dstSqToLastValid )
                {
                    continue;
                }

                if( obj.factionId != factionId )
                {
                    continue;
                }

                // discard any objects that are unusable.
                if( obj is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)obj).isUsable )
                {
                    continue;
                }

                InventoryModule[] inventories = obj.GetModules<InventoryModule>();

                foreach( var inv in inventories )
                {
                    if( !inv.isStorage )
                    {
                        continue;
                    }

                    if( inv.GetSpaceLeft( resourceId ) > 0 )
                    {
                        dstSqToLastValid = newDstSq; // only mark distance to an actual valid objects.
                        ret = inv;
                        break; // break inner loop
                    }
                }
            }
            return ret;
        }
    }
}