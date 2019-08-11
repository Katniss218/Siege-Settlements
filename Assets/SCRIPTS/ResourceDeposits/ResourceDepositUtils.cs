using UnityEngine;

namespace SS
{
	public static class ResourceDepositUtils
	{
		public static Material CreateMaterial( Texture2D albedo, Texture2D normal, Texture2D emission, float metallic, float smoothness )
		{
			Material mat = new Material( Main.resourceDepositShader );
			
			mat.SetTexture( "_Albedo", albedo );
			mat.SetTexture( "_Normal", normal );
			mat.SetTexture( "_Emission", emission );
			mat.SetFloat( "_Metallic", metallic );
			mat.SetFloat( "_Smoothness", smoothness );

			return mat;
		}
	}
}