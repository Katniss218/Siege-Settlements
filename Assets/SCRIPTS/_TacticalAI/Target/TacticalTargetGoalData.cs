using KFF;
using SS.Objects.Modules;
using System;

namespace SS.AI.Goals
{
	public class TacticalTargetGoalData : TacticalGoalData
	{
		public Targeter.TargetingMode targetingMode { get; set; }
		public Guid? targetGuid { get; set; }


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.targetingMode = (Targeter.TargetingMode)serializer.ReadByte( "TargetingMode" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'TargetingMode' (" + serializer.file.fileName + ")." );
			}

			if( serializer.Analyze( "TargetGuid" ).isSuccess )
			{
				try
				{
					this.targetGuid = Guid.ParseExact( serializer.ReadString( "TargetGuid" ), "D" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'TargetGuid' (" + serializer.file.fileName + ")." );
				}
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteByte( "", "TargetingMode", (byte)this.targetingMode );
			if( this.targetGuid != null )
			{
				serializer.WriteString( "", "TargetGuid", this.targetGuid.Value.ToString( "D" ) );
			}
		}
	}
}