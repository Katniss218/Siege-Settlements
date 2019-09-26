﻿using KFF;
using UnityEngine;

namespace SS
{
	public class DropoffToNewData : TAIGoalData
	{
		public Vector3 destination { get; set; }


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