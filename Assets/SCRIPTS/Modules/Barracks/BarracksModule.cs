using SS.Buildings;
using SS.Content;
using SS.Diplomacy;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class BarracksModule : Module, IPaymentReceiver
	{
		public UnityEvent onTrainingBegin = new UnityEvent();

		public UnityEvent onTrainingProgress = new UnityEvent();

		public UnityEvent onTrainingEnd = new UnityEvent();
		
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

		private Selectable selectable;
		
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		private bool IsPaymentDone()
		{
			if( resourcesRemaining == null )
			{
				return true;
			}
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
			if( resourcesRemaining == null )
			{
				Debug.LogWarning( "The payment of " + amount + "x '" + id + "' was not needed." );
				return;
			}
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
			Dictionary<string, int> ret = new Dictionary<string, int>();
			if( this.resourcesRemaining == null )
			{
				return ret;
			}
			foreach( var kvp in this.resourcesRemaining )
			{
				if( kvp.Value > 0 )
				{
					ret.Add( kvp.Key, kvp.Value );
				}
			}
			return ret;
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
			this.onTrainingBegin?.Invoke();
		}

		// Start is called before the first frame update
		void Awake()
		{
			this.factionMember = this.GetComponent<FactionMember>();

			this.selectable = this.GetComponent<Selectable>();



			Building building = this.GetComponent<Building>();
			if( building == null )
			{
				this.spawnPosition = Vector3.zero;
			}
			else
			{
				if( building.entrance == null )
				{
					throw new Exception( "Can't assign barracks to a building with no entrance." );
				}
				this.spawnPosition = building.entrance.Value;
			}
		}
		
		// Update is called once per frame
		void Update()
		{
			if( IsPaymentDone() )
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
						Quaternion spawnRot = Quaternion.identity;

						RaycastHit hitInfo;
						if( Physics.Raycast( new Ray( spawnPos + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down ), out hitInfo, 100f, ObjectLayer.TERRAIN_MASK ) )
						{
							UnitData data = new UnitData();
							data.guid = Guid.NewGuid();
							data.position = hitInfo.point;
							data.rotation = spawnRot;
							data.factionId = this.factionMember.factionId;
							data.health = this.trainedUnit.healthMax;
#warning need to have "identity" data (can created from definition, when there's no other data).
							throw new Exception( "NOT IMPLEMENTED YET" );
							//data.inventoryData = new InventoryUnconstrainedData();

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
						this.trainProgress = 0.0f;
						this.resourcesRemaining = null;
						this.onTrainingEnd?.Invoke();
					}

					else
					{
						this.onTrainingProgress?.Invoke();
					}
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
		public override ModuleData GetData()
		{
			BarracksModuleData saveState = new BarracksModuleData();

			saveState.resourcesRemaining = this.resourcesRemaining;
			if( this.trainedUnit == null )
			{
				saveState.trainedUnitId = "";
			}
			else
			{
				saveState.trainedUnitId = this.trainedUnit.id;
			}
			saveState.trainProgress = this.trainProgress;

			return saveState;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is BarracksModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is BarracksModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}
			
			BarracksModuleDefinition def = (BarracksModuleDefinition)_def;
			BarracksModuleData data = (BarracksModuleData)_data;

			this.trainSpeed = def.trainSpeed;
			this.trainableUnits = new UnitDefinition[def.trainableUnits.Length];
			for( int i = 0; i < this.trainableUnits.Length; i++ )
			{
				this.trainableUnits[i] = DefinitionManager.GetUnit( def.trainableUnits[i] );
			}

			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				// if applied before, remove.
				selectable.onHighlight.RemoveListener( this.OnHighlight );
				// add.
				selectable.onHighlight.AddListener( this.OnHighlight );


				this.onTrainingBegin.RemoveListener( this.OnTrainingBegin );
				this.onTrainingBegin.AddListener( this.OnTrainingBegin );

				this.onTrainingProgress.RemoveListener( this.OnTrainingProgress );
				this.onTrainingProgress.AddListener( this.OnTrainingProgress );

				this.onTrainingEnd.RemoveListener( this.OnTrainingEnd );
				this.onTrainingEnd.AddListener( this.OnTrainingEnd );

				if( this.factionMember != null )
				{
					LevelDataManager.onTechStateChanged.AddListener( OnTechStateChanged );
				}
			}

			this.resourcesRemaining = data.resourcesRemaining;
			if( data.trainedUnitId == "" )
			{
				this.trainedUnit = null;
			}
			else
			{
				this.trainedUnit = DefinitionManager.GetUnit( data.trainedUnitId );
			}
			this.trainProgress = data.trainProgress;
			this.rallyPoint = data.rallyPoint;
		}
		
		void OnDestroy()
		{
			if( this.factionMember != null )
			{
				LevelDataManager.onTechStateChanged.RemoveListener( OnTechStateChanged );
			}
		}

		private void ShowList()
		{
			GameObject[] gridElements = new GameObject[this.trainableUnits.Length];
			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < this.trainableUnits.Length; i++ )
			{
				UnitDefinition unitDef = this.trainableUnits[i];
				// If the unit's techs required have not been researched yet, add unclickable button, otherwise, add normal button.
				if( Technologies.TechLock.CheckLocked( unitDef, LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].GetAllTechs() ) )
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, null );
				}
				else
				{
					gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), unitDef.icon, () =>
					{
						this.StartTraining( unitDef );
					} );
				}
			}

			GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "barracks.list", list.transform );
		}

		private void OnTechStateChanged( int factionId, string id, TechnologyResearchProgress newProgress )
		{
			if( factionId != this.factionMember.factionId )
			{
				return;
			}
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( !this.isTraining )
				{
					if( SelectionPanel.instance.obj.GetElement( "barracks.list" ) != null )
					{
						SelectionPanel.instance.obj.Clear( "barracks.list" );
					}
				}
			}
			this.ShowList();
		}

		private void OnTrainingBegin()
		{
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			if( SelectionPanel.instance.obj.GetElement( "barracks.list" ) != null )
			{
				SelectionPanel.instance.obj.Clear( "barracks.list" );
			}
			if( !this.IsPaymentDone() )
			{
				Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
				if( statusUI != null )
				{
					UIUtils.EditText( statusUI.gameObject, "Waiting for resources: '" + this.trainedUnit.displayName + "'." );
				}
			}
		}

		private void OnTrainingProgress()
		{
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}
			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Training...: '" + this.trainedUnit.displayName + "' - " + (int)this.trainProgress + " s." );
			}
		}

		private void OnTrainingEnd()
		{
			if( !Selection.IsHighlighted( this.selectable ) )
			{
				return;
			}

			Transform statusUI = SelectionPanel.instance.obj.GetElement( "barracks.status" );
			if( statusUI != null )
			{
				UIUtils.EditText( statusUI.gameObject, "Select unit to make..." );
			}

			this.ShowList();
		}

		private void OnHighlight()
		{
			// If the barracks are on a building, that is not usable.
			if( this.selectable.gameObject.layer == ObjectLayer.BUILDINGS )
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
			if( this.factionMember.factionId != LevelDataManager.PLAYER_FAC )
			{
				return;
			}
			if( this.IsPaymentDone() )
			{
				if( this.isTraining )
				{
					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Training...: '" + this.trainedUnit.displayName + "' - " + (int)this.trainProgress + " s." );
					SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
				}
				else
				{
					this.ShowList();

					GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select unit to make..." );
					SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
				}
			}
			else
			{
				GameObject status = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.trainedUnit.displayName + "'." );
				SelectionPanel.instance.obj.RegisterElement( "barracks.status", status.transform );
			}
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