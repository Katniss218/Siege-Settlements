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
		private SSObjectDFS __target;
		public SSObjectDFS target
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


		public Targeter( int layers, SSObjectDFS factionMember )
		{
			this.layers = layers;
			this.factionMember = factionMember;
		}

		public event Action onTargetSet = null;
		public event Action onTargetReset = null;

		public SSObjectDFS factionMember { get; private set; }
		

		public static bool CanTarget( Vector3 positionSelf, float searchRange, SSObjectDFS target, SSObjectDFS factionMemberSelf )
		{
			if( target == null )
			{
				return false;
			}

			if( !factionMemberSelf.CanTargetAnother( target.GetComponent<IFactionMember>() ) )
			{
				return false;
			}

			if( Vector3.Distance( target.transform.position, positionSelf ) > searchRange )
			{
				return false;
			}

			return true;
		}

		public SSObjectDFS TrySetTarget( Vector3 positionSelf, float searchRange, TargetingMode targetingMode, SSObjectDFS target = null )
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
				if( !this.factionMember.CanTargetAnother( target ) )
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

		public SSObjectDFS TrySetTarget( Vector3 positionSelf, float searchRange, SSObjectDFS target )
		{
			if( CanTarget( positionSelf, searchRange, target, this.factionMember ) )
			{
				this.target = target;
			}
			return this.target;
		}
		
		public static SSObjectDFS FindTargetArbitrary( Vector3 positionSelf, float searchRange, int layerMask, SSObjectDFS factionMemberSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, layerMask );
			if( col.Length == 0 )
			{
				return null;
			}
			
			for( int i = 0; i < col.Length; i++ )
			{
				SSObjectDFS facOther = col[i].GetComponent<SSObjectDFS>();

				// Check if the overlapped object can be targeted by this finder.
				if( !factionMemberSelf.CanTargetAnother( facOther ) )
				{
					continue;
				}

				if( !Main.IsInRange( col[i].transform.position, positionSelf, searchRange ) )
				{
					continue;
				}

				return facOther;
			}
			return null;
		}

		public static SSObjectDFS FindTargetClosest( Vector3 positionSelf, float searchRange, int layerMask, SSObjectDFS factionMemberSelf )
		{
			Collider[] col = Physics.OverlapSphere( positionSelf, searchRange, layerMask );
			if( col.Length == 0 )
			{
				return null;
			}
			SSObjectDFS ret = null;
			float needThisCloseSq = searchRange * searchRange;
			float needThisClose = searchRange;

			for( int i = 0; i < col.Length; i++ )
			{
				SSObjectDFS facOther = col[i].GetComponent<SSObjectDFS>();

				// Check if the overlapped object can be targeted by this finder.
				if( !factionMemberSelf.CanTargetAnother( facOther ) )
				{
					continue;
				}

				float distSq = (col[i].transform.position - positionSelf).sqrMagnitude;
				if( distSq >= needThisCloseSq )
				{
					continue;
				}

				needThisCloseSq = distSq;
				ret = facOther;
			}
			return ret;
		}
	}
}