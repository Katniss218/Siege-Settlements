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
		
		public float searchRange { get; set; }
		public int layers { get; private set; }


		public Targeter( float searchRange, int layers, FactionMember factionMember )
		{
			this.searchRange = searchRange;
			this.layers = layers;
			this.factionMember = factionMember;
		}

		public event Action onTargetSet = null;
		public event Action onTargetReset = null;

		public FactionMember factionMember { get; private set; }
		

		public static bool CanTarget( FactionMember factionMemberSelf, Damageable target, Vector3 positionSelf, float searchRange )
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
		
		public void TrySetTarget( Vector3 positionSelf, TargetingMode targetingMode, Damageable target = null )
		{
			if( targetingMode == TargetingMode.ARBITRARY )
			{
				this.target = this.FindTargetArbitrary( positionSelf );
			}
			else if( targetingMode == TargetingMode.CLOSEST )
			{
				this.target = this.FindTargetClosest( positionSelf );
			}
			else if( targetingMode == TargetingMode.TARGET )
			{
				if( target == null )
				{
					return;
				}
				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( target.GetComponent<FactionMember>() ) )
				{
					return;
				}

				if( !Main.IsInRange( target.transform.position, positionSelf, this.searchRange ) )
				{
					return;
				}
				
				this.target = target;
			}
			return;
		}

		public Damageable TrySetTarget( Vector3 positionSelf, Damageable target )
		{
			if( CanTarget( this.factionMember, target, positionSelf, this.searchRange ) )
			{
				this.target = target;
			}
			return this.target;
		}
		
		private Damageable FindTargetArbitrary( Vector3 positionSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, this.searchRange, this.layers );
			if( col.Length == 0 )
			{
				return null;
			}
			
			for( int i = 0; i < col.Length; i++ )
			{
				FactionMember facOther = col[i].GetComponent<FactionMember>();

				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( facOther ) )
				{
					continue;
				}

				if( !Main.IsInRange( col[i].transform.position, positionSelf, this.searchRange ) )
				{
					continue;
				}

				IDamageable ssDamageable = col[i].GetComponent<IDamageable>();
				return ssDamageable.damageable;
			}
			return null;
		}

		private Damageable FindTargetClosest( Vector3 positionSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, this.searchRange, this.layers );
			if( col.Length == 0 )
			{
				return null;
			}
			Damageable ret = null;
			float needThisClose = this.searchRange;

			for( int i = 0; i < col.Length; i++ )
			{
				FactionMember facOther = col[i].GetComponent<FactionMember>();

				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( facOther ) )
				{
					continue;
				}

				if( !Main.IsInRange( col[i].transform.position, positionSelf, needThisClose ) )
				{
					continue;
				}

				needThisClose = Vector3.Distance( col[i].transform.position, positionSelf );
				IDamageable ssDamageable = col[i].GetComponent<IDamageable>();
				ret = ssDamageable.damageable;
			}
			return ret;
		}
	}
}