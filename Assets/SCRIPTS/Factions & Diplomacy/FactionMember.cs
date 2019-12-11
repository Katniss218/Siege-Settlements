using SS.Levels;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Diplomacy
{
	[DisallowMultipleComponent]
	/// <summary>
	/// The object that belongs to a faction.
	/// </summary>
	public sealed class FactionMember : MonoBehaviour
	{
#warning This doesn't have to be a MonoBehaviour, really.
		[SerializeField] private int __factionId;
		/// <summary>
		/// Contains the index of the faction that this object belongs to.
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

		public float viewRange;

		// Checks if the faction members can target each other.
		// The condition is: --- Fac1 can target Fac2 IF: Fac1 or Fac2 is nor present, or the Fac1 belongs to different faction than Fac2.
		internal bool CanTargetAnother( FactionMember fac2 )
		{
			if( fac2 == null )
			{
				return true;
			}
			if( this.factionId == fac2.factionId )
			{
				return false;
			}
			return LevelDataManager.GetRelation( this.factionId, fac2.factionId ) == DiplomaticRelation.Enemy;
		}

		internal static bool CanTarget( int fac1, FactionMember fac2 )
		{
			if( fac1 < 0 || fac2 == null )
			{
				return true;
			}
			if( fac1 == fac2.factionId )
			{
				return false;
			}
			return LevelDataManager.GetRelation( fac1, fac2.factionId ) == DiplomaticRelation.Enemy;
		}
#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere( this.transform.position, this.viewRange );
		}
#endif
	}
}