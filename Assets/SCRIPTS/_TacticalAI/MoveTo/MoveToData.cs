using KFF;
using System;
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
			try
			{
				this.destination = serializer.ReadVector3( "Destination" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Destination' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteVector3( "", "Destination", this.destination );
		}
	}
}