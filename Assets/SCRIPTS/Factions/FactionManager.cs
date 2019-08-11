using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public static class FactionManager
	{
		public static Faction[] factions;

		public static void SetFactions( Faction[] factions )
		{
			FactionManager.factions = factions;
		}
	}
}