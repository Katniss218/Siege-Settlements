using UnityEngine;
using UnityEngine.Events;

namespace SS
{
	public sealed class FactionMember : MonoBehaviour
	{		
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
				this.onFactionChange?.Invoke();
			}
		}

		public UnityEvent onFactionChange = new UnityEvent();
	}
}