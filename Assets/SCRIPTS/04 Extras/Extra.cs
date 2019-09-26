using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
{
	public class Extra : MonoBehaviour
	{
		private static List<GameObject> _allExtras = new List<GameObject>();

		public static GameObject[] GetAllExtras()
		{
			return _allExtras.ToArray();
		}

		public Guid guid { get; set; }

		public string defId { get; set; }


		void OnEnable()
		{
			_allExtras.Add( this.gameObject );
		}

		void OnDisable()
		{
			_allExtras.Remove( this.gameObject );
		}
	}
}