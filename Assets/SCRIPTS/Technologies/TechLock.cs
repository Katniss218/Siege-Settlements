using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Technologies
{
	public static class TechLock
	{
		public static bool CheckLocked( ITechsRequired obj, Dictionary<string, TechnologyResearchProgress> listOfTechs )
		{
			for( int i = 0; i < obj.techsRequired.Length; i++ )
			{
				TechnologyResearchProgress prog;
				if( listOfTechs.TryGetValue( obj.techsRequired[i], out prog ) )
				{
					// If the required tech is not researched
					if( prog != TechnologyResearchProgress.Researched )
					{
						return true;
					}
				}
				// If the required tech is not present.
				else
				{
					Debug.LogWarning( "CheckLocked: The required tech was not present in the list of all techs." );
					return true;
				}
			}
			return false;
		}
	}
}