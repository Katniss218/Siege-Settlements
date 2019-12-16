using System.Collections.Generic;

namespace Katniss.ModifierAffectedValues
{
	public class FloatM
	{
		public float baseValue;

		List<string> modifierIds;
		List<float> modifiers;

		public float this[string id]
		{
			get
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifierIds[i] == id )
					{
						return this.modifiers[i];
					}
				}
				throw new System.Exception( "Unknown id '" + id + "'." );
			}
			set
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifierIds[i] == id )
					{
						this.modifiers[i] = value;
					}
				}
				this.modifiers.Add( value );
				this.modifierIds.Add( id );
			}
		}

		public FloatM( float baseValue )
		{
			this.baseValue = baseValue;
			this.modifierIds = new List<string>();
			this.modifiers = new List<float>();
		}

		public float GetModifiedValue()
		{
			if( this.modifiers.Count == 0 )
			{
				return this.baseValue;
			}
			
			float value = this.baseValue;
			for( int i = 0; i < this.modifiers.Count; i++ )
			{
				value *= this.modifiers[i];
			}
			return value;
		}
		
		public static explicit operator FloatM( float right )
		{
			return new FloatM( right );
		}
	}
}