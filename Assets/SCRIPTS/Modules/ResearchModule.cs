using SS.Data;
using SS.ResourceSystem.Payment;
using SS.Technologies;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	[RequireComponent( typeof( FactionMember ) )]
	public class ResearchModule : Module, IPaymentProgress
	{
		/// <summary>
		/// Contains the currently researched technology (Read only).
		/// </summary>
		public TechnologyDefinition technologyResearched { get; private set; }

		/// <summary>
		/// Contains the progress of the research (Read Only).
		/// </summary>
		public float researchProgress { get; private set; }

		/// <summary>
		/// Contains the multiplier used as the construction speed.
		/// </summary>
		public float researchSpeed { get; set; }

		/// <summary>
		/// returns true if the research module is currently researching a technology.
		/// </summary>
		public bool isResearching
		{
			get
			{
				return this.technologyResearched != null;
			}
		}

		public Func<bool> IsDone { get; private set; }


		private Dictionary<string, int> resourcesRemaining = new Dictionary<string, int>();

		private FactionMember factionMember;


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

		/// <summary>
		/// Begins researching a technology.
		/// </summary>
		/// <param name="def">The technology to research.</param>
		public void StartResearching( TechnologyDefinition def )
		{
			if( this.isResearching )
			{
				Debug.LogWarning( "There is already technology being researched." );
				return;
			}
			this.technologyResearched = def;
			this.researchProgress = 10.0f;

			this.resourcesRemaining.Clear();
			foreach( var kvp in this.technologyResearched.cost )
			{
				this.resourcesRemaining.Add( kvp.Key, kvp.Value );
			}

			AwaitForPayment();
		}

		// Start is called before the first frame update
		void Start()
		{
			this.factionMember = GetComponent<FactionMember>();

			Selectable selectable = this.GetComponent<Selectable>();

			if( selectable != null )
			{
				//####
				// Assign the UI redraw pass.
				//####
				selectable.onSelectionUIRedraw.AddListener( () =>
				{
					Damageable d = this.GetComponent<Damageable>();
					// If the research facility is not usable.
					if( selectable.gameObject.layer == LayerMask.NameToLayer( "Buildings" ) && d != null && !Buildings.Building.CheckUsable( d ) )
					{
						return;
					}
					if( this.isResearching )
					{
						if( this.GetComponent<PaymentReceiver>() != null )
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Waiting for resources: '"+ this.technologyResearched.displayName+"'." );
						}
						else
						{
							UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Researching...: '" + this.technologyResearched.displayName + "' - " + (int)this.researchProgress + " s." );
						}
					}
					else
					{
						List<TechnologyDefinition> techDefs = DataManager.GetAllOfType<TechnologyDefinition>();
						List<GameObject> gridElements = new List<GameObject>();
						// Add every available technology to the list.
						for( int i = 0; i < techDefs.Count; i++ )
						{
							TechnologyDefinition techDef = techDefs[i];
							// If it can be researched, add clickable button, otherwise add unclickable button that represents tech already researched/locked.
							if( FactionManager.factions[this.factionMember.factionId].techs[techDef.id] == TechnologyResearchProgress.Available )
							{
								gridElements.Add( UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, () =>
								{
									StartResearching( techDef );
									// Force the Object UI to update and show that now we are researching a tech.
									SelectionManager.ForceSelectionUIRedraw( selectable );
								} ) );
							}
							else
							{
								gridElements.Add( UIUtils.InstantiateIconButton( SelectionPanel.objectTransform, new GenericUIData( new Vector2( i * 72.0f, 72.0f ), new Vector2( 72.0f, 72.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), techDef.icon.Item2, null ) );
							}
						}
						// Create the actual UI.
						UIUtils.InstantiateText( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 0.0f, 0.0f ), new Vector2( -50.0f, 50.0f ), new Vector2( 0.5f, 1.0f ), Vector2.up, Vector2.one ), "Select tech to research..." );
						UIUtils.InstantiateScrollableGrid( SelectionPanel.objectTransform, new GenericUIData( new Vector2( 75.0f, 5.0f ), new Vector2( -150.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), 72, gridElements.ToArray() );
					}
				} );

			}
		}

		// Update is called once per frame
		void Update()
		{
			if( this.isResearching )
			{
				if( this.GetComponent<PaymentReceiver>() == null )
				{
					this.researchProgress -= this.researchSpeed * Time.deltaTime;
					if( this.researchProgress <= 0 )
					{
						FactionManager.factions[this.factionMember.factionId].techs[this.technologyResearched.id] = TechnologyResearchProgress.Researched;
						this.technologyResearched = null;
						
						SelectionManager.ForceSelectionUIRedraw( null ); // if it needs to update (e.g. civilian that could now build new buildings).
					}

					// Force the SelectionPanel.Object UI to update and show that we either have researched the tech, ot that the progress progressed.
					Selectable selectable = this.GetComponent<Selectable>();
					if( selectable != null )
					{
						SelectionManager.ForceSelectionUIRedraw( selectable );
					}
				}
			}
		}

		public static void AddTo( GameObject obj, ResearchModuleDefinition def )
		{
			ResearchModule research = obj.AddComponent<ResearchModule>();

			research.researchSpeed = def.researchSpeed;

			// Set the method for checking progress of the construction.
			research.IsDone = () =>
			{
				foreach( var kvp in research.resourcesRemaining )
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