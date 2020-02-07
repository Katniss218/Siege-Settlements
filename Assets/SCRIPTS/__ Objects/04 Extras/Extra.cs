using SS.UI;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Extras
{
	public class Extra : SSObject, ISelectDisplayHandler
	{
		NavMeshObstacle __obstacle = null;
		private bool isObstacleGet = false;
		public NavMeshObstacle obstacle
		{
			get
			{
				if( !this.isObstacleGet )
				{
					this.__obstacle = this.GetComponent<NavMeshObstacle>();
				}
				return this.__obstacle;
			}
		}

		BoxCollider __collider = null;
		new public BoxCollider collider
		{
			get
			{
				if( this.__collider == null )
				{
					this.__collider = this.GetComponent<BoxCollider>();
				}
				return this.__collider;
			}
		}

		public void OnDisplay()
		{
			SelectionPanel.instance.obj.SetIcon( this.icon );

			SelectionPanel.instance.obj.displayNameText.text = this.displayName;
		}

		public void OnHide()
		{

		}
	}
}