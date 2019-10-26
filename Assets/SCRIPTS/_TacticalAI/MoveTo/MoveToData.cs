using KFF;
using UnityEngine;

namespace SS
{
	public class MoveToData : TAIGoalData
	{
		public Vector3 destination { get; set; }

		public override void AssignTo( GameObject gameObject )
		{
			TAIGoal.MoveTo.AssignTAIGoal( gameObject, this.destination );
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.destination = serializer.ReadVector3( "Destination" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteVector3( "", "Destination", this.destination );
		}
	}
}