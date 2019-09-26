using KFF;

namespace SS.Levels.SaveStates
{
	public class ConstructionSiteData : IKFFSerializable
	{
#error TODO - change this to dictionary, and the construction site's fields to dict as well.
		public float[] resourcesRemaining { get; set; }


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