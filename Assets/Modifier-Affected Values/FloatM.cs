using System;
using System.Collections.Generic;

namespace Katniss.ModifierAffectedValues
{
	public class FloatM
	{
		private float __baseValue;
		public float baseValue
		{
			get
			{
				return this.__baseValue;
			}
			set
			{
				this.__baseValue = value;
				this.CalculateModifiedValue();
			}
		}

		public Action onAnyChangeCallback;

		private List<Modifier> modifiers;

		public float modifiedValue { get; private set; }
		
		public float this[string id]
		{
			get
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifiers[i].id == id )
					{
						return this.modifiers[i].value;
					}
				}
				throw new Exception( "Unknown id '" + id + "'." );
			}
			set
			{
				for( int i = 0; i < this.modifiers.Count; i++ )
				{
					if( this.modifiers[i].id == id )
					{
						this.modifiers[i].Set( value );
						this.CalculateModifiedValue();
						return;
					}
				}
				this.modifiers.Add( new Modifier( id, value ) );
				this.CalculateModifiedValue();
			}
		}

		public FloatM( float baseValue )
		{
			this.baseValue = baseValue;
			this.modifiers = new List<Modifier>();
			this.CalculateModifiedValue();
		}

		public Modifier[] GetModifiers()
		{
			return this.modifiers.ToArray();
		}

		private void CalculateModifiedValue()
		{
			float oldModified = this.modifiedValue;

			if( this.modifiers == null || this.modifiers.Count == 0 )
			{
				this.modifiedValue = this.baseValue;
				this.onAnyChangeCallback?.Invoke();
				return;
			}

			float value = this.baseValue;
			for( int i = 0; i < this.modifiers.Count; i++ )
			{
				value *= this.modifiers[i].value;
			}
			this.modifiedValue = value;
			this.onAnyChangeCallback?.Invoke();
		}
		
		public static explicit operator FloatM( float right )
		{
			return new FloatM( right );
		}
	}
}