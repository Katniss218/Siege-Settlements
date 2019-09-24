﻿using SS.Buildings;
using SS.Content;
using SS.Levels.SaveStates;
using SS.ResourceSystem.Payment;
using SS.UI;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class BarracksModule : Module, IPaymentReceiver
	{
		private BarracksModuleDefinition def;

		/// <summary>
		/// Contains every unit that can be created in the barracks.
		/// </summary>
		public UnitDefinition[] trainableUnits { get; set; }

		/// <summary>
		/// Contains the currently trained unit (Read only).
		/// </summary>
		public UnitDefinition trainedUnit { get; private set; }

		/// <summary>
		/// returns true if the barracks module is currently training a unit (after payment has been completed).
		/// </summary>
		public bool isTraining
		{
			get
			{
				return this.trainedUnit != null;
			}
		}

		/// <summary>
		/// Contains the progress of the training (Read Only).
		/// </summary>
		public float trainProgress { get; private set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float trainSpeed { get; set; }

		/// <summary>
		/// Contains the local-space position, where the units are created.
		/// </summary>
		public Vector3 spawnPosition { get; private set; }

		/// <summary>
		/// Contains the local-space position, the units move towards, after creation.
		/// </summary>
		public Vector3 rallyPoint { get; set; }

		private FactionMember factionMember;

		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();

		
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private bool IsPaymentDone()
		{
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value > 0 )
				{
					return false;
				}
			}
			return true;
		}



		public void ReceivePayment( string id, int amount )
		{
			if( this.resourcesRemaining.ContainsKey( id ) )
			{
				this.resourcesRemaining[id] -= amount;
				if( this.resourcesRemaining[id] < 0 )
				{
					this.resourcesRemaining[id] = 0;
				}
			}
		}

		public Dictionary<string, int> GetWantedResources()
		{
			return this.resourcesRemaining;
		}
		
		private void StartTraining( UnitDefinition def )
		{
			if( this.isTraining )
			{
				throw new Exception( "There is already unit being trained." );
			}

			this.trainedUnit = def;
			this.trainProgress = def.buildTime;

			this.resourcesRemaining = new Dictionary<string, int>( this.trainedUnit.cost );
			/*foreach( var kvp in this.trainedUnit.cost )
			{
				this.resourcesRemaining.Add( kvp.Key, kvp.Value );
			}*/
		}

		// Start is called before the first frame update
		void Start()
		{
			this.factionMember = this.GetComponent<FactionMember>();

			
		}

		// Update is called once per frame
		void Update()
		{
			// If we are building something
			if( this.isTraining )
			{
				Selectable selectable = this.GetComponent<Selectable>();

				this.trainProgress -= this.trainSpeed * Time.deltaTime;
				if( this.trainProgress <= 0 )
				{
					// Calculate world-space spawn position.
					Matrix4x4 toWorld = this.transform.localToWorldMatrix;
					Vector3 spawnPos = toWorld.MultiplyVector( this.spawnPosition ) + this.transform.position;

					// Calculate world-space spawn rotation.
					Quaternion spawnRot = Quaternion.Euler( this.transform.position - spawnPos );

					RaycastHit hitInfo;
					if( Physics.Raycast( new Ray( spawnPos + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down ), out hitInfo, 100f, ObjectLayer.TERRAIN_MASK ) )
					{
						UnitData data = new UnitData();
						data.position = hitInfo.point;
						data.rotation = spawnRot;
						data.factionId = this.factionMember.factionId;
						data.health = this.trainedUnit.healthMax;

						GameObject obj = UnitCreator.Create( this.trainedUnit, data );
						// Move the newly spawned unit to the rally position.
						Vector3 rallyPointWorld = toWorld.MultiplyVector( this.rallyPoint ) + this.transform.position;
						TAIGoal.MoveTo.AssignTAIGoal( obj, rallyPointWorld );
					}
					else
					{
						Debug.LogWarning( "No suitable position for spawning was found." );
					}
					
					this.trainedUnit = null;
				}

				// Force the SelectionPanel.Object UI to update and show that we either have researched the tech, ot that the progress progressed.
				if( selectable != null )
				{
					Selection.ForceSelectionUIRedraw( selectable );
				}
			}
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		/// <summary>
		/// Creates a new BarracksModuleSaveState from a GameObject.
		/// </summary>
		/// <param name="unit">The GameObject to extract the save state from.</param>
		public BarracksModuleSaveState GetSaveState()
		{
			BarracksModuleSaveState saveState = new BarracksModuleSaveState();
			saveState.def = this.def;

			saveState.resourcesRemaining = this.resourcesRemaining;
			saveState.trainedUnit = this.trainedUnit;
			saveState.trainProgress = this.trainProgress;

			return saveState;
		}

		/// <summary>
		/// Adds this BarracksModuleSaveState to the specified GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to apply the BarracksModule to.</param>
		public void SetSaveState( BarracksModuleSaveState saveState )
		{
			this.trainSpeed = saveState.def.trainSpeed;
			this.trainableUnits = new UnitDefinition[saveState.def.trainableUnits.Length];
			for( int i = 0; i < this.trainableUnits.Length; i++ )
			{
				this.trainableUnits[i] = DefinitionManager.Get<UnitDefinition>( saveState.def.trainableUnits[i] );
			}

			Building building = this.GetComponent<Building>();
			if( building == null )
			{
				this.spawnPosition = Vector3.zero;
			}
			else
			{
				this.spawnPosition = building.entrance;
			}

			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				//####
				// Assign the UI redraw pass.
				//####
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

					if( this.IsPaymentDone() )
					{
						if( this.isTraining )
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Training...: '" + this.trainedUnit.displayName + "' - " + (int)this.trainProgress + " s." );
						}

						else
						{
							GameObject[] gridElements = new GameObject[this.trainableUnits.Length];
							// Initialize the grid elements' GameObjects.
							for( int i = 0; i < this.trainableUnits.Length; i++ )
							{
								UnitDefinition unitDef = this.trainableUnits[i];
								// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
								if( Technologies.TechLock.CheckLocked( unitDef, FactionManager.factions[FactionManager.PLAYER].techs ) )
								{
									gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, null );
								}
								else
								{
									gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon.Item2, () =>
									{
										this.StartTraining( unitDef );
										// Force the Object UI to update and show that now we are training a unit.
										Selection.ForceSelectionUIRedraw( selectable );
									} );
								}
							}

							UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select unit to make..." );
						}
					}
					else
					{
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.trainedUnit.displayName + "'." );
					}
				} );
			}

			this.trainedUnit = saveState.trainedUnit;
			this.trainProgress = saveState.trainProgress;
			this.rallyPoint = saveState.rallyPoint;
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