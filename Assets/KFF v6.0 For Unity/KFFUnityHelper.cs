﻿using Object = KFF.DataStructures.Object;
using UnityEngine;
using static KFF.KFFSerializer;
using System;

namespace KFF
{
	/// <summary>
	/// The official KFF unity extension module.
	/// </summary>
	public static class KFFUnityHelper
	{
		public static Guid ReadGuid( this KFFSerializer serializer, Path path )
		{
			return Guid.ParseExact( serializer.ReadString( path ), "D" );
		}

		public static void WriteGuid( this KFFSerializer serializer, Path path, string name, Guid value )
		{
			serializer.WriteString( path, name, value.ToString( "D" ) );
		}

		public static void WriteGuidArray( this KFFSerializer serializer, Path path, string name, Guid[] values )
		{
			string[] guidArray = new string[values.Length];
			for( int i = 0; i < guidArray.Length; i++ )
			{
				guidArray[i] = values[i].ToString( "D" );
			}
			serializer.WriteStringArray( path, name, guidArray );
		}

		// Everything specific to Unity's implementation of KFF goes here.

		public static Vector2 ReadVector2( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float x = serializer.ReadFloat( "X" );
			float y = serializer.ReadFloat( "Y" );

			serializer.scopeRoot = beginScope;
			return new Vector2( x, y );
		}

		public static Vector2[] ReadVector2Array( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;

			serializer.MoveScope( path, true );
			Object arrayScope = serializer.scopeRoot;

			int length = serializer.Analyze( "" ).childCount;

			Vector2[] ret = new Vector2[length];
			for( int i = 0; i < length; i++ )
			{
				serializer.MoveScope( new Path( "{0}", i ), true );
				float x = serializer.ReadFloat( "X" );
				float y = serializer.ReadFloat( "Y" );
				serializer.scopeRoot = arrayScope;
				ret[i] = new Vector2( x, y );
			}

			serializer.scopeRoot = beginScope;
			return ret;
		}

		public static void WriteVector2( this KFFSerializer serializer, Path path, string name, Vector2 value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "X", value.x );
			serializer.WriteFloat( "", "Y", value.y );

			serializer.scopeRoot = beginScope;
		}

		public static void WriteVector2Array( this KFFSerializer serializer, Path path, string name, Vector2[] values )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			Object arrayScope = serializer.scopeRoot;

			for( int i = 0; i < values.Length; i++ )
			{

				serializer.AppendClass( "" );
				serializer.MoveScope( new Path( "{0}", i ), true );

				serializer.WriteFloat( "", "X", values[i].x );
				serializer.WriteFloat( "", "Y", values[i].y );

				serializer.scopeRoot = arrayScope;
			}
			serializer.scopeRoot = beginScope;
		}

		public static Vector2Int ReadVector2Int( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			int x = serializer.ReadInt( "X" );
			int y = serializer.ReadInt( "Y" );

			serializer.scopeRoot = beginScope;
			return new Vector2Int( x, y );
		}

		public static void WriteVector2Int( this KFFSerializer serializer, Path path, string name, Vector2Int value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteInt( "", "X", value.x );
			serializer.WriteInt( "", "Y", value.y );

			serializer.scopeRoot = beginScope;
		}

		public static Vector3 ReadVector3( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float x = serializer.ReadFloat( "X" );
			float y = serializer.ReadFloat( "Y" );
			float z = serializer.ReadFloat( "Z" );

			serializer.scopeRoot = beginScope;
			return new Vector3( x, y, z );
		}

		public static Vector3[] ReadVector3Array( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;

			serializer.MoveScope( path, true );
			Object arrayScope = serializer.scopeRoot;

			int length = serializer.Analyze( "" ).childCount;

			Vector3[] ret = new Vector3[length];
			for( int i = 0; i < length; i++ )
			{
				serializer.MoveScope( new Path( "{0}", i ), true );
				float x = serializer.ReadFloat( "X" );
				float y = serializer.ReadFloat( "Y" );
				float z = serializer.ReadFloat( "Z" );
				serializer.scopeRoot = arrayScope;
				ret[i] = new Vector3( x, y, z );
			}

			serializer.scopeRoot = beginScope;
			return ret;
		}

		public static void WriteVector3( this KFFSerializer serializer, Path path, string name, Vector3 value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "X", value.x );
			serializer.WriteFloat( "", "Y", value.y );
			serializer.WriteFloat( "", "Z", value.z );

			serializer.scopeRoot = beginScope;
		}

		public static void WriteVector3Array( this KFFSerializer serializer, Path path, string name, Vector3[] values )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteList( "", name );
			serializer.MoveScope( name, true );

			Object arrayScope = serializer.scopeRoot;

			for( int i = 0; i < values.Length; i++ )
			{
				serializer.AppendClass( "" );
				serializer.MoveScope( new Path( "{0}", i ), true );

				serializer.WriteFloat( "", "X", values[i].x );
				serializer.WriteFloat( "", "Y", values[i].y );
				serializer.WriteFloat( "", "Z", values[i].z );

				serializer.scopeRoot = arrayScope;
			}
			serializer.scopeRoot = beginScope;
		}

		public static Vector3Int ReadVector3Int( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			int x = serializer.ReadInt( "X" );
			int y = serializer.ReadInt( "Y" );
			int z = serializer.ReadInt( "Z" );

			serializer.scopeRoot = beginScope;
			return new Vector3Int( x, y, z );
		}

		public static void WriteVector3Int( this KFFSerializer serializer, Path path, string name, Vector3Int value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteInt( "", "X", value.x );
			serializer.WriteInt( "", "Y", value.y );
			serializer.WriteInt( "", "Z", value.z );

			serializer.scopeRoot = beginScope;
		}

		public static Vector4 ReadVector4( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float x = serializer.ReadFloat( "X" );
			float y = serializer.ReadFloat( "Y" );
			float z = serializer.ReadFloat( "Z" );
			float w = serializer.ReadFloat( "W" );

			serializer.scopeRoot = beginScope;
			return new Vector4( x, y, z, w );
		}

		public static void WriteVector4( this KFFSerializer serializer, Path path, string name, Vector4 value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "X", value.x );
			serializer.WriteFloat( "", "Y", value.y );
			serializer.WriteFloat( "", "Z", value.z );
			serializer.WriteFloat( "", "W", value.w );

			serializer.scopeRoot = beginScope;
		}

		public static Quaternion ReadQuaternion( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float x = serializer.ReadFloat( "X" );
			float y = serializer.ReadFloat( "Y" );
			float z = serializer.ReadFloat( "Z" );
			float w = serializer.ReadFloat( "W" );

			serializer.scopeRoot = beginScope;
			return new Quaternion( x, y, z, w );
		}

		public static void WriteQuaternion( this KFFSerializer serializer, Path path, string name, Quaternion value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "X", value.x );
			serializer.WriteFloat( "", "Y", value.y );
			serializer.WriteFloat( "", "Z", value.z );
			serializer.WriteFloat( "", "W", value.w );

			serializer.scopeRoot = beginScope;
		}

		public static Color ReadColor( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float r = serializer.ReadFloat( "R" );
			float g = serializer.ReadFloat( "G" );
			float b = serializer.ReadFloat( "B" );
			float a = serializer.ReadFloat( "A" );

			serializer.scopeRoot = beginScope;
			return new Color( r, g, b, a );
		}

		public static void WriteColor( this KFFSerializer serializer, Path path, string name, Color value )
		{
			Object beginScope = serializer.scopeRoot;

			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "R", value.r );
			serializer.WriteFloat( "", "G", value.g );
			serializer.WriteFloat( "", "B", value.b );
			serializer.WriteFloat( "", "A", value.a );

			serializer.scopeRoot = beginScope;
		}

		public static Bounds ReadBounds( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float x = serializer.ReadFloat( "X" );
			float y = serializer.ReadFloat( "Y" );
			float z = serializer.ReadFloat( "Z" );

			float dx = serializer.ReadFloat( "DX" );
			float dy = serializer.ReadFloat( "DY" );
			float dz = serializer.ReadFloat( "DZ" );

			serializer.scopeRoot = beginScope;
			return new Bounds( new Vector3( x, y, z ), new Vector3( dx, dy, dz ) );
		}

		public static void WriteBounds( this KFFSerializer serializer, Path path, string name, Bounds value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "X", value.center.x );
			serializer.WriteFloat( "", "Y", value.center.y );
			serializer.WriteFloat( "", "Z", value.center.z );

			serializer.WriteFloat( "", "DX", value.size.x );
			serializer.WriteFloat( "", "DY", value.size.y );
			serializer.WriteFloat( "", "DZ", value.size.z );

			serializer.scopeRoot = beginScope;
		}

		public static Rect ReadRect( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			float x = serializer.ReadFloat( "X" );
			float y = serializer.ReadFloat( "Y" );
			float w = serializer.ReadFloat( "W" );
			float h = serializer.ReadFloat( "H" );

			serializer.scopeRoot = beginScope;
			return new Rect( x, y, w, h );
		}

		public static void WriteRect( this KFFSerializer serializer, Path path, string name, Rect value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteFloat( "", "X", value.x );
			serializer.WriteFloat( "", "Y", value.y );
			serializer.WriteFloat( "", "W", value.width );
			serializer.WriteFloat( "", "H", value.height );

			serializer.scopeRoot = beginScope;
		}

		public static BoundsInt ReadBoundsInt( this KFFSerializer serializer, Path path )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			int x = serializer.ReadInt( "X" );
			int y = serializer.ReadInt( "Y" );
			int z = serializer.ReadInt( "Z" );

			int dx = serializer.ReadInt( "DX" );
			int dy = serializer.ReadInt( "DY" );
			int dz = serializer.ReadInt( "DZ" );

			serializer.scopeRoot = beginScope;
			return new BoundsInt( new Vector3Int( x, y, z ), new Vector3Int( dx, dy, dz ) );
		}

		public static void WriteBoundsInt( this KFFSerializer serializer, Path path, string name, BoundsInt value )
		{
			Object beginScope = serializer.scopeRoot;
			serializer.MoveScope( path, true );

			serializer.WriteClass( "", name );
			serializer.MoveScope( name, true );

			serializer.WriteInt( "", "X", value.position.x );
			serializer.WriteInt( "", "Y", value.position.y );
			serializer.WriteInt( "", "Z", value.position.z );

			serializer.WriteInt( "", "DX", value.size.x );
			serializer.WriteInt( "", "DY", value.size.y );
			serializer.WriteInt( "", "DZ", value.size.z );

			serializer.scopeRoot = beginScope;
		}
	}
}