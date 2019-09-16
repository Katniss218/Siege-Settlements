using SS.Buildings;
using SS.Data;
using SS.ResourceSystem.Payment;
using SS.UI;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class BarracksModule : Module, IPaymentProgress
	{
		/// <summary>
		/// Contains every unit that can be created in the barracks.
		/// </summary>
		public UnitDefinition[] spawnableUnits { get; set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float constructionSpeed { get; set; }

		public Func<bool> IsDone { get; private set; }

		public Func<Vector3> GetSpawnPosition { get; private set; }

		public Vector3 rallyPoint { get; set; }

		private UnitDefinition buildingUnit;
		private float buildTime;

		public bool isTraining
		{
			get
			{
				return this.buildingUnit != null;
			}
		}

		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();

		private void StartTraining( UnitDefinition def )
		{
			if( this.isTraining )
			{
				Debug.LogWarning( "There is already unit being trained." );
				return;
			}
			this.buildingUnit = def;
			this.buildTime = def.buildTime;

			this.resourcesRemaining.Clear();
			foreach( var kvp in this.buildingUnit.cost )
			{
				this.resourcesRemaining.Add( kvp.Key, kvp.Value );
			}

			AwaitForPayment();

		}

		private void AwaitForPayment()
		{
			if( this.GetComponent<PaymentReceiver>() == null )
			{
				PaymentReceiver paymentReceiver = this.gameObject.AddComponent<PaymentReceiver>();
				paymentReceiver.paymentProgress = this;
				paymentReceiver.onPaymentMade.AddListener( ( string id, int amount ) =>
				{
					if( this.resourcesRemaining.ContainsKey( id ) )
					{
						this.resourcesRemaining[id] -= amount;
						if( this.resourcesRemaining[id] < 0 )
						{
							this.resourcesRemaining[id] = 0;
						}
					}
				} );
			}
		}

		public int GetWantedAmount( string resourceId )
		{
			int value = 0;
			return resourcesRemaining.TryGetValue( resourceId, out value ) ? value : 0;
		}
		
		// Start is called before the first frame update
		void Start()
		{
			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				selectable.onSelectionUIRedraw.AddListener( () =>
				{
					Damageable d = this.GetComponent<Damageable>();
					// If the barracks are not usable.
					if( selectable.gameObject.layer == LayerMask.NameToLayer( "Buildings" ) && d != null && !Buildings.Building.CheckUsable( d ) )
					{
						return;
					}

					// Create the actual UI
					if( this.isTraining )
					{
						if( this.GetComponent<PaymentReceiver>() != null )
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.buildingUnit.displayName + "'." );
						}
						else
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Training...: '" + this.buildingUnit.displayName + "' - " + (int)this.buildTime + " s." );
						}
					}
					else
					{
						GameObject[] gridElements = new GameObject[spawnableUnits.Length];
						// Initialize the grid elements' GameObjects.
						for( int i = 0; i < spawnableUnits.Length; i++ )
						{
							UnitDefinition unitDef = this.spawnableUnits[i];
							// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
							if( Technologies.TechLock.CheckLocked( unitDef, FactionManager.factions[0].techs ) )
							{
								gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, null );
							}
							else
							{
								gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, () =>
								{
									StartTraining( unitDef );
									// Force the Object UI to update and show that now we are training a unit.
									SelectionManager.ForceSelectionUIRedraw( selectable );
								} );
							}
						}

						UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select unit to make..." );
					}
				} );
			}
		}

		// Update is called once per frame
		void Update()
		{
			// If we are building something
			if( this.isTraining )
			{
				Selectable selectable = this.GetComponent<Selectable>();

				if( this.GetComponent<PaymentReceiver>() == null )
				{
					this.buildTime -= this.constructionSpeed * Time.deltaTime;
					if( this.buildTime <= 0 )
					{
						// Calculate world-space spawn position.
						Matrix4x4 toWorld = this.transform.localToWorldMatrix;
						Vector3 spawnPos = toWorld.MultiplyVector( this.GetSpawnPosition() ) + this.transform.position;

						// Calculate world-space spawn rotation.
						Quaternion spawnRot = Quaternion.Euler( this.transform.position - spawnPos );

						RaycastHit hitInfo;
						GameObject obj;
						if( Physics.Raycast( new Ray( spawnPos + new Vector3( 0, 50, 0 ), Vector3.down ), out hitInfo, 100f, 1 << LayerMask.NameToLayer( "Terrain" ) ) )//( new Ray( spawnPos, Vector3.down ), out hitInfo, 100f, 1 < LayerMask.NameToLayer( "Terrain" ) ) )
						{
							obj = Unit.Create( this.buildingUnit, hitInfo.point, spawnRot, this.GetComponent<FactionMember>().factionId );
						}
						else
						{
							// FIXME ----- add method for raycasting top-down
							// FIXME ----- add method for getting layers of the specified object type (unit/bldg/terrain/etc)
							// convert vector to ray
							// default ray length
							// layer of the terrain.

							Debug.LogWarning( "No suitable position for spawning was found." );
							
							obj = Unit.Create( this.buildingUnit, hitInfo.point, spawnRot, this.GetComponent<FactionMember>().factionId );
						}

						// Move the newly spawned unit to the rally position.
						Vector3 rallyPos = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;
						TAIGoal.MoveTo.AssignTAIGoal( obj, rallyPos );

						this.buildingUnit = null;
					}
					
					// Force the SelectionPanel.Object UI to update and show that we either have researched the tech, ot that the progress progressed.
					if( selectable != null )
					{
						SelectionManager.ForceSelectionUIRedraw( selectable );
					}
				}
			}
		}

		public static void AddTo( GameObject obj, BarracksModuleDefinition def )
		{
			BarracksModule barracks = obj.AddComponent<BarracksModule>();
			barracks.constructionSpeed = def.constructionSpeed;
			barracks.spawnableUnits = new UnitDefinition[def.spawnableUnits.Length];
			for( int i = 0; i < barracks.spawnableUnits.Length; i++ )
			{
				barracks.spawnableUnits[i] = DataManager.Get<UnitDefinition>( def.spawnableUnits[i] );
			}

			Building building = obj.GetComponent<Building>();
			if( building == null )
			{
				barracks.GetSpawnPosition = () =>
				{
					return Vector3.zero;
				};
			}
			else
			{
				barracks.GetSpawnPosition = () =>
				{
					return building.entrance;
				};
			}

			// Set the method for checking progress of the construction.
			barracks.IsDone = () =>
			{
				foreach( var kvp in barracks.resourcesRemaining )
				{
					if( kvp.Value != 0 )
					{
						return false;
					}
				}
				return true;
			};

			//paymentReceiver.onProgressComplete.AddListener( () =>
			//{
			//	
			//} );
		}

#if UNITY_EDITOR

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			Vector3 spawnPos = toWorld.MultiplyVector( this.GetSpawnPosition() ) + this.transform.position;

			Gizmos.DrawSphere( spawnPos, 0.1f );

			Vector3 rallyPos = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere( rallyPos, 0.15f );
		}

#endif
	}
}