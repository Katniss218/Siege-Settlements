using UnityEngine.Events;

namespace SS.Objects
{
	public interface IFactionMember
	{
		int factionId { get; set; }

		UnityEvent onFactionChange { get; }
		//FactionMember factionMember { get; }
	}
}