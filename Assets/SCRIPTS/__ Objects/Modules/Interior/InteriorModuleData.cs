using KFF;
using SS.Objects.Modules;
using System;
using System.Collections.Generic;

namespace SS.Levels.SaveStates
{
	public class InteriorModuleData : ModuleData
	{
		public Dictionary<int, Guid> slots { get; set; }
		public Dictionary<int, Guid> workerSlots { get; set; }

		public InteriorModuleData()
		{

		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Slots" );
			this.slots = new Dictionary<int, Guid>();
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				int index = serializer.ReadInt( new Path( "Slots.{0}.Index", i ) );
				Guid guid = serializer.ReadGuid( new Path( "Slots.{0}.ObjInside", i ) );

				this.slots.Add( index, guid );
			}
			
			analysisData = serializer.Analyze( "WorkerSlots" );
			this.workerSlots = new Dictionary<int, Guid>();
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				int index = serializer.ReadInt( new Path( "WorkerSlots.{0}.Index", i ) );
				Guid guid = serializer.ReadGuid( new Path( "WorkerSlots.{0}.ObjInside", i ) );

				this.workerSlots.Add( index, guid );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteList( "", "Slots" );
			int i = 0;
			foreach( var kvp in this.slots )
			{
				serializer.AppendClass( "Slots" );
				serializer.WriteInt( new Path( "Slots.{0}", i ), "Index", kvp.Key );
				serializer.WriteGuid( new Path( "Slots.{0}", i ), "ObjInside", kvp.Value );

				i++;
			}
			
			serializer.WriteList( "", "WorkerSlots" );
			i = 0;
			foreach( var kvp in this.workerSlots )
			{
				serializer.AppendClass( "WorkerSlots" );
				serializer.WriteInt( new Path( "WorkerSlots.{0}", i ), "Index", kvp.Key );
				serializer.WriteGuid( new Path( "WorkerSlots.{0}", i ), "ObjInside", kvp.Value );

				i++;
			}
		}
	}
}