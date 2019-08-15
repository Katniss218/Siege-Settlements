using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	public sealed class FactionMember : MonoBehaviour
	{
		// TODO ----- change this to event parametrized with Serializable attrib?
		public class _UnityEventFactionMember : UnityEvent<FactionMember> { }
		
		[SerializeField] private int __factionId;
		public int factionId
		{
			get
			{
				return this.__factionId;
			}
			set
			{
				this.__factionId = value;
				this.onFactionChange?.Invoke( this );
			}
		}

		public _UnityEventFactionMember onFactionChange = new _UnityEventFactionMember();
	}
}