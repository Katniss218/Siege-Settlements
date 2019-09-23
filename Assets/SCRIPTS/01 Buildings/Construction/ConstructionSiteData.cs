using KFF;

namespace SS.Levels.SaveStates
{
	public class ConstructionSiteData : IKFFSerializable
	{
#warning TODO - change this to dictionary, and the construction site's fields to dict as well.
		public float[] resourcesRemaining { get; set; }
#warning TODO - building can be under construction but no resources remaining data might be present to set.

		public void DeserializeKFF( KFFSerializer serializer )
		{
#warning incomplete.
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
#warning incomplete.
		}
	}
}