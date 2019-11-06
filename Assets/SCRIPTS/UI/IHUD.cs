using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.UI
{
	public interface IHUD
	{
		void SetColor( Color color );
		void SetHealthBarFill( float percentHealth );
	}
}