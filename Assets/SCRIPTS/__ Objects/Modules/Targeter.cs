using SS.Diplomacy;
using System;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class Targeter
	{
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

		public Damageable TrySetTarget( Vector3 positionSelf )
		{
			this.target = this.FindTarget( positionSelf );
			return this.target;
		}

		public Damageable TrySetTarget( Vector3 positionSelf, Damageable target )
		{
			if( CanTarget( this.factionMember, target, positionSelf, this.searchRange ) )
			{
				this.target = target;
			}
			return this.target;
		}
		
		private Damageable FindTarget( Vector3 positionSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, this.searchRange, this.layers );
			if( col.Length == 0 )
			{
				return null;
			}
			
			for( int i = 0; i < col.Length; i++ )
			{
				SSObject ssObject = col[i].GetComponent<SSObject>();

				// Check if the overlapped object can be targeted by this finder.
				if( !this.factionMember.CanTargetAnother( (ssObject as IFactionMember).factionMember ) )
				{
					continue;
				}

				if( !Main.IsInRange( col[i].transform.position, positionSelf, searchRange ) )
				{
					continue;
				}

				return (ssObject as IDamageable).damageable;
			}
			return null;
		}
	}
}