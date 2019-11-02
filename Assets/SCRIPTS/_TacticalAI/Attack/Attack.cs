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
			public GameObject target { get; private set; }

			private Damageable targetDamageable = null;


			private NavMeshAgent navMeshAgent = null;
			private ITargetFinder[] targeters = null;
			
			private float attackDistance = 0.0f;
			private float maxSearchRange = 0.0f;

			void Start()
			{
				this.navMeshAgent = this.GetComponent<NavMeshAgent>();
				this.targeters = this.GetComponents<ITargetFinder>();
				for( int i = 0; i < this.targeters.Length; i++ )
				{
					if( this.maxSearchRange >= this.targeters[i].searchRange )
					{
						continue;
					}
					this.maxSearchRange = this.targeters[i].searchRange;
				}
				this.attackDistance = this.maxSearchRange / 2.0f;

				if( this.navMeshAgent == null )
				{
					throw new System.Exception( "Can't add Attack TAI goal to: " + this.gameObject.name );
				}
				if( this.targeters == null || this.targeters.Length == 0 )
				{
					throw new System.Exception( "Can't add Attack TAI goal to: " + this.gameObject.name );
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

				if( Vector3.Distance( this.transform.position, this.target.transform.position ) <= this.maxSearchRange )
				{
					for( int i = 0; i < this.targeters.Length; i++ )
					{
						this.targeters[i].TrySetTarget( this.targetDamageable );
					}
				}
				this.navMeshAgent.SetDestination( this.target.transform.position );
			}
			
			void Update()
			{
				if( this.target == null )
				{
					this.navMeshAgent.ResetPath();
					Object.Destroy( this );
					return;
				}
				if( Vector3.Distance( this.transform.position, this.target.transform.position ) <= this.attackDistance )
				{
					for( int i = 0; i < this.targeters.Length; i++ )
					{
						this.targeters[i].TrySetTarget( this.targetDamageable );
					}
					this.navMeshAgent.ResetPath();
				}
				else
				{
					if( this.navMeshAgent.hasPath )
					{
						return;
					}
					this.navMeshAgent.SetDestination( this.target.transform.position );
				}
			}


			public override TAIGoalData GetData()
			{
				AttackData data = new AttackData();

				data.targetGuid = Main.GetGuid( this.target );

				return data;
			}

			/// <summary>
			/// Assigns a new PickupDeposit TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, GameObject target )
			{
				TAIGoal.ClearGoal( gameObject );

				Attack attack = gameObject.AddComponent<TAIGoal.Attack>();

				attack.target = target;
			}
		}
	}
}