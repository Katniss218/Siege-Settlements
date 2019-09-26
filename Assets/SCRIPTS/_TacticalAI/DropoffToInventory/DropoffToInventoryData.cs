﻿using System;
using KFF;
using UnityEngine;

namespace SS
{
	public class DropoffToInventoryData : TAIGoalData
	{
		public Guid destinationGuid { get; set; }


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