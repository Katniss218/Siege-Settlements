using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Katniss.Utils;
using SS.ResourceSystem.Payment;
using Object = UnityEngine.Object;
using SS.Data;
using katniss.Utils;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	public class ConstructionSite : MonoBehaviour, IPaymentProgress
	{
		public class _UnityEvent_string_int : UnityEvent<string, int> { }

		/// <summary>
		/// An array of resource types needed for construction (Read Only).
		/// </summary>
		[SerializeField] private string[] resourceIds;

		/// <summary>
		/// Resources needed to progress from Building.STARTING_HEALTH_PERCENT health to 100%.
		/// </summary>
		[SerializeField] private int[] initialResources;

		/// <summary>
		/// An array of remaining resources, per resource type (use resourceIds[i] to get the Id) (Read Only).
		/// </summary>
		[SerializeField] private float[] resourcesRemaining;

		/// <summary>
		/// Each entry represents a percentage of total resources needed (initial).
		/// </summary>
		[SerializeField] private float[] healthToResources;

		public Func<bool> IsDone { get; private set; }


		public int GetWantedAmount( string resourceId )
		{
			for( int i = 0; i < this.resourceIds.Length; i++ )
			{
				if( this.resourceIds[i] == resourceId )
				{
					return SpecialRound( this.resourcesRemaining[i] );
				}
			}
			return 0;
		}

		/// <summary>
		/// Assigns a cost (in resources) to fully construct the building. Resets any progression in resources.
		/// </summary>
		public void SetRequiredResources( Dictionary<string, int> requiredResources )
		{
			this.resourceIds = new string[requiredResources.Count];
			this.resourcesRemaining = new float[requiredResources.Count];
			this.healthToResources = new float[requiredResources.Count];
			this.initialResources = new int[requiredResources.Count];

			float totalResourcesNeeded = 0;
			int i = 0;
			foreach( var id in requiredResources.Keys )
			{
				int amount = requiredResources[id];
				this.resourceIds[i] = id;
				this.resourcesRemaining[i] = amount;
				this.initialResources[i] = amount;

				totalResourcesNeeded += amount;

				i++;
			}

			// Once we have our total, calculate how much each resource contributes to the total.
			for( i = 0; i < this.healthToResources.Length; i++ )
			{
				this.healthToResources[i] = this.resourcesRemaining[i] / totalResourcesNeeded;
			}

		}

		// Rounds down when the decimal is <=0.5, rounds up when >0.5
		private static int SpecialRound( float remainingResAmt )
		{
			int floored = Mathf.FloorToInt( remainingResAmt );
			if( remainingResAmt - floored <= 0.5f )
			{
				return floored;
			}
			return floored + 1;
		}

		private static GameObject CreateConstructionSiteGraphics( GameObject gameObject )
		{
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();

			int numX = Mathf.FloorToInt( collider.size.x * 2.0f );
			int numZ = Mathf.FloorToInt( collider.size.z * 2.0f );

			float spacingX = collider.size.x / (numX + 1);
			float spacingZ = collider.size.z / (numZ + 1);

			Mesh corner = AssetsManager.GetMesh( "Models/ConstructionSites/Corner.kff" );
			Mesh segment = AssetsManager.GetMesh( "Models/ConstructionSites/Segment.kff" );


			GameObject constructionSiteGfx = new GameObject( "constructionsite" );
			constructionSiteGfx.transform.SetParent( gameObject.transform );
			constructionSiteGfx.transform.localPosition = new Vector3( -collider.size.x / 2f, 0, -collider.size.z / 2f );
			constructionSiteGfx.transform.localRotation = Quaternion.identity;

			GameObject corner00 = new GameObject( "c00" );
			corner00.transform.SetParent( constructionSiteGfx.transform );
			corner00.transform.localPosition = new Vector3( 0, 0, 0 );
			corner00.transform.localRotation = Quaternion.identity;

			MeshFilter meshFilter = corner00.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			MeshRenderer meshRenderer = corner00.AddComponent<MeshRenderer>();
			meshRenderer.material = Main.materialSolid;
			meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );

			GameObject corner01 = new GameObject( "c01" );
			corner01.transform.SetParent( constructionSiteGfx.transform );
			corner01.transform.localPosition = new Vector3( 0, 0, collider.size.z );
			corner01.transform.localRotation = Quaternion.identity;

			meshFilter = corner01.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner01.AddComponent<MeshRenderer>();
			meshRenderer.material = Main.materialSolid;
			meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );

			GameObject corner10 = new GameObject( "c10" );
			corner10.transform.SetParent( constructionSiteGfx.transform );
			corner10.transform.localPosition = new Vector3( collider.size.x, 0, 0 );
			corner10.transform.localRotation = Quaternion.identity;

			meshFilter = corner10.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner10.AddComponent<MeshRenderer>();
			meshRenderer.material = Main.materialSolid;
			meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );

			GameObject corner11 = new GameObject( "c11" );
			corner11.transform.SetParent( constructionSiteGfx.transform );
			corner11.transform.localPosition = new Vector3( collider.size.x, 0, collider.size.z );
			corner11.transform.localRotation = Quaternion.identity;

			meshFilter = corner11.AddComponent<MeshFilter>();
			meshFilter.mesh = corner;

			meshRenderer = corner11.AddComponent<MeshRenderer>();
			meshRenderer.material = Main.materialSolid;
			meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );


			for( int i = 0; i < numX; i++ )
			{
				// 2 rows


				GameObject line1 = new GameObject( "X0-" + i );
				line1.transform.SetParent( constructionSiteGfx.transform );
				line1.transform.localPosition = new Vector3( (i * spacingX) + spacingX, 0, 0 );
				line1.transform.localRotation = Quaternion.identity;

				meshFilter = line1.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line1.AddComponent<MeshRenderer>();
				meshRenderer.material = Main.materialSolid;
				meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );

				GameObject line2 = new GameObject( "X1-" + i );
				line2.transform.SetParent( constructionSiteGfx.transform );
				line2.transform.localPosition = new Vector3( (i * spacingX) + spacingX, 0, collider.size.z );
				line2.transform.localRotation = Quaternion.identity;

				meshFilter = line2.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line2.AddComponent<MeshRenderer>();
				meshRenderer.material = Main.materialSolid;
				meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );
			}

			for( int i = 0; i < numZ; i++ )
			{
				// 2 rows


				GameObject line1 = new GameObject( "Z0-" + i );
				line1.transform.SetParent( constructionSiteGfx.transform );
				line1.transform.localPosition = new Vector3( 0, 0, (i * spacingZ) + spacingZ );
				line1.transform.localRotation = Quaternion.identity;

				meshFilter = line1.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line1.AddComponent<MeshRenderer>();
				meshRenderer.material = Main.materialSolid;
				meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );

				GameObject line2 = new GameObject( "Z1-" + i );
				line2.transform.SetParent( constructionSiteGfx.transform );
				line2.transform.localPosition = new Vector3( collider.size.x, 0, (i * spacingZ) + spacingZ );
				line2.transform.localRotation = Quaternion.identity;

				meshFilter = line2.AddComponent<MeshFilter>();
				meshFilter.mesh = segment;

				meshRenderer = line2.AddComponent<MeshRenderer>();
				meshRenderer.material = Main.materialSolid;
				meshRenderer.material.SetTexture( "_BaseMap", Texture2DUtils.CreateBlank() );
			}

			return constructionSiteGfx;
		}

		/// <summary>
		/// Starts the construction / repair of the specified building.
		/// </summary>
		public static void BeginConstructionOrRepair( GameObject gameObject )
		{
			Damageable damageable = gameObject.GetComponent<Damageable>();
			if( !Building.IsRepairable( damageable ) )
			{
				Debug.LogError( gameObject.name + " - Building is not repairable." );
			}

			Building building = gameObject.GetComponent<Building>();
			

			ConstructionSite constructionSite = gameObject.AddComponent<ConstructionSite>();
			constructionSite.SetRequiredResources( building.StartToEndConstructionCost );
			constructionSite.IsDone = () =>
			{
				for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
				{
					int roundedAmount = SpecialRound( constructionSite.resourcesRemaining[i] );

					if( roundedAmount != 0 )
					{
						return false;
					}
				}
				return true;
			};

			MeshRenderer meshRenderer = gameObject.transform.Find( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME ).GetComponent<MeshRenderer>();

			// cache the function listener so we can reference it to remove it when the construction is complete.
			UnityAction<float> onHealthChange_whenConstructing = ( float deltaHP ) =>
			{
				meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
				if( deltaHP < 0 )
				{
					for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
					{
						float resAmt = (constructionSite.initialResources[i] / (damageable.healthMax * (1 - 0.1f))) * constructionSite.healthToResources[i] * -deltaHP;

						constructionSite.resourcesRemaining[i] += resAmt;
					}
				}
			};

			damageable.onHealthChange.AddListener( onHealthChange_whenConstructing );

			GameObject constructionSiteGfx = CreateConstructionSiteGraphics( gameObject );

			PaymentReceiver paymentReceiver = gameObject.AddComponent<PaymentReceiver>();
			paymentReceiver.paymentProgress = constructionSite;

			// Every time the construction progresses:
			// - Heal the building depending on the amount of resources (100% health => total amount of resources as specified in the definition).
			// - Emit particles.
			// - Play sound.
			paymentReceiver.onPaymentMade.AddListener( ( string id, int amount ) =>
			{
				for( int i = 0; i < constructionSite.resourceIds.Length; i++ )
				{
					// Check if we even want the received resources.
					if( constructionSite.resourceIds[i] == id )
					{
						int roundedRemaining = SpecialRound( constructionSite.resourcesRemaining[i] );
						if( roundedRemaining == 0 )
						{
							throw new Exception( "Received resource wasn't wanted." );
						}
						// Received more than was needed (invalid behavior).
						if( roundedRemaining < amount )
						{
							throw new Exception( "Received amount of '" + id + "' (" + amount + ") was more than the required amount (" + roundedRemaining + ")." );
						}

						float healAmt = ((damageable.healthMax * (1 - 0.1f)) / constructionSite.initialResources[i]) * constructionSite.healthToResources[i] * amount;

						// If it would be healed above the max health (due to rounding up the actual resource amount received), heal it just to the max health.
						// Otherwise, heal it normally.
						if( damageable.health + healAmt > damageable.healthMax )
						{
							damageable.health = damageable.healthMax;
						}
						else
						{
							damageable.health += healAmt;
						}

						constructionSite.resourcesRemaining[i] -= amount;
						if( constructionSite.resourcesRemaining[i] < 0 )
						{
							constructionSite.resourcesRemaining[i] = 0;
						}

						Main.particleSystem.transform.position = gameObject.transform.position + new Vector3( 0, 0.125f, 0 );
						ParticleSystem.ShapeModule shape = Main.particleSystem.GetComponent<ParticleSystem>().shape;

						BoxCollider col = building.GetComponent<BoxCollider>();
						shape.scale = new Vector3( col.size.x, 0.25f, col.size.z );
						shape.position = Vector3.zero;
						Main.particleSystem.GetComponent<ParticleSystem>().Emit( 36 );

						AudioManager.PlayNew( building.buildSoundEffect, 1.0f, 1.0f );

						return;
					}
				}
			} );

			paymentReceiver.onProgressComplete.AddListener( () =>
			{
				// Remove onHealthChange_whenConstructing, so the damageable doesn't call listener, that doesn't exist (cause the construction ended).
				damageable.onHealthChange.RemoveListener( onHealthChange_whenConstructing );

				Object.DestroyImmediate( constructionSite );
				Object.Destroy( constructionSiteGfx );

				Selectable selectable = building.GetComponent<Selectable>();
				if( selectable != null )
				{
					SelectionManager.ForceSelectionUIRedraw( selectable ); // forse redraw to refresh after the const site has beed destroyed.
				}
			} );

			// When the construction starts, set the _Progress attrribute of the material to the current health percent (to make the building appear as being constructed).
			meshRenderer.material.SetFloat( "_Progress", damageable.healthPercent );
		}
	}
}