using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public class GenericUnitInventory : IInventory
	{
		public string resId { get; private set; }
		public int amt { get; private set; }
		public int amtMax { get; private set; }

		public GenericUnitInventory( int inventorySize )
		{
			this.amtMax = inventorySize;
		}

		public void Add( string resId, ushort resAmt )
		{
			if( this.resId != "" && !this.resId.Equals( resId ) )
			{
				throw new System.Exception( "Resource IDs don't match up." );
			}
			if( this.resId == "" )
			{
				this.resId = resId;
			}
			this.amt += resAmt;
		}

		public int canHold( string resId, ushort resAmt )
		{
			if( this.resId != "" && !this.resId.Equals( resId ) )
			{
				return 0;
			}
			if( amt + resAmt > amtMax )
			{
				return amtMax - amt;
			}
			return resAmt;
		}

		public void Remove( string resId, ushort resAmtMax )
		{
			if( !this.resId.Equals( resId ) )
			{
				throw new System.Exception( "Resource IDs don't match up." );
			}
			if( amt - resAmtMax <= 0 )
			{
				amt = 0;
				resId = "";
			}
			else
			{
				amt -= resAmtMax;
			}
		}

		public void Clear()
		{
			this.resId = "";
			this.amt = 0;
		}
	}
}