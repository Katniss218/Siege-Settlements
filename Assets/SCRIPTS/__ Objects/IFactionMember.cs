using UnityEngine.Events;

namespace SS.Objects
{
	public class UnityEvent_int_int : UnityEvent<int, int> { }

	public interface IFactionMember
	{

		int factionId { get; set; }

		UnityEvent_int_int onFactionChange { get; }
		//FactionMember factionMember { get; }
	}
}