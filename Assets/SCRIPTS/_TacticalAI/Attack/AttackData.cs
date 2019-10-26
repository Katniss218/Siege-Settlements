using KFF;
using System;

namespace SS
{
	public class AttackData : TAIGoalData
	{
		public Guid targetGuid { get; set; }


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.targetGuid = Guid.ParseExact( serializer.ReadString( "TargetGuid" ), "D" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "TargetGuid", this.targetGuid.ToString( "D" ) );
		}
	}
}