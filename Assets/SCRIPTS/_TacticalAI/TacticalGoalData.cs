using KFF;

namespace SS.AI.Goals
{
	public abstract class TacticalGoalData : IKFFSerializable
	{
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );
	}
}