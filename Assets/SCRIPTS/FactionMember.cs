using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	public interface IFactionMember
	{
		int factionId { get; }

		void SetFaction( int id );
	}
}