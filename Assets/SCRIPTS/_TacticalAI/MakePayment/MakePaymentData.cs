using KFF;
using System;
using UnityEngine;

namespace SS
{
	public class MakePaymentData : TAIGoalData
	{
		public Guid destinationGuid { get; set; }

		public override void AssignTo( GameObject gameObject )
		{
			TAIGoal.MakePayment.AssignTAIGoal( gameObject, Main.GetSSObject( this.destinationGuid ) );
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.destinationGuid = Guid.ParseExact( serializer.ReadString( "DestinationGuid" ), "D" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "DestinationGuid", this.destinationGuid.ToString( "D" ) );
		}
	}
}