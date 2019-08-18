using System;
using UnityEngine;

namespace SS
{
	public class EveryFrameSingle : MonoBehaviour
	{
		/// <summary>
		/// Is called every frame in the Update() method.
		/// </summary>
		public Action onUpdate;

		/// <summary>
		/// Is called every frame in the LateUpdate() method.
		/// </summary>
		public Action onLateUpdate;

		void Update()
		{
			onUpdate?.Invoke();
		}

		void LateUpdate()
		{
			onLateUpdate?.Invoke();
		}
	}
}