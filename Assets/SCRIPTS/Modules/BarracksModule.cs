using SS.Data;
using SS.ResourceSystem;
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

#warning TODO - change the resource costs from array to dict.

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
			for( int i = 0; i < def.cost.Length; i++ )
			{
				this.resourcesRemaining.Add( this.buildingUnit.cost[i].id, this.buildingUnit.cost[i].amount );
			}

			AwaitForPayment();

		}

		private void AwaitForPayment()
		{
			if( this.GetComponent<PaymentReceiver>() == null )
			{
				PaymentReceiver paymentReceiver = this.gameObject.AddComponent<PaymentReceiver>();
				paymentReceiver.paymentProgress = this;
				paymentReceiver.onPaymentMade.AddListener( ( ResourceStack payment ) =>
				{
					if( this.resourcesRemaining.ContainsKey( payment.id ) )
					{
						this.resourcesRemaining[payment.id] -= payment.amount;
						if( this.resourcesRemaining[payment.id] < 0 )
						{
							this.resourcesRemaining[payment.id] = 0;
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
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 250.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '" + this.buildingUnit.displayName + "'." );
						}
						else
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 250.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Training...: '" + this.buildingUnit.displayName + "' - " + (int)this.buildTime + " s." );
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

						UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 25.0f, 5.0f ), new Vector2( -50.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements );
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select unit to make..." );
					}
				} );
			}
		}

		// Update is called once per frame
		void Update()
		{
			// If we are building something
			if( isTraining )
			{
				Selectable selectable = this.GetComponent<Selectable>();

				if( this.GetComponent<PaymentReceiver>() == null )
				{
					this.buildTime -= this.constructionSpeed * Time.deltaTime;
					if( this.buildTime <= 0 )
					{
						Unit.Create( buildingUnit, this.transform.position, Quaternion.identity, this.GetComponent<FactionMember>().factionId );

						buildingUnit = null;
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
	}
}