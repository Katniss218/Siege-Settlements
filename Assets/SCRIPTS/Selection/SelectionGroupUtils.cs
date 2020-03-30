using SS.Objects;
using System;
using System.Collections.Generic;


namespace SS
{
	public static class SelectionGroupUtils
	{
		public static Guid[][] GetSaveData()
		{
			Guid[][] ret = new Guid[10][];

			for( byte i = 0; i < ret.Length; i++ )
			{

				SSObject[] group = Selection.GetGroup( i );

				ret[i] = new Guid[group.Length];

				for( int j = 0; j < group.Length; j++ )
				{
					ret[i][j] = group[j].guid;
				}
			}

			return ret;
		}

		public static void SetSaveData( Guid[][] groups )
		{
			for( byte i = 0; i < groups.Length; i++ )
			{
				SSObject[] objects = new SSObject[groups[i].Length];

				for( int j = 0; j < groups[i].Length; j++ )
				{
					objects[j] = SSObject.Find( groups[i][j] );
				}

				Selection.SetGroup( i, objects );
			}
		}
	}
}
