using System;
using System.Collections;
using System.Collections.Generic;
using KFF;
using UnityEngine;

namespace SS
{
	public class DropoffToInventoryData : TAIGoalData
	{
		Guid destinationGuid { get; set; }

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.destinationGuid = getFromArray( read int index )
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
#warning add guid identifier to each object when spawning
#error still requires having all objects spawned before adding anything that references those guids.
		throw new System.NotImplementedException();
		}
	}
}