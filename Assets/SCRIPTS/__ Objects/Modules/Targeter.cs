using SS.Diplomacy;
using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class Targeter
	{
		public enum TargetingMode : byte
		{
			ARBITRARY,
			CLOSEST,
			TARGET
		}

#warning Do I even want that? The Tactical Goals are targeters.
		private Damageable __target;
		public Damageable target
		{
			get
			{
				return this.__target;
			}
			set
			{
				this.__target = value;
				if( value == null )
				{
					onTargetReset?.Invoke();
				}
				else
				{
					onTargetSet?.Invoke();
				}
			}
		}
		
		public int layers { get; private set; }


		public Targeter( int layers, FactionMember factionMember )
		{
			this.layers = layers;
			this.factionMember = factionMember;
		}

		public event Action onTargetSet = null;
		public event Action onTargetReset = null;

		public FactionMember factionMember { get; private set; }
		

		public static bool CanTarget( Vector3 positionSelf, float searchRange, Damageable target, FactionMember factionMemberSelf )
		{
			if( target == null )
			{
				return false;
			}

			if( !factionMemberSelf.CanTargetAnother( target.GetComponent<FactionMember>() ) )
			{
				return false;
			}

			if( Vector3.Distance( target.transform.position, positionSelf ) > searchRange )
			{
				return false;
			}

			return true;
		}

		public Damageable TrySetTarget( Vector3 positionSelf, float searchRange, TargetingMode targetingMode, Damageable target = null )
		{
			if( targetingMode == TargetingMode.ARBITRARY )
			{
				this.target = FindTargetArbitrary( positionSelf, searchRange, this.layers, this.factionMember );
			}
			else if( targetingMode == TargetingMode.CLOSEST )
			{
				this.target = FindTargetClosest( positionSelf, searchRange, this.layers, this.factionMember );
			}
			else if( targetingMode == TargetingMode.TARGET )
			{
				if( target == null )
				{
					this.target = null;
					return null;
				}
				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( target.GetComponent<FactionMember>() ) )
				{
					return this.target;
				}

				if( !Main.IsInRange( target.transform.position, positionSelf, searchRange ) )
				{
					return this.target;
				}
				
				this.target = target;
			}
			return this.target;
		}

		public Damageable TrySetTarget( Vector3 positionSelf, float searchRange, Damageable target )
		{
			if( CanTarget( positionSelf, searchRange, target, this.factionMember ) )
			{
				this.target = target;
			}
			return this.target;
		}
		
		public static Damageable FindTargetArbitrary( Vector3 positionSelf, float searchRange, int layerMask, FactionMember factionMemberSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, layerMask );
			if( col.Length == 0 )
			{
				return null;
			}
			
			for( int i = 0; i < col.Length; i++ )
			{
				FactionMember facOther = col[i].GetComponent<FactionMember>();

				// Check if the overlapped object can be targeted by this finder.
				if( !factionMemberSelf.CanTargetAnother( facOther ) )
				{
					continue;
				}

				if( !Main.IsInRange( col[i].transform.position, positionSelf, searchRange ) )
				{
					continue;
				}

				return col[i].GetComponent<Damageable>();
			}
			return null;
		}

		public static Damageable FindTargetClosest( Vector3 positionSelf, float searchRange, int layerMask, FactionMember factionMemberSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, layerMask );
			if( col.Length == 0 )
			{
				return null;
			}
			Damageable ret = null;
			float needThisCloseSq = searchRange * searchRange;
			float needThisClose = searchRange;

			for( int i = 0; i < col.Length; i++ )
			{
				FactionMember facOther = col[i].GetComponent<FactionMember>();

				// Check if the overlapped object can be targeted by this finder.
				if( !factionMemberSelf.CanTargetAnother( facOther ) )
				{
					continue;
				}

				float distSq = (col[i].transform.position - positionSelf).sqrMagnitude;
				//if( !Main.IsInRange( col[i].transform.position, positionSelf, needThisClose ) )
				if( distSq >= needThisCloseSq )
				{
					continue;
				}

				needThisCloseSq = distSq;
				ret = col[i].GetComponent<Damageable>();
			}
			return ret;
		}
	}
}