// Project: RacingGame, File: LineManager2D.cs
// Namespace: RacingGame.Graphics, Class: LineManager2D
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 408, Size of file: 11,36 KB
// Creation date: 07.09.2006 05:56
// Last modified: 07.11.2006 03:08
// Generated with Commenter by abi.exDream.com

#region Unit Testing
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Graphics;
using RacingGame.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Shaders;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// LineManager class, used for drawing lines. We can't simply draw
	/// lines like in OpenGL (glLine) because we need to do everything with
	/// vertex buffers, which is a bit more work (and using DrawPrimitives
	/// directly produces way worse code).
	/// </summary>
	public class LineManager2D : IDisposable
	{
		#region Variables
		/// <summary>
		/// Number of lines used this frame, will be set to 0 when rendering.
		/// </summary>
		private int numOfLines = 0;

		/// <summary>
		/// The actual list for all the lines, it will NOT be reseted each
		/// frame like numOfLines! We will remember the last lines and
		/// only change this list when anything changes (new line, old
		/// line missing, changing line data).
		/// When this happens buildVertexBuffer will be set to true.
		/// </summary>
		private List<Line> lines = new List<Line>();

		/// <summary>
		/// Struct for a line, instances of this struct will be added to lines.
		/// </summary>
		struct Line
		{
			#region Variables
			/// <summary>
			/// Positions
			/// </summary>
			public Point startPoint, endPoint;
			/// <summary>
			/// Color
			/// </summary>
			public Color color;
			#endregion

			#region Constructor
			/// <summary>
			/// Create line
			/// </summary>
			/// <param name="setStartPoint">Set start point</param>
			/// <param name="setEndPoint">Set end point</param>
			/// <param name="setColor">Set color</param>
			public Line(Point setStartPoint,
				Point setEndPoint, Color setColor)
			{
				startPoint = setStartPoint;
				endPoint = setEndPoint;
				color = setColor;
			} // Line(setStartPoint, setEndPoint, setColor)
			#endregion

			#region Equals operators
			/// <summary>
			/// Are these two Lines equal?
			/// </summary>
			public static bool operator ==(Line a, Line b)
			{
				return a.startPoint == b.startPoint &&
					a.endPoint == b.endPoint &&
					a.color == b.color;
			} // ==(a, b)

			/// <summary>
			/// Are these two Lines not equal?
			/// </summary>
			public static bool operator !=(Line a, Line b)
			{
				return a.startPoint != b.startPoint ||
					a.endPoint != b.endPoint ||
					a.color != b.color;
			} // !=(a, b)

			/// <summary>
			/// Support Equals(.) to keep the compiler happy
			/// (because we used == and !=)
			/// </summary>
			public override bool Equals(object a)
			{
				if (a.GetType() == typeof(Line))
					return (Line)a == this;
				else
					return false; // Object is not a Line
			} // Equals(a)

			/// <summary>
			/// Support GetHashCode() to keep the compiler happy
			/// (because we used == and !=)
			/// </summary>
			public override int GetHashCode()
			{
				return 0; // Not supported or nessescary
			} // GetHashCode()
			#endregion
		} // struct Line

		/// <summary>
		/// Build vertex buffer this frame because the line list was changed?
		/// Note: The vertex buffer implementation was removed some time ago,
		/// but this variable is still used to check for updates to lineVertices!
		/// </summary>
		private bool buildVertexBuffer = false;

		/// <summary>
		/// Vertex buffer for all lines
		/// </summary>
		VertexPositionColor[] lineVertices =
			new VertexPositionColor[MaxNumOfLines * 2];
		/// <summary>
		/// Real number of primitives currently used.
		/// </summary>
		private int numOfPrimitives = 0;

		/// <summary>
		/// Max. number of lines allowed.
		/// </summary>
		private const int MaxNumOfLines = 64;

		/// <summary>
		/// Vertex declaration for our lines.
		/// </summary>
		VertexDeclaration decl = null;
		#endregion
		
		#region Constructor
		/// <summary>
		/// Create LineManager
		/// </summary>
		public LineManager2D()
		{
			decl = new VertexDeclaration(
				RacingGameManager.Device, VertexPositionColor.VertexElements);
		} // LineManager2D()
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		} // Dispose()

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing">Disposing</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				decl.Dispose();
			} // if
		} // Dispose(disposing)
		#endregion

		#region Vertex buffer stuff
		/// <summary>
		/// Update vertex buffer
		/// </summary>
		private void UpdateVertexBuffer()
		{
			// Don't do anything if we got no lines.
			if (numOfLines == 0 ||
				// Or if some data is invalid
				lines.Count < numOfLines)
			{
				numOfPrimitives = 0;
				return;
			} // if (numOfLines)
#if LOG_STUFF
			Log.Write("LineManager.UpdateVertexBuffer() numOfLines=" +
				numOfLines + ", buildVertexBuffer=" + buildVertexBuffer);
#endif

			// Set all lines
			for (int lineNum = 0; lineNum < numOfLines; lineNum++)
			{
				Line line = (Line)lines[lineNum];
				lineVertices[lineNum * 2 + 0] = new VertexPositionColor(
					new Vector3(
					-1.0f + 2.0f * line.startPoint.X / BaseGame.Width,
					-(-1.0f + 2.0f * line.startPoint.Y / BaseGame.Height), 0),
					line.color);
				lineVertices[lineNum * 2 + 1] = new VertexPositionColor(
					new Vector3(
					-1.0f + 2.0f * line.endPoint.X / BaseGame.Width,
					-(-1.0f + 2.0f * line.endPoint.Y / BaseGame.Height), 0),
					line.color);
			} // for (lineNum)
			numOfPrimitives = numOfLines;

			// Vertex buffer was build
			buildVertexBuffer = false;
		} // UpdateVertexBuffer()
		#endregion

		#region Add line
		/// <summary>
		/// Add line
		/// </summary>
		public void AddLine(Point startPoint, Point endPoint, Color color)
		{
			// Don't add new lines if limit is reached
			if (numOfLines >= MaxNumOfLines)
			{
				Log.Write("Too many lines requested in LineManager2D. " +
					"Max lines = " + MaxNumOfLines);
				return;
			} // if (numOfLines)

			// Build line
			Line line = new Line(startPoint, endPoint, color);

			// Check if this exact line exists at the current lines position.
			if (lines.Count > numOfLines)
			{
				if ((Line)lines[numOfLines] != line)
				{
					// overwrite old line, otherwise just increase numOfLines
					lines[numOfLines] = line;
					// Remember to build vertex buffer in Render()
					buildVertexBuffer = true;
				} // if (Line)
			} // if (lines.Count)
			else
			{
				// Then just add new line
				lines.Add(line);
				// Remember to build vertex buffer in Render()
				buildVertexBuffer = true;
			} // else

			// nextUpValue line
			numOfLines++;
		} // AddLine(startPoint, endPoint, color)

		/// <summary>
		/// Add line with shadow
		/// </summary>
		/// <param name="startPoint">Start point</param>
		/// <param name="endPoint">End point</param>
		/// <param name="color">Color</param>
		public void AddLineWithShadow(Point startPoint, Point endPoint,
			Color color)
		{
			AddLine(new Point(startPoint.X, startPoint.Y+1),
				new Point(endPoint.X, endPoint.Y+1), Color.Black);
			AddLine(startPoint, endPoint, color);
		} // AddLineWithShadow(startPoint, endPoint, color)
		#endregion

		#region Render
		/// <summary>
		/// Render all lines added this frame
		/// </summary>
		public virtual void Render()
		{
			// Need to build vertex buffer?
			if (buildVertexBuffer ||
				numOfPrimitives != numOfLines)
			{
				UpdateVertexBuffer();
			} // if (buildVertexBuffer)

			// Render lines if we got any lines to render
			if (numOfPrimitives > 0)
			{
				BaseGame.AlphaBlending = true;
				BaseGame.WorldMatrix = Matrix.Identity;
				ShaderEffect.lineRendering.Render(
					"LineRendering2D",
					delegate
					{
						BaseGame.Device.VertexDeclaration = decl;
						BaseGame.Device.DrawUserPrimitives<VertexPositionColor>(
							PrimitiveType.LineList, lineVertices, 0, numOfPrimitives);
					});
			} // if (numOfVertices)

			// Ok, finally reset numOfLines for next frame
			numOfLines = 0;
		} // Render()
		#endregion

		#region Unit Testing
#if DEBUG
#if !XBOX360
		#region TestLineRendering2D
		/// <summary>
		/// Test line rendering2D
		/// </summary>
		public static void TestLineRendering2D()
		{
			Matrix Projection = Matrix.Identity,
				View = Matrix.Identity;
			Point pos1 = new Point(0, 0),
				pos2 = new Point(500, 250);
			VertexPositionColor[] lineVertices =
				new VertexPositionColor[2];
			Effect effect = null;
			EffectParameter worldViewProj = null;

			TestGame.Start("TestLineRendering2D",
				delegate // init
				{
					float aspectRatio = (float)TestGame.Width / (float)TestGame.Height;
					Projection = Matrix.CreatePerspectiveFieldOfView(
						(float)Math.PI / 2, aspectRatio, 0.1f, 1000.0f);
					View = Matrix.CreateLookAt(
						new Vector3(0, 0, -50), Vector3.Zero, Vector3.Up);

					lineVertices[0] = new VertexPositionColor(
						new Vector3(
						-1.0f + 2.0f * pos1.X / TestGame.Width,
						-(-1.0f + 2.0f * pos1.Y / TestGame.Height), 0), Color.Red);
					lineVertices[1] = new VertexPositionColor(
						new Vector3(
						-1.0f + 2.0f * pos2.X / TestGame.Width,
						-(-1.0f + 2.0f * pos2.Y / TestGame.Height), 0), Color.Green);

					CompiledEffect compiledEffect = Effect.CompileEffectFromFile(
						"Content\\LineRendering.fx", null, null, CompilerOptions.None,
						TargetPlatform.Windows);
					effect = new Effect(TestGame.Device,
						compiledEffect.GetEffectCode(), CompilerOptions.None, null);

					worldViewProj = effect.Parameters["worldViewProj"];

					//Log.Write("techniques: " + effect.Techniques.Count);
				},
				delegate // update
				{
				},
				delegate // render
				{
					// Start line shader
					effect.CurrentTechnique = effect.Techniques["LineRendering2D"];
					effect.Begin(SaveStateMode.None);
					foreach (EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Begin();

						// Render line
						worldViewProj.SetValue(View * Projection);
						TestGame.Device.VertexDeclaration = new VertexDeclaration(
							TestGame.Device, VertexPositionColor.VertexElements);
						TestGame.Device.RenderState.CullMode =
							CullMode.CullClockwiseFace;
						TestGame.Device.DrawUserPrimitives<VertexPositionColor>(
							PrimitiveType.LineList, lineVertices, 0, 1);

						// End shader
						pass.End();
					} // foreach (pass)
					effect.End();
				});
		} // TestLineRendering2D()
		#endregion
#endif

		#region TestRenderLines
		/// <summary>
		/// Test render
		/// </summary>
		//[Test]
		public static void TestRenderLines()
		{
			TestGame.Start(
				delegate
				{
					TestGame.DrawLine(new Point(-100, 0), new Point(50, 100),
						Color.White);
					TestGame.DrawLine(new Point(0, 100), new Point(100, 0),
						Color.Gray);
					TestGame.DrawLine(new Point(400, 0), new Point(100, 0),
						Color.Red);
					TestGame.DrawLine(new Point(10, 50), new Point(100, +150),
						new Color(255, 0, 0, 64));//64, Color.Red));
				});
		} // TestRenderLines()
		#endregion
#endif
		#endregion
	} // class LineManager2D
} // namespace Abi.Graphic
