using KFF;
using System;
using UnityEngine;

namespace SS
{
	public class PickupDepositData : TAIGoalData
	{
		public Guid destinationGuid { get; set; }

		public override void AssignTo( GameObject gameObject )
		{
			TAIGoal.PickupDeposit.AssignTAIGoal( gameObject, Main.GetSSObject( this.destinationGuid ) );
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.destinationGuid = Guid.ParseExact( serializer.ReadString( "DestinationGuid" ), "D" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DestinationGuid' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "DestinationGuid", this.destinationGuid.ToString( "D" ) );
		}
	}
}