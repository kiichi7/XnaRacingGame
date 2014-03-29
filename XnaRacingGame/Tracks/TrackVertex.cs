// Project: RacingGame, File: TrackVertex.cs
// Namespace: RacingGame.Tracks, Class: TrackVertex
// Path: C:\code\RacingGame\Tracks, Author: Abi
// Code lines: 276, Size of file: 6,87 KB
// Creation date: 30.09.2006 11:23
// Last modified: 01.10.2006 06:20
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Graphics;
#endregion

namespace RacingGame.Tracks
{
	/// <summary>
	/// Track vertex helper class, used for the initial spline line points,
	/// but also for the final road generation (left and right sides).
	/// This class can be easily be converted to the TangentVertex struct,
	/// we got all the data we need plus some more in here.
	/// For example just use LeftTangentVertex, etc. to get TangentVertices!
	/// </summary>
	public class TrackVertex
	{
		#region Variables
		/// <summary>
		/// Position of this point
		/// </summary>
		public Vector3 pos;
		/// <summary>
		/// Right, up and dir vectors 
		/// </summary>
		public Vector3 right, up, dir;
		/// <summary>
		/// Texture coordinates, u goes along with the road, v is left and right,
		/// which is on the texture top and bottom.
		/// </summary>
		public Vector2 uv;

		/// <summary>
		/// Minimun, maximum and default road width for our track.
		/// </summary>
		public const float MinRoadWidth = 0.25f,
			DefaultRoadWidth = 1.0f,
			MaxRoadWidth = 2.0f,
			RoadWidthScale = 13.25f;//12.5f;//10.0f;//5.0f;

		/// <summary>
		/// Thickness (up/down) of the road for the 3d side.
		/// </summary>
		const float RoadThickness = 1.0f;//0.5f;//0.75f;

		/// <summary>
		/// Road tunnel height.
		/// </summary>
		const float RoadTunnelHeight = 7.125f;//6.5f;//3.5f;//2.0f;//2.5f;//3.5f;

		/// <summary>
		/// Road width
		/// </summary>
		public float roadWidth = DefaultRoadWidth;
		#endregion

		#region Properties
		/// <summary>
		/// Left side track vertex generation, used for the GuardRail class.
		/// </summary>
		/// <returns>Track vertex</returns>
		public TrackVertex LeftTrackVertex
		{
			get
			{
				return new TrackVertex(
					pos - RoadWidthScale * roadWidth * right / 2,
					right, up, dir,
					new Vector2(uv.X, 0),
					roadWidth);
			} // get
		} // LeftTrackVertex

		/// <summary>
		/// Right side track vertex generation, used for the GuardRail class.
		/// </summary>
		/// <returns>Track vertex</returns>
		public TrackVertex RightTrackVertex
		{
			get
			{
				return new TrackVertex(
					pos + RoadWidthScale * roadWidth * right / 2,
					right, up, dir,
					new Vector2(uv.X, roadWidth),
					roadWidth);
			} // get
		} // RightTrackVertex

		/// <summary>
		/// Left side tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex LeftTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos - RoadWidthScale * roadWidth * right / 2,
					new Vector2(uv.X, 0),
					up, right);
			} // get
		} // LeftTangentVertex

		/// <summary>
		/// Right side tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex RightTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos + RoadWidthScale * roadWidth * right / 2,
					new Vector2(uv.X, roadWidth),
					up, right);
			} // get
		} // RightTangentVertex

		/// <summary>
		/// Middle tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex MiddleTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos,
					new Vector2(uv.X, roadWidth / 2),
					up, right);
			} // get
		} // MiddleTangentVertex

		/// <summary>
		/// Middle left tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex MiddleLeftTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos - RoadWidthScale * roadWidth * right / 4,
					new Vector2(uv.X, roadWidth / 4),
					up, right);
			} // get
		} // MiddleLeftTangentVertex

		/// <summary>
		/// Middle right tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex MiddleRightTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos + RoadWidthScale * roadWidth * right / 4,
					new Vector2(uv.X, roadWidth * 3 / 4),
					up, right);
			} // get
		} // MiddleRightTangentVertex

		/// <summary>
		/// Bottom left side tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex BottomLeftSideTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos - RoadWidthScale * roadWidth * right / 2 -
					up * RoadThickness * roadWidth,
					new Vector2(uv.X, 0),
					-up, -right);
			} // get
		} // BottomLeftSideTangentVertex

		/// <summary>
		/// Bottom right side tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex BottomRightSideTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos + RoadWidthScale * roadWidth * right / 2 -
					up * RoadThickness * roadWidth,
					new Vector2(uv.X, 1),
					-up, -right);
			} // get
		} // BottomRightSideTangentVertex

		/// <summary>
		/// Tunnel top left side tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex TunnelTopLeftSideTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos - RoadWidthScale * roadWidth * right / 2 +
					up * RoadTunnelHeight,
					new Vector2(uv.X, 0),
					-up, -right);
			} // get
		} // BottomLeftSideTangentVertex

		/// <summary>
		/// Tunnel top right side tangent vertex
		/// </summary>
		/// <returns>Tangent vertex</returns>
		public TangentVertex TunnelTopRightSideTangentVertex
		{
			get
			{
				return new TangentVertex(
					pos + RoadWidthScale * roadWidth * right / 2 +
					up * RoadTunnelHeight,
					new Vector2(uv.X, 1),
					-up, -right);
			} // get
		} // TunnelTopRightSideTangentVertex
		#endregion

		#region Constructor
		/// <summary>
		/// Create track vertex
		/// </summary>
		/// <param name="setPos">Set position</param>
		/// <param name="setRight">Set right</param>
		/// <param name="setUp">Set up</param>
		/// <param name="setDir">Set dir</param>
		/// <param name="setUv">Set uv</param>
		public TrackVertex(Vector3 setPos,
			Vector3 setRight, Vector3 setUp, Vector3 setDir,
			Vector2 setUv, float setRoadWidth)
		{
			pos = setPos;
			right = setRight;
			up = setUp;
			dir = setDir;
			uv = setUv;
			roadWidth = setRoadWidth;
		} // TrackVertex(setPos, setRight, setUp)

		/// <summary>
		/// Create track vertex
		/// /// </summary>
		/// <param name="setPos">Set position</param>
		public TrackVertex(Vector3 setPos)
		{
			pos = setPos;
 			right = Vector3.Right;
 			up = Vector3.Up;
 			dir = Vector3.Forward;
 			uv = Vector2.Zero;
		} // TrackVertex(setPos)
		#endregion
	} // class TrackVertex
} // namespace RacingGame.Tracks
