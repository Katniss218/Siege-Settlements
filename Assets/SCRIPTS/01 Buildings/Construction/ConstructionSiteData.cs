using KFF;
using System.Collections.Generic;

namespace SS.Levels.SaveStates
{
	public class ConstructionSiteData : IKFFSerializable
	{
		public Dictionary<string, float> resourcesRemaining { get; set; }

		//public float[] resourcesRemaining { get; set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
#warning incomplete. setup the data structure firstly tho.
			throw new System.Exception();
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
#warning incomplete. setup the data structure firstly tho.
			throw new System.Exception();
		}
	}
}