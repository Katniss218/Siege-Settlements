using SS.Buildings;
using SS.Content;
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
		public UnitDefinition[] trainableUnits { get; set; }

		/// <summary>
		/// Contains the currently trained unit (Read only).
		/// </summary>
		public UnitDefinition trainedUnit { get; private set; }
		
		/// <summary>
		/// Contains the progress of the training (Read Only).
		/// </summary>
		public float trainProgress { get; private set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float trainSpeed { get; set; }

		/// <summary>
		/// returns true if the barracks module is currently training a unit.
		/// </summary>
		public bool isTraining
		{
			get
			{
				return this.trainedUnit != null;
			}
		}

		/// <summary>
		/// Contains the local-space position, where the units are created.
		/// </summary>
		public Vector3 spawnPosition { get; private set; }

		/// <summary>
		/// Contains the local-space position, the units move towards, after creation.
		/// </summary>
		public Vector3 rallyPoint { get; set; }


		public Func<bool> IsDone { get; private set; }
		
		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();


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

		private void StartTraining( UnitDefinition def )
		{
			if( this.isTraining )
			{
				Debug.LogWarning( "There is already unit being trained." );
				return;
			}
			this.trainedUnit = def;
			this.trainProgress = def.buildTime;

			this.resourcesRemaining.Clear();
			foreach( var kvp in this.trainedUnit.cost )
			{
				this.resourcesRemaining.Add( kvp.Key, kvp.Value );
			}

			AwaitForPayment();

		}

		// Start is called before the first frame update
		void Start()
		{
			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				selectable.onSelectionUIRedraw.AddListener( () =>
				{
					// If the barracks are on a building, that is not usable.
					if( selectable.gameObject.layer == ObjectLayer.BUILDINGS )
					{
						Damageable damageable = this.GetComponent<Damageable>();

						if( damageable != null )
						{
							if( !Building.IsUsable( damageable ) )
							{
								return;
							}
						}
					}

					// Create the actual UI
					if( this.isTraining )
					{
						if( this.GetComponent<PaymentReceiver>() != null )
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.trainedUnit.displayName + "'." );
						}
						else
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Training...: '" + this.trainedUnit.displayName + "' - " + (int)this.trainProgress + " s." );
						}
					}
					else
					{
						GameObject[] gridElements = new GameObject[trainableUnits.Length];
						// Initialize the grid elements' GameObjects.
						for( int i = 0; i < trainableUnits.Length; i++ )
						{
							UnitDefinition unitDef = this.trainableUnits[i];
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
					this.trainProgress -= this.trainSpeed * Time.deltaTime;
					if( this.trainProgress <= 0 )
					{
						// Calculate world-space spawn position.
						Matrix4x4 toWorld = this.transform.localToWorldMatrix;
						Vector3 spawnPos = toWorld.MultiplyVector( this.spawnPosition ) + this.transform.position;

						// Calculate world-space spawn rotation.
						Quaternion spawnRot = Quaternion.Euler( this.transform.position - spawnPos );

						RaycastHit hitInfo;
						GameObject obj;
						if( Physics.Raycast( new Ray( spawnPos + new Vector3( 0, 50, 0 ), Vector3.down ), out hitInfo, 100f, ObjectLayer.TERRAIN_MASK ) )
						{
							obj = UnitCreator.Create( this.trainedUnit, hitInfo.point, spawnRot, this.GetComponent<FactionMember>().factionId );
						}
						else
						{
							Debug.LogWarning( "No suitable position for spawning was found." );
							
							obj = UnitCreator.Create( this.trainedUnit, hitInfo.point, spawnRot, this.GetComponent<FactionMember>().factionId );
						}

						// Move the newly spawned unit to the rally position.
						Vector3 rallyPos = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;
						TAIGoal.MoveTo.AssignTAIGoal( obj, rallyPos );

						this.trainedUnit = null;
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
			barracks.trainSpeed = def.constructionSpeed;
			barracks.trainableUnits = new UnitDefinition[def.spawnableUnits.Length];
			for( int i = 0; i < barracks.trainableUnits.Length; i++ )
			{
				barracks.trainableUnits[i] = DataManager.Get<UnitDefinition>( def.spawnableUnits[i] );
			}

			Building building = obj.GetComponent<Building>();
			if( building == null )
			{
				barracks.spawnPosition = Vector3.zero;
			}
			else
			{
				barracks.spawnPosition = building.entrance;
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
			Vector3 spawnPos = toWorld.MultiplyVector( this.spawnPosition ) + this.transform.position;

			Gizmos.DrawSphere( spawnPos, 0.1f );

			Vector3 rallyPos = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere( rallyPos, 0.15f );
			Gizmos.DrawLine( spawnPos, rallyPos );
		}
#endif
	}
}