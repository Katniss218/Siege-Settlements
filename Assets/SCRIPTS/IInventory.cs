using SS.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public interface IInventory
	{
		int canHold( string resId, ushort resAmt ); // returns how much the inventory can hold.

		void Add( string resId, ushort resAmt );
		void Remove( string resId, ushort resAmtMax );

		void Clear();
	}
}