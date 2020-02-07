using SS.AI;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using System.Collections.Generic;

namespace SS.Objects.Modules
{
	public class TavernWorkplaceModule : WorkplaceModule
	{
		public const string KFF_TYPEID = "workplace_tavern";

		void Update()
		{
			bool isWorkerInside = false;
			List<CivilianUnitExtension> emp = this.interior.GetAllEmployed();
			for( int i = 0; i < emp.Count; i++ )
			{
				if( emp[i].workplace == this && emp[i].obj.interior == this.interior && emp[i].obj.slotType == InteriorModule.SlotType.Worker )
				{
					isWorkerInside = true;
				}
			}

			if( isWorkerInside )
			{
				for( int i = 0; i < this.interior.SlotCount( InteriorModule.SlotType.Generic ); i++ )
				{
					IInteriorUser objInside = this.interior.GetUser( InteriorModule.SlotType.Generic, i );
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
			// empty at the moment.
		}


		public override SSModuleData GetData()
		{
			// no data needs to be saved.
			return new TavernWorkplaceModuleData();
		}

		public override void SetData( SSModuleData _data )
		{
			// no data needs to be saved.
			TavernWorkplaceModuleData data = ValidateDataType<TavernWorkplaceModuleData>( _data );
		}
	}
}
