using SS.Objects.Units;
using SS.UI;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class TavernWorkplaceModule : WorkplaceModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "workplace_tavern";

		void Update()
		{
			bool isWorkerInside = false;
			Unit[] emp = this.interior.GetEmployed();
			for( int i = 0; i < emp.Length; i++ )
			{
				if( emp[i].workplace == this )
				{
					isWorkerInside = true;
				}
			}

			if( isWorkerInside )
			{
				for( int i = 0; i < this.interior.slots.Length; i++ )
				{
					IEnterableInside objInside = this.interior.slots[i].objInside;
					if( objInside == null )
					{
						continue;
					}
					if( objInside is IDamageable )
					{
						IDamageable damInside = (IDamageable)objInside;

						if( damInside.health < damInside.healthMax )
						{
							damInside.health += 0.01f;
						}
					}
				}
			}
		}

		public override void MakeDoWork( Unit worker )
		{
#warning worker schedule.
			throw new System.NotImplementedException();
		}


		public override ModuleData GetData()
		{
			throw new System.NotImplementedException();
		}

		public override void SetData( ModuleData data )
		{
			throw new System.NotImplementedException();
		}



		public void OnDisplay()
		{
			
		}

		public void OnHide()
		{

		}
	}
}
