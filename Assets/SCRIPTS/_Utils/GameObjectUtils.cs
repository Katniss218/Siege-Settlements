using SS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Katniss.Utils
{
	public static class GameObjectUtils
	{
		public const string GRAPHICS_GAMEOBJECT_NAME = "graphics";

		public static void RectTransform( Transform parent, string name, Vector2 pos, Vector2 size, Vector2 piv, Vector2 amin, Vector2 amax, out GameObject gameObject, out RectTransform rectTransform )
		{
			GameObject go = new GameObject( name );
			RectTransform rt = go.AddComponent<RectTransform>();
			rt.SetParent( parent );
			rt.ApplyUIData( new GenericUIData( pos, size, piv, amin, amax ) );
			gameObject = go;
			rectTransform = rt;

		}

		public static void RectTransform( Transform parent, string name, GenericUIData data, out GameObject gameObject, out RectTransform rectTransform )
		{
			GameObject go = new GameObject( name );
			RectTransform rt = go.AddComponent<RectTransform>();
			rt.SetParent( parent );
			rt.ApplyUIData( data );
			gameObject = go;
			rectTransform = rt;

		}
		
		public static Image AddImageSliced( this GameObject gameObject, Sprite sprite, bool raycastTarget )
		{
			Image image = gameObject.AddComponent<Image>();

			image.type = Image.Type.Sliced;
			image.sprite = sprite;
			image.raycastTarget = raycastTarget;

			return image;
		}

		public static void AddParticleSystem( this GameObject gameObject, float amountPerSec, Texture2D tex, Color c, float startSize, float endSize, float emissionRadius, float lifetime )
		{
			ParticleSystem particleSystem = gameObject.AddComponent<ParticleSystem>();
			ParticleSystem.MainModule main = particleSystem.main;
			main.startSpeed = 0;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = 1.0f;
			main.startLifetime = lifetime;

			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.radius = emissionRadius;
			shape.shapeType = ParticleSystemShapeType.Sphere;

			ParticleSystem.SizeOverLifetimeModule sizeOverTime = particleSystem.sizeOverLifetime;
			sizeOverTime.enabled = true;
			AnimationCurve curve = new AnimationCurve();
			curve.AddKey( 0.0f, startSize );
			curve.AddKey( 1.0f, endSize );
			sizeOverTime.size = new ParticleSystem.MinMaxCurve( 1.0f, curve );

			//shape

			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.rateOverTime = amountPerSec;
			
			ParticleSystemRenderer renderer = gameObject.GetComponent<ParticleSystemRenderer>();
			renderer.material = CreateMaterial( tex, c );
		}

		private static Material CreateMaterial( Texture2D tex, Color c )
		{
			Material mat = new Material( SS.Main.materialParticle );
			mat.SetTexture( "_BaseMap", tex );
			mat.SetColor( "_BaseColor", c );

			return mat;
		}
		/*
		public static TextMeshProUGUI AddTextMesh( this GameObject gameObject, string text, TMP_FontAsset font, int fontSize, FontWeight fontWeight, TextAlignmentOptions alignment )
		{
			TextMeshProUGUI titleText = gameObject.AddComponent<TextMeshProUGUI>();
			titleText.text = "";
			titleText.font = Main.mainFont;
			titleText.fontSize = titleFontSize;
			titleText.fontWeight = FontWeight.Bold;
			titleText.alignment = TextAlignmentOptions.Center;
			titleText.characterSpacing = 0f;
			titleText.enableWordWrapping = false;
		}*/
	}
}