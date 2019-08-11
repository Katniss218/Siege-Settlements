using UnityEngine;

namespace SS.Units
{
	public static class UnitUtils
	{
		public static Material CreateMaterial( Color factionColor, Texture2D albedo, Texture2D normal, Texture2D emission, float metallic, float smoothness )
		{
			Material mat = new Material( Main.unitShader );

			mat.SetColor( "_FactionColor", factionColor );
			mat.SetTexture( "_Albedo", albedo );
			mat.SetTexture( "_Normal", normal );
			mat.SetTexture( "_Emission", emission );
			mat.SetFloat( "_Metallic", metallic );
			mat.SetFloat( "_Smoothness", smoothness );

			return mat;
		}
	}
}