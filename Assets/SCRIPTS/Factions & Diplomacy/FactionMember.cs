using SS.Levels;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Diplomacy
{
	/// <summary>
	/// The object that belongs to a faction.
	/// </summary>
	public sealed class FactionMember : MonoBehaviour
	{		
		[SerializeField] private int __factionId;
		/// <summary>
		/// Contains the identifier of the faction that this object belongs to.
		/// </summary>
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

		/// <summary>
		/// Fired when the faction ID changes.
		/// </summary>
		public UnityEvent onFactionChange = new UnityEvent();

		// Checks if the faction members can target each other.
		// The condition is: --- Fac1 can target Fac2 IF: Fac1 or Fac2 is nor present, or the Fac1 belongs to different faction than Fac2.
		internal static bool CanTargetAnother( FactionMember fac1, FactionMember fac2 )
		{
			if( fac1 == null )
			{
				return true;
			}
			if( fac2 == null )
			{
				return true;
			}
			if( fac1.factionId == fac2.factionId )
			{
				return false;
			}
			return LevelDataManager.diplomaticRelations.Get( fac1.factionId, fac2.factionId ) == DiplomaticRelation.Enemy;
		}
	}
}