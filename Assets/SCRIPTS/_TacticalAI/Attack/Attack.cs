using SS.Objects;
using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class Attack : TAIGoal
		{
			/// <summary>
			/// The object to attack.
			/// </summary>
			public SSObject target { get; private set; }

			private Damageable targetDamageable = null;


			private NavMeshAgent navMeshAgent = null;
			private ITargeterModule[] targeters = null;
			
			private float attackDistance = 0.0f;
			private float maxSearchRange = 0.0f;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.targeters = this.GetComponents<ITargeterModule>();
				for( int i = 0; i < this.targeters.Length; i++ )
				{
					if( this.maxSearchRange >= this.targeters[i].searchRange )
					{
						continue;
					}
					this.maxSearchRange = this.targeters[i].searchRange;
				}
				this.attackDistance = this.maxSearchRange / 2.0f;

				if( this.targeters == null || this.targeters.Length == 0 )
				{
					throw new System.Exception( "Can't add Attack TAI goal to: " + this.gameObject.name ); // nothing to get the target from.
				}
				if( this.target == null )
				{
					Debug.LogWarning( "Not assigned target to: " + this.gameObject.name );
					Object.Destroy( this );
				}
				else
				{
					targetDamageable = this.target.GetComponent<Damageable>();
					if( this.targetDamageable  == null )
					{
						Debug.LogWarning( "Can't assign target to: " + this.gameObject.name + " - no Damageable found." );
						Object.Destroy( this );
					}
				}

				bool hasAcquiredTarget = false;
				if( Vector3.Distance( this.transform.position, this.target.transform.position ) <= this.maxSearchRange )
				{
					for( int i = 0; i < this.targeters.Length; i++ )
					{
						bool f = this.targeters[i].TrySetTarget( this.targetDamageable );
						if( !hasAcquiredTarget )
						{
							hasAcquiredTarget = f;
						}
					}
				}
				// Only move if none of the present targeters can target the specified unit.
				if( !hasAcquiredTarget )
				{
					if( this.navMeshAgent != null )
					{
						this.navMeshAgent.SetDestination( this.target.transform.position );
					}
				}
			}
			
			void Update()
			{
				if( this.target == null )
				{
					if( this.navMeshAgent != null )
					{
						this.navMeshAgent.ResetPath();
					}
					Object.Destroy( this );
					return;
				}
				if( Vector3.Distance( this.transform.position, this.target.transform.position ) <= this.attackDistance )
				{
					for( int i = 0; i < this.targeters.Length; i++ )
					{
						this.targeters[i].TrySetTarget( this.targetDamageable );
					}

					if( this.navMeshAgent != null )
					{
						this.navMeshAgent.ResetPath();
					}
				}
				else
				{
					if( this.navMeshAgent != null )
					{
						if( this.navMeshAgent.hasPath )
						{
							return;
						}

						this.navMeshAgent.SetDestination( this.target.transform.position );
					}
				}
			}


			public override TAIGoalData GetData()
			{
				AttackData data = new AttackData();

				data.targetGuid = this.target.guid.Value;

				return data;
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, SSObject target )
			{
				TAIGoal.ClearGoal( gameObject );

				Attack attack = gameObject.AddComponent<TAIGoal.Attack>();

				attack.target = target;
			}
		}
	}
}