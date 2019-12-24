using SS.Diplomacy;
using SS.Levels;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Objects
{
	public class _UnityEvent_float : UnityEvent<float> { }
	public class _UnityEvent_SSObjectDFS_float : UnityEvent<SSObjectDFS, float> { }
	public class _UnityEvent_SSObjectDFS : UnityEvent<SSObjectDFS> { }

	/// <summary>
	/// Add this to any object to make it selectable by the player.
	/// </summary>
	public abstract class SSObjectDFS : SSObject, ISelectDisplayHandler, IDamageable, IFactionMember
	{
		/// <summary>
		/// The icon that is shown on the list of all selected objects.
		/// </summary>
		public Sprite icon;

		/// <summary>
		/// Is called when the object gets selected.
		/// </summary>
		public UnityEvent onSelect = new UnityEvent();

		/// <summary>
		/// Is called when the object gets highlighted.
		/// </summary>
		public UnityEvent onHighlight = new UnityEvent();

		/// <summary>
		/// Is called when the object gets deselected.
		/// </summary>
		public UnityEvent onDeselect = new UnityEvent();


		//
		//
		//


		public float viewRange { get; set; }


		[SerializeField] private int __factionId = -1; // Needs to be different then any index of the factions array (otherwise onFactionChange won't trigger on spawn).
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
				bool isDifferentFactionId = this.factionId != value;
				this.__factionId = value;

				if( isDifferentFactionId ) // Only call onFactionChange if the faction ID has actually changed.
				{
					this.onFactionChange?.Invoke();
				}
			}
		}

		/// <summary>
		/// Fired when the faction ID changes.
		/// </summary>
		public UnityEvent onFactionChange { get; set; } = new UnityEvent();

		// Checks if the faction members can target each other.
		// The condition is: --- Fac1 can target Fac2 IF: Fac1 or Fac2 is nor present, or the Fac1 belongs to different faction than Fac2.
		internal bool CanTargetAnother( IFactionMember fac2 )
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

		internal static bool CanTarget( int fac1, IFactionMember fac2 )
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

		/// <summary>
		/// Returns true if the object should display it's parameters on the Selection Panel. True if the faction id matches player's faction.
		/// </summary>
		public override bool IsDisplaySafe()
		{
			return this.factionId == LevelDataManager.PLAYER_FAC;
		}


		//
		//
		//


		public UnityEvent onHealthPercentChanged { get; set; } = new UnityEvent();

		/// <summary>
		/// Fires when the 'health' value is changed.
		/// </summary>
		public _UnityEvent_float onHealthChange { get; set; } = new _UnityEvent_float();
		public static _UnityEvent_SSObjectDFS_float onHealthChangeAny = new _UnityEvent_SSObjectDFS_float();

		/// <summary>
		/// Fires when the damageable is killed ('health' value is less or equal to 0, or by using Die()).
		/// </summary>
		public UnityEvent onDeath { get; set; } = new UnityEvent();
		public static _UnityEvent_SSObjectDFS onDeathAny = new _UnityEvent_SSObjectDFS();


		public float lastDamageTakenTimestamp { get; private set; }
		public float lastHealTimestamp { get; private set; }

		[SerializeField]
		private float __health;
		/// <summary>
		/// Gets or sets the health value. Calls 'onHealthChange'. If set to 0, the damageable will die, calling 'onDeath'.
		/// </summary>
		public float health
		{
			get
			{
				return this.__health;
			}
			set
			{
				if( value > this.healthMax )
				{
					Debug.LogWarning( "Tried setting the health to above max health." );
					value = this.healthMax;
				}
				// Make sure that health can't go below 0.
				if( value < 0 )
				{
					value = 0;
				}
				float diff = value - this.__health; // if new value is bigger, diff should be positive.
				this.__health = value;

				if( diff > 0 )
				{
					this.lastHealTimestamp = Time.time;
				}
				else if( diff < 0 )
				{
					this.lastDamageTakenTimestamp = Time.time;
				}

				this.onHealthChange?.Invoke( diff );
				this.onHealthPercentChanged?.Invoke();
				onHealthChangeAny?.Invoke( this, diff );

				// If the health is 0, kill the damageable.
				if( this.__health == 0 )
				{
					this.Die();
				}
			}
		}

		[SerializeField]
		private float __healthMax;
		/// <summary>
		/// Gets or sets the maximum health value of this damageable.
		/// </summary>
		public float healthMax
		{
			get
			{
				return this.__healthMax;
			}
			set
			{
				this.__healthMax = value;

				if( this.healthMax < this.health )
				{
					this.health = this.healthMax;
				}

				this.onHealthPercentChanged?.Invoke();
			}
		}

		/// <summary>
		/// Gets or sets the percentage of health of this damageable.
		/// </summary>
		public float healthPercent
		{
			get
			{
				return this.health / this.healthMax;
			}
			set
			{
				if( value < 0 )
				{
					value = 0;
				}
				if( value > 1 )
				{
					value = 1;
				}
				this.health = value * this.healthMax;
			}
		}

		/// <summary>
		/// The armor of the damageable.
		/// </summary>
		public Armor armor { get; set; }

		protected virtual void Awake()
		{
			this.lastDamageTakenTimestamp = 0.0f; // init to 0 in constructor.
			this.lastHealTimestamp = 0.0f; // init to 0 in constructor.
		}

		internal static string GetHealthDisplay( float health, float healthMax )
		{
			return "Health: " + (int)health + "/" + (int)healthMax;
		}

		/// <summary>
		/// makes the damageable take an amount of damage, using the scaling reduction formula.
		/// </summary>
		/// <param name="type">The type of damage taken.</param>
		/// <param name="amount">The raw amount of damage to take.</param>
		/// <param name="armorPenetration">The armor penetration of the attacker.</param>
		public virtual void TakeDamage( DamageType type, float amount, float armorPenetration )
		{
			if( amount <= 0 )
			{
				throw new ArgumentOutOfRangeException( "Damage inflicted must be greater than 0." );
			}

			float reducedDamage = this.armor.CalculateReducedDamage( type, amount, armorPenetration );

			this.health -= reducedDamage;
		}

		/// <summary>
		/// Makes this damageable die.
		/// </summary>
		public virtual void Die()
		{
			Destroy( this.gameObject );
			this.onDeath?.Invoke();
			onDeathAny?.Invoke( this );
		}

		public abstract void OnDisplay();
		public abstract void OnHide();

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere( this.transform.position, this.viewRange );
		}
#endif
	}
}