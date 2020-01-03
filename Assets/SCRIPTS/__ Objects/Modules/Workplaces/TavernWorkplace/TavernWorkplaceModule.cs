using SS.AI;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class TavernWorkplaceModule : WorkplaceModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "workplace_tavern";

		void Update()
		{
			bool isWorkerInside = false;
			List<CivilianUnitExtension> emp = this.interior.GetEmployed();
			for( int i = 0; i < emp.Count; i++ )
			{
				if( emp[i].workplace == this && emp[i].unit.interior == this.interior && emp[i].unit.slotType == InteriorModule.SlotType.Worker )
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
		}


		public override ModuleData GetData()
		{
			return new TavernWorkplaceModuleData();
#warning data.
		}

		public override void SetData( ModuleData data )
		{

		}



		public void OnDisplay()
		{
			
		}

		public void OnHide()
		{

		}
	}
}
