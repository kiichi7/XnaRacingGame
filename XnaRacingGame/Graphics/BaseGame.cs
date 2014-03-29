// Project: RacingGame, File: BaseGame.cs
// Namespace: RacingGame.Graphics, Class: BaseGame
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 1773, Size of file: 51,89 KB
// Creation date: 07.09.2006 05:56
// Last modified: 27.10.2006 01:03
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using RacingGame.GameLogic;
using RacingGame.Helpers;
using RacingGame.Properties;
using RacingGame.Shaders;
using RacingGame.Sounds;
using System.IO;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// Base game class for all the basic game support.
	/// Connects all our helper classes together and makes our live easier!
	/// Note: This game was designed for 16:9 (e.g. 1920x1200), but it
	/// also works just fine for 4:3 (1024x768, 800x600, etc.).
	/// This is the reason you might see 1024x640 instead of 1024x768 in
	/// some width/height calculations and the UI textures.
	/// The game looks best at 1920x1200 (HDTV 1020p resolution).
	/// </summary>
	public partial class BaseGame : Microsoft.Xna.Framework.Game
	{
		#region Constants
		/// <summary>
		/// Background color
		/// </summary>
		private static readonly Color BackgroundColor = Color.Black;
		/// <summary>
		/// Field of view and near and far plane distances for the
		/// ProjectionMatrix creation.
		/// </summary>
		private const float FieldOfView = (float)Math.PI / 2,
			NearPlane = 0.5f,//1.0f,
			FarPlane = 1750;//2000.0f;//1250.0f;//1750.0f;//2500.0f;//1000.0f;

		/// <summary>
		/// Viewable field of view for object visibility testing (see Model class)
		/// </summary>
		public const float ViewableFieldOfView = FieldOfView / 1.125f;
		#endregion

		#region Variables
		/// <summary>
		/// Graphics device manager, used for the graphics creation and holds
		/// the GraphicsDevice.
		/// </summary>
		protected GraphicsDeviceManager graphicsManager = null;

		/// <summary>
		/// Device
		/// </summary>
		protected static GraphicsDevice device;

		/// <summary>
		/// Content manager
		/// </summary>
		protected static ContentManager content = null;

		/// <summary>
		/// UI Renderer helper class for all 2d rendering.
		/// </summary>
		protected static UIRenderer ui = null;

		/// <summary>
		/// Our screen resolution: Width and height of visible render area.
		/// </summary>
		protected static int width, height;
		/// <summary>
		/// Aspect ratio of our current resolution
		/// </summary>
		private static float aspectRatio = 1.0f;

		/// <summary>
		/// Remember windows title to check if we are in certain unit tests!
		/// </summary>
		private static string remWindowsTitle = "";

		/// <summary>
		/// Get windows title to check if we are in certain unit tests!
		/// </summary>
		public static string WindowsTitle
		{
			get
			{
				return remWindowsTitle;
			} // get
		} // WindowsTitle

		/// <summary>
		/// Line manager 2D
		/// </summary>
		private static LineManager2D lineManager2D = null;
		/// <summary>
		/// Line manager 3D
		/// </summary>
		private static LineManager3D lineManager3D = null;

		/// <summary>
		/// Mesh render manager to render meshes of models in a highly
		/// optimized manner. We don't really have anything stored in here
		/// except for a sorted list on how to render everything based on the
		/// techniques and the materials and links to the renderable meshes.
		/// </summary>
		private static MeshRenderManager meshRenderManager =
			new MeshRenderManager();

		/// <summary>
		/// Matrices for shaders. Used in a similar way than in Rocket Commander,
		/// but since we don't have a fixed function pipeline here we just use
		/// these values in the shader. Make sure to set all matrices before
		/// calling a shader. Inside a shader you have to update the shader
		/// parameter too, just setting the WorldMatrix alone does not work.
		/// </summary>
		private static Matrix worldMatrix,
			viewMatrix,
			projectionMatrix;

		/// <summary>
		/// Light direction, please read matrices info above for more details.
		/// The same things apply here.
		/// </summary>
		private static Vector3 lightDirection = new Vector3(0, 0, 1);

		/// <summary>
		/// Light direction
		/// </summary>
		/// <returns>Vector 3</returns>
		public static Vector3 LightDirection
		{
			get
			{
				return lightDirection;
			} // get
			set
			{
				lightDirection = value;
				lightDirection.Normalize();
			} // set
		} // LightDirection

		/// <summary>
		/// Elapsed time this frame in ms. Always have something valid here
		/// in case we devide through this values!
		/// </summary>
		private static float elapsedTimeThisFrameInMs = 0.001f, totalTimeMs = 0,
			lastFrameTotalTimeMs = 0;

		/// <summary>
		/// Helper for calculating frames per second. 
		/// </summary>
		private static float startTimeThisSecond = 0;

		/// <summary>
		/// For more accurate frames per second calculations,
		/// just count for one second, then fpsLastSecond is updated.
		/// Btw: Start with 1 to help some tests avoid the devide through zero
		/// problem.
		/// </summary>
		private static int
			frameCountThisSecond = 0,
			totalFrameCount = 0,
			fpsLastSecond = 60;

		/// <summary>
		/// Return true every checkMilliseconds.
		/// </summary>
		/// <param name="checkMilliseconds">Check ms</param>
		/// <returns>Bool</returns>
		public static bool EveryMillisecond(int checkMilliseconds)
		{
			return (int)(lastFrameTotalTimeMs / checkMilliseconds) !=
				(int)(totalTimeMs / checkMilliseconds);
		} // EveryMillisecond(checkMilliseconds)
		#endregion

		#region Properties
		#region Device
		/// <summary>
		/// Device
		/// </summary>
		/// <returns>Graphics device</returns>
		public static GraphicsDevice Device
		{
			get
			{
				return device;
			} // get
		} // Device

		private static bool alreadyCheckedOptionsAndPSVersion = false,
			canUsePS20 = false,
			canUsePS30 = false;

		/// <summary>
		/// Check options and PS version
		/// </summary>
		internal static void CheckOptionsAndPSVersion()
		{
			if (device == null)
				throw new InvalidOperationException("Device is not created yet!");

			alreadyCheckedOptionsAndPSVersion = true;
			
			usePostScreenShaders = GameSettings.Default.PostScreenEffects;
			allowShadowMapping = GameSettings.Default.ShadowMapping;
			highDetail = GameSettings.Default.HighDetail;

			canUsePS20 =
				device.GraphicsDeviceCapabilities.PixelShaderVersion.Major >= 2 &&
				GameSettings.Default.PerformanceSettings != 0;
			canUsePS30 =
				device.GraphicsDeviceCapabilities.PixelShaderVersion.Major >= 3 &&
				(GameSettings.Default.PerformanceSettings == -1 ||
				GameSettings.Default.PerformanceSettings == 2);
		} // CheckOptionsAndPSVersion()

		/// <summary>
		/// Can use PS20
		/// </summary>
		/// <returns>Bool</returns>
		public static bool CanUsePS20
		{
			get
			{
				if (alreadyCheckedOptionsAndPSVersion == false)
					CheckOptionsAndPSVersion();

				return canUsePS20;
			} // get
		} // CanUsePS20

		/// <summary>
		/// Can use PS30
		/// </summary>
		/// <returns>Bool</returns>
		public static bool CanUsePS30
		{
			get
			{
				if (alreadyCheckedOptionsAndPSVersion == false)
					CheckOptionsAndPSVersion();

				return canUsePS30;
			} // get
		} // CanUsePS30

		private static bool highDetail = true;
		/// <summary>
		/// High detail
		/// </summary>
		/// <returns>Bool</returns>
		public static bool HighDetail
		{
			get
			{
				if (alreadyCheckedOptionsAndPSVersion == false)
					CheckOptionsAndPSVersion();

				return highDetail;
			} // get
		} // HighDetail

		private static bool allowShadowMapping = true;
		/// <summary>
		/// Allow shadow mapping
		/// </summary>
		/// <returns>Bool</returns>
		public static bool AllowShadowMapping
		{
			get
			{
				if (alreadyCheckedOptionsAndPSVersion == false)
					CheckOptionsAndPSVersion();

				return allowShadowMapping;
			} // get
		} // AllowShadowMapping

		private static bool usePostScreenShaders = true;
		/// <summary>
		/// Use post screen shaders
		/// </summary>
		/// <returns>Bool</returns>
		public static bool UsePostScreenShaders
		{
			get
			{
				if (alreadyCheckedOptionsAndPSVersion == false)
					CheckOptionsAndPSVersion();

				return usePostScreenShaders;
			} // get
		} // UsePostScreenShaders
		#endregion

		#region Content manager
		/// <summary>
		/// Content
		/// </summary>
		/// <returns>Content manager</returns>
		public static new ContentManager Content
		{
			get
			{
				return content;
			} // get
		} // Content
		#endregion

		#region UI
		/// <summary>
		/// User interface renderer helper ^^
		/// </summary>
		/// <returns>UIRenderer</returns>
		public static UIRenderer UI
		{
			get
			{
				return ui;
			} // get
		} // UI
		#endregion

		#region MeshRenderManager
		/// <summary>
		/// Mesh render manager to render meshes of models in a highly
		/// optimized manner.
		/// </summary>
		/// <returns>Mesh render manager</returns>
		public static MeshRenderManager MeshRenderManager
		{
			get
			{
				return meshRenderManager;
			} // get
		} // MeshRenderManager
		#endregion

		#region Resolution and stuff
		/// <summary>
		/// Width
		/// </summary>
		/// <returns>Int</returns>
		public static int Width
		{
			get
			{
				return width;
			} // get
		} // Width
		/// <summary>
		/// Height
		/// </summary>
		/// <returns>Int</returns>
		public static int Height
		{
			get
			{
				return height;
			} // get
		} // Height

		/// <summary>
		/// Aspect ratio
		/// </summary>
		/// <returns>Float</returns>
		public static float AspectRatio
		{
			get
			{
				return aspectRatio;
			} // get
		} // AspectRatio

		/// <summary>
		/// Resolution rectangle
		/// </summary>
		/// <returns>Rectangle</returns>
		public static Rectangle ResolutionRect
		{
			get
			{
				return new Rectangle(0, 0, width, height);
			} // get
		} // ResolutionRect
		#endregion

		#region Calc rectangle helpers
		/// <summary>
		/// XToRes helper method to convert 1024x640 to the current
		/// screen resolution. Used to position UI elements.
		/// </summary>
		/// <param name="xIn1024px">X in 1024px width resolution</param>
		/// <returns>Int</returns>
		public static int XToRes(int xIn1024px)
		{
			return (int)Math.Round(xIn1024px * BaseGame.Width / 1024.0f);
		} // XToRes(xIn1024px)

		/// <summary>
		/// YToRes helper method to convert 1024x640 to the current
		/// screen resolution. Used to position UI elements.
		/// </summary>
		/// <param name="yIn640px">Y in 640px height</param>
		/// <returns>Int</returns>
		public static int YToRes(int yIn640px)
		{
			return (int)Math.Round(yIn640px * BaseGame.Height / 640.0f);
		} // YToRes(yIn768px)

		/// <summary>
		/// YTo res 768
		/// </summary>
		/// <param name="yIn768px">Y in 768px</param>
		/// <returns>Int</returns>
		public static int YToRes768(int yIn768px)
		{
			return (int)Math.Round(yIn768px * BaseGame.Height / 768.0f);
		} // YToRes768(yIn768px)

		/// <summary>
		/// XTo res 1600
		/// </summary>
		/// <param name="xIn1600px">X in 1600px</param>
		/// <returns>Int</returns>
		public static int XToRes1600(int xIn1600px)
		{
			return (int)Math.Round(xIn1600px * BaseGame.Width / 1600.0f);
		} // XToRes1600(xIn1600px)

		/// <summary>
		/// YTo res 1200
		/// </summary>
		/// <param name="yIn768px">Y in 1200px</param>
		/// <returns>Int</returns>
		public static int YToRes1200(int yIn1200px)
		{
			return (int)Math.Round(yIn1200px * BaseGame.Height / 1200.0f);
		} // YToRes1200(yIn1200px)

		/// <summary>
		/// XTo res 1400
		/// </summary>
		/// <param name="xIn1400px">X in 1400px</param>
		/// <returns>Int</returns>
		public static int XToRes1400(int xIn1400px)
		{
			return (int)Math.Round(xIn1400px * BaseGame.Width / 1400.0f);
		} // XToRes1400(xIn1400px)

		/// <summary>
		/// YTo res 1200
		/// </summary>
		/// <param name="yIn1050px">Y in 1050px</param>
		/// <returns>Int</returns>
		public static int YToRes1050(int yIn1050px)
		{
			return (int)Math.Round(yIn1050px * BaseGame.Height / 1050.0f);
		} // YToRes1050(yIn1050px)

		/// <summary>
		/// Calc rectangle, helper method to convert from our images (1024)
		/// to the current resolution. Everything will stay in the 16/9
		/// format of the textures.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="width">Width</param>
		/// <param name="height">Height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangle(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 640.0f;
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangle(x, y, width)

		/// <summary>
		/// Calc rectangle with bounce effect, same as CalcRectangle, but sizes
		/// the resulting rect up and down depending on the bounceEffect value.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <param name="bounceEffect">Bounce effect</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleWithBounce(
			int relX, int relY, int relWidth, int relHeight, float bounceEffect)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 640.0f;
			float middleX = (relX + relWidth / 2) * widthFactor;
			float middleY = (relY + relHeight / 2) * heightFactor;
			float retWidth = relWidth * widthFactor * bounceEffect;
			float retHeight = relHeight * heightFactor * bounceEffect;
			return new Rectangle(
				(int)Math.Round(middleX - retWidth / 2),
				(int)Math.Round(middleY - retHeight / 2),
				(int)Math.Round(retWidth),
				(int)Math.Round(retHeight));
		} // CalcRectangleWithBounce(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
		/// ratio for the image. The Rect will take same screen space in
		/// 16:9 and 4:3 modes. E.g. Buttons should be displayed this way.
		/// Should be used for 1024px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 768.0f;
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangleKeep4To3(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
		/// ratio for the image. The Rect will take same screen space in
		/// 16:9 and 4:3 modes. E.g. Buttons should be displayed this way.
		/// Should be used for 1024px width graphics.
		/// </summary>
		/// <param name="gfxRect">Gfx rectangle</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3(
			Rectangle gfxRect)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 768.0f;
			return new Rectangle(
				(int)Math.Round(gfxRect.X * widthFactor),
				(int)Math.Round(gfxRect.Y * heightFactor),
				(int)Math.Round(gfxRect.Width * widthFactor),
				(int)Math.Round(gfxRect.Height * heightFactor));
		} // CalcRectangleKeep4To3(gfxRect)

		/// <summary>
		/// Calc rectangle for 1600px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangle1600(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1600.0f;
			// keep height factor: float heightFactor = height / 1200.0f;
			float heightFactor = (height / 1200.0f);// / (aspectRatio / (16 / 9));
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangle1600(relX, relY, relWidth)
		
		/// <summary>
		/// Calc rectangle 2000px, just a helper to scale stuff down
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangle2000(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 2000.0f;
			float heightFactor = (height / 1500.0f);
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangle2000(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle keep 4 to 3 align bottom
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3AlignBottom(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor16To9 = height / 640.0f;
			float heightFactor4To3 = height / 768.0f;
			return new Rectangle(
				(int)(relX * widthFactor),
				(int)(relY * heightFactor16To9) -
				(int)Math.Round(relHeight * heightFactor4To3),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor4To3));
		} // CalcRectangleKeep4To3AlignBottom(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle keep 4 to 3 align bottom right
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3AlignBottomRight(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor16To9 = height / 640.0f;
			float heightFactor4To3 = height / 768.0f;
			return new Rectangle(
				(int)(relX * widthFactor) -
				(int)Math.Round(relWidth * widthFactor),
				(int)(relY * heightFactor16To9) -
				(int)Math.Round(relHeight * heightFactor4To3),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor4To3));
		} // CalcRectangleKeep4To3AlignBottomRight(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle centered with given height.
		/// This one uses relX and relY points as the center for our rect.
		/// The relHeight is then calculated and we align everything
		/// with help of gfxRect (determinating the width).
		/// Very useful for buttons, logos and other centered UI textures.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relHeight">Rel height</param>
		/// <param name="gfxRect">Gfx rectangle</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleCenteredWithGivenHeight(
			int relX, int relY, int relHeight, Rectangle gfxRect)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 640.0f;
			int rectHeight = (int)Math.Round(relHeight * heightFactor);
			// Keep aspect ratio
			int rectWidth = (int)Math.Round(
				gfxRect.Width * rectHeight / (float)gfxRect.Height);
			return new Rectangle(
				Math.Max(0, (int)Math.Round(relX * widthFactor) - rectWidth / 2),
				Math.Max(0, (int)Math.Round(relY * heightFactor) - rectHeight / 2),
				rectWidth, rectHeight);
		} // CalcRectangleCenteredWithGivenHeight(relX, relY, relHeight)
		#endregion

		#region Frames per second
		/// <summary>
		/// Fps
		/// </summary>
		/// <returns>Int</returns>
		public static int Fps
		{
			get
			{
				return fpsLastSecond;
			} // get
		} // Fps

		/*suxx, just use fpsInterpolated, it more accurate than overall checks
		/// <summary>
		/// Fps after 10 seconds
		/// </summary>
		/// <returns>Int</returns>
		public static int FpsAfter10Seconds
		{
			get
			{
				// Return 100 if 10 seconds have not passed yet.
				return TotalTime < 10 ? 100 :
					// Else return total frames devided by the game time in sec.
					(int)(totalFrameCount / TotalTime);
			} // get
		} // FpsAfter10Seconds
		 */

		/// <summary>
		/// Interpolated fps over the last 10 seconds.
		/// Obviously goes down if our framerate is low.
		/// </summary>
		private static float fpsInterpolated = 100.0f;

		/// <summary>
		/// Total frames
		/// </summary>
		/// <returns>Int</returns>
		public static int TotalFrames
		{
			get
			{
				return totalFrameCount;
			} // get
		} // TotalFrames
		#endregion

		#region Timing stuff
		/// <summary>
		/// Elapsed time this frame in ms
		/// </summary>
		/// <returns>Int</returns>
		public static float ElapsedTimeThisFrameInMilliseconds
		{
			get
			{
				// Max. return 500ms (0.5 sec) in case loading did take a while!
				return Math.Min(500, elapsedTimeThisFrameInMs);
			} // get
		} // ElapsedTimeThisFrameInMilliseconds

		/// <summary>
		/// Total time in seconds
		/// </summary>
		/// <returns>Int</returns>
		public static float TotalTime
		{
			get
			{
				return totalTimeMs / 1000.0f;
			} // get
		} // TotalTime

		/// <summary>
		/// Total time ms
		/// </summary>
		/// <returns>Float</returns>
		public static float TotalTimeMilliseconds
		{
			get
			{
				return totalTimeMs;
			} // get
		} // TotalTimeMilliseconds

		/// <summary>
		/// Move factor per second, when we got 1 fps, this will be 1.0f,
		/// when we got 100 fps, this will be 0.01f.
		/// </summary>
		public static float MoveFactorPerSecond
		{
			get
			{
				return elapsedTimeThisFrameInMs / 1000.0f;
			} // get
		} // MoveFactorPerSecond
		#endregion

		#region Camera
		/// <summary>
		/// World matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix WorldMatrix
		{
			get
			{
				return worldMatrix;
			} // get
			set
			{
				worldMatrix = value;
				// Update worldViewProj here?
			} // set
		} // WorldMatrix

		/// <summary>
		/// View matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix ViewMatrix
		{
			get
			{
				return viewMatrix;
			} // get
			set
			{
				// Set view matrix, usually only done in ChaseCamera.Update!
				viewMatrix = value;

				// Update camera pos and rotation, used all over the game!
				invViewMatrix = Matrix.Invert(viewMatrix);
				camPos = invViewMatrix.Translation;
				cameraRotation = Vector3.TransformNormal(
					new Vector3(0, 0, 1), invViewMatrix);
			} // set
		} // ViewMatrix

		/// <summary>
		/// Projection matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix ProjectionMatrix
		{
			get
			{
				return projectionMatrix;
			} // get
			set
			{
				projectionMatrix = value;
				// Update worldViewProj here?
			} // set
		} // ProjectionMatrix

		/// <summary>
		/// Camera pos, updated each frame in ViewMatrix!
		/// Public to allow easy access from everywhere, will be called a lot each
		/// frame, for example Model.Render uses this for distance checks.
		/// </summary>
		private static Vector3 camPos;

		/// <summary>
		/// Get camera position from inverse view matrix. Similar to method
		/// used in shader. Works only if ViewMatrix is correctly set.
		/// </summary>
		/// <returns>Vector 3</returns>
		public static Vector3 CameraPos
		{
			get
			{
				return camPos;
			} // get
		} // CameraPos

		/// <summary>
		/// Camera rotation, used to compare objects for visibility.
		/// </summary>
		private static Vector3 cameraRotation = new Vector3(0, 0, 1);

		/// <summary>
		/// Camera rotation
		/// </summary>
		/// <returns>Vector 3</returns>
		public static Vector3 CameraRotation
		{
			get
			{
				return cameraRotation;
			} // get
		} // CameraRotation

		/// <summary>
		/// Remember inverse view matrix.
		/// </summary>
		private static Matrix invViewMatrix;

		/// <summary>
		/// Inverse view matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix InverseViewMatrix
		{
			get
			{
				return invViewMatrix;//Matrix.Invert(ViewMatrix);
			} // get
		} // InverseViewMatrix

		/// <summary>
		/// View projection matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix ViewProjectionMatrix
		{
			get
			{
				return ViewMatrix * ProjectionMatrix;
			} // get
		} // ViewProjectionMatrix

		/// <summary>
		/// World view projection matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix WorldViewProjectionMatrix
		{
			get
			{
				return WorldMatrix * ViewMatrix * ProjectionMatrix;
			} // get
		} // WorldViewProjectionMatrix
		#endregion

		#region Render states
		/// <summary>
		/// Alpha blending
		/// </summary>
		/// <returns>Bool</returns>
		public static bool AlphaBlending
		{
			get
			{
				throw new InvalidOperationException(
					"Sorry, it is not supported to get the current state " +
					"of alpha blending and this getter was only implemented " +
					"to get rid of the FxCop warning ^^");
			} // get
			set
			{
				if (value)
				{
					device.RenderState.AlphaBlendEnable = true;
					device.RenderState.SourceBlend = Blend.SourceAlpha;
					device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
				} // if (value)
				else
					device.RenderState.AlphaBlendEnable = false;
			} // set
		} // AlphaBlending

		/// <summary>
		/// Alpha modes
		/// </summary>
		public enum AlphaMode
		{
			/// <summary>
			/// Disable alpha blending for this (even if the texture has alpha)
			/// </summary>
			DisableAlpha,
			/// <summary>
			/// Default alpha mode: SourceAlpha and InvSourceAlpha, which does
			/// nothing if the texture does not have alpha, else it just displays
			/// it as it is (with transparent pixels).
			/// </summary>
			Default,
			/// <summary>
			/// Use source alpha one mode, this is the default mode for lighting
			/// effects.
			/// </summary>
			SourceAlphaOne,
			/// <summary>
			/// One one alpha mode.
			/// </summary>
			OneOne,
		} // enum AlphaMode

		/// <summary>
		/// Current alpha mode
		/// </summary>
		/// <returns>Alpha modes</returns>
		public static AlphaMode CurrentAlphaMode
		{
			get
			{
				throw new InvalidOperationException(
					"Sorry, it is not supported to get the current " +
					"alpha mode and this getter was only implemented " +
					"to get rid of the FxCop warning ^^");
			} // get
			set
			{
				switch (value)
				{
					case AlphaMode.DisableAlpha:
						device.RenderState.SourceBlend = Blend.Zero;
						device.RenderState.DestinationBlend = Blend.One;
						break;
					case AlphaMode.Default:
						device.RenderState.SourceBlend = Blend.SourceAlpha;
						device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
						break;
					case AlphaMode.SourceAlphaOne:
						device.RenderState.SourceBlend = Blend.SourceAlpha;
						device.RenderState.DestinationBlend = Blend.One;
						break;
					case AlphaMode.OneOne:
						device.RenderState.SourceBlend = Blend.One;
						device.RenderState.DestinationBlend = Blend.One;
						break;
				} // switch (value)
			} // set
		} // CurrentAlphaMode
		#endregion
		#endregion

		#region Constructor
		/// <summary>
		/// Create base game
		/// </summary>
		/// <param name="setWindowsTitle">Set windows title</param>
		protected BaseGame(string setWindowsTitle)
		{
			// Set graphics
			graphicsManager = new GraphicsDeviceManager(this);

			int resolutionWidth = GameSettings.Default.ResolutionWidth;
			int resolutionHeight = GameSettings.Default.ResolutionHeight;
			// Use current desktop resolution if autodetect is selected.
			if (resolutionWidth <= 0 ||
				resolutionHeight <= 0)
			{
				resolutionWidth =
					GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
				resolutionHeight =
					GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			} // if (resolutionWidth)

#if XBOX360
			// Telling the Xbox 360 which resolution we want does not matter
			graphicsManager.IsFullScreen = true;
			// The Xbox 360 is fast enough for some nice multisampling
			//leave default: graphicsManager.PreferMultiSampling = true;
		
			// Tell the Xbox 360 the resolution anyways, the viewport might
			// be just 800x600 else!
			graphicsManager.PreferredBackBufferWidth =
				GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			graphicsManager.PreferredBackBufferHeight =
				GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else
			graphicsManager.PreferredBackBufferWidth = resolutionWidth;
			graphicsManager.PreferredBackBufferHeight = resolutionHeight;
			graphicsManager.IsFullScreen = GameSettings.Default.Fullscreen;

			/*just leave default, avoids problem with RenderToTexture and
			 * uses multisampling anyway if user sets it for his graphic card.
			// Try to use multisampling, looks much better, but hurts performance!
			this.graphicsManager.PreferMultiSampling = true;
			/*old:
			// Make sure multisampling is always off, this is important
			// because it does not work on older hardware and it causes
			// very many problems with the RenderToTexture class!
			// Note: Even if you enable it, it does not mean it can be used,
			// often the current mode does not support multisampling or the
			// graphic hardware is too old.
			this.graphicsManager.PreferMultiSampling = false;
			 */
#endif

//#if DEBUG
			// Disable vertical retrace to get highest framerates possible for
			// testing performance.
			graphicsManager.SynchronizeWithVerticalRetrace = false;
//#endif
			// Update as fast as possible, do not use fixed time steps.
			// The whole game is designed this way, if you remove this line
			// the car will not behave normal anymore!
			this.IsFixedTimeStep = false;
			
			// Init content manager
			content = base.Content;//obs: new ContentManager(this.Services);

#if !XBOX360
			// Make sure we use the current directory, not the executing directory,
			// which is not the same for unit tests.
			// Also include the content directory to make the paths easier and shorter
			content.RootDirectory = Directory.GetCurrentDirectory();
#endif

			// Update windows title (used for unit testing)
			this.Window.Title = setWindowsTitle;
			remWindowsTitle = setWindowsTitle;

			// Disable multisampling for huge resolutions (can run out of memory)
			this.graphicsManager.PreferMultiSampling = false;
		} // BaseGame(setWindowsTitle)

		/// <summary>
		/// Empty constructor for the designer support
		/// </summary>
		protected BaseGame()
			: this("Game")
		{
		} // BaseGame()

		/// <summary>
		/// Initialize
		/// </summary>
		protected override void Initialize()
		{
#if !XBOX360
			// Add screenshot capturer. Note: Don't do this in constructor,
			// we need the correct window name for screenshots!
			this.Components.Add(new ScreenshotCapturer(this));
#endif

			base.Initialize();
			
			// Set device and resolution
			device = graphicsManager.GraphicsDevice;
			width = device.Viewport.Width;//Window.ClientBounds.Width;
			height = device.Viewport.Height;//Window.ClientBounds.Height;

			RenderToTexture.InitializeDepthBufferFormatAndMultisampling(
				graphicsManager.PreferredDepthStencilFormat);

			// Update resolution if it changes
			Window.ClientSizeChanged += new EventHandler(Window_ClientSizeChanged);
			graphicsManager.DeviceReset += new EventHandler(graphics_DeviceReset);
			graphics_DeviceReset(null, EventArgs.Empty);

			// Create matrices for our shaders, this makes it much easier
			// to manage all the required matrices since there is no fixed
			// function support and theirfore no Device.Transform class.
			WorldMatrix = Matrix.Identity;
			aspectRatio = (float)width / (float)height;
			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				FieldOfView, aspectRatio, NearPlane, FarPlane);

			// ViewMatrix is updated in camera class
			ViewMatrix = Matrix.CreateLookAt(
				new Vector3(0, 0, 250), Vector3.Zero, Vector3.Up);

			// Init global manager classes, which will be used all over the place ^^
			lineManager2D = new LineManager2D();
			lineManager3D = new LineManager3D();
			ui = new UIRenderer();
		} // Initialize()

		/// <summary>
		/// Graphics device reset
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">E</param>
		void graphics_DeviceReset(object sender, EventArgs e)
		{
			// Re-Set device
			device = graphicsManager.GraphicsDevice;

			// Restore z buffer state
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
			// Set u/v addressing back to wrap
			BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
			BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
			// Restore normal alpha blending
			BaseGame.CurrentAlphaMode = BaseGame.AlphaMode.Default;

			// Set 128 and greate alpha compare for Model.Render
			BaseGame.Device.RenderState.ReferenceAlpha = 128;
			BaseGame.Device.RenderState.AlphaFunction = CompareFunction.Greater;

			// Recreate all render-targets
			RenderToTexture.HandleDeviceReset();
		} // graphics_DeviceReset(sender, e)

		/// <summary>
		/// Window client size changed
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">E</param>
		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			// Update width and height
			width = device.Viewport.Width;//Window.ClientBounds.Width;
			height = device.Viewport.Height;//Window.ClientBounds.Height;
			aspectRatio = (float)width / (float)height;
			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				FieldOfView, aspectRatio, NearPlane, FarPlane);
		} // Window_ClientSizeChanged(sender, e)
		#endregion

		#region Helper methods for 3d-calculations
		/// <summary>
		/// Epsilon (1/1000000) for comparing stuff which is nearly equal.
		/// </summary>
		public const float Epsilon = 0.000001f;

		/// <summary>
		/// Convert 3D vector to 2D vector, this is kinda the oposite of
		/// GetScreenPlaneVector (not shown here). This can be useful for user
		/// input/output, because we will often need the actual position on screen
		/// of an object in 3D space from the users view to handle it the right
		/// way. Used for lens flare and asteroid optimizations.
		/// </summary>
		/// <param name="point">3D world position</param>
		/// <return>Resulting 2D screen position</return>
		public static Point Convert3DPointTo2D(Vector3 point)
		{
			Vector4 result4 = Vector4.Transform(point,
				ViewProjectionMatrix);

			if (result4.W == 0)
				result4.W = BaseGame.Epsilon;
			Vector3 result = new Vector3(
				result4.X / result4.W,
				result4.Y / result4.W,
				result4.Z / result4.W);

			// Output result from 3D to 2D
			return new Point(
				(int)Math.Round(+result.X * (width / 2)) + (width / 2),
				(int)Math.Round(-result.Y * (height / 2)) + (height / 2));
		} // Convert3DPointTo2D(point)

		/// <summary>
		/// Is point in front of camera?
		/// </summary>
		/// <param name="point">Position to check.</param>
		/// <returns>Bool</returns>
		public static bool IsInFrontOfCamera(Vector3 point)
		{
			Vector4 result = Vector4.Transform(
				new Vector4(point.X, point.Y, point.Z, 1),
				ViewProjectionMatrix);

			// Is result in front?
			return result.Z > result.W - NearPlane;
		} // IsInFrontOfCamera(point)

		/// <summary>
		/// Helper to check if a 3d-point is visible on the screen.
		/// Will basically do the same as IsInFrontOfCamera and Convert3DPointTo2D,
		/// but requires less code and is faster. Also returns just an bool.
		/// Will return true if point is visble on screen, false otherwise.
		/// Use the offset parameter to include points into the screen that are
		/// only a couple of pixel outside of it.
		/// </summary>
		/// <param name="point">Point</param>
		/// <param name="checkOffset">Check offset in percent of total
		/// screen</param>
		/// <returns>Bool</returns>
		public static bool IsVisible(Vector3 point, float checkOffset)
		{
			Vector4 result = Vector4.Transform(
				new Vector4(point.X, point.Y, point.Z, 1),
				ViewProjectionMatrix);

			// Point must be in front of camera, else just skip everything.
			if (result.Z > result.W - NearPlane)
			{
				Vector2 screenPoint = new Vector2(
					result.X / result.W, result.Y / result.W);

				// Change checkOffset depending on how depth we are into the scene
				// for very near objects (z < 5) pass all tests!
				// for very far objects (z >> 5) only pass if near to +- 1.0f
				float zDist = Math.Abs(result.Z);
				if (zDist < 5.0f)
					return true;
				checkOffset = 1.0f + (checkOffset / zDist);

				return
					screenPoint.X >= -checkOffset && screenPoint.X <= +checkOffset &&
					screenPoint.Y >= -checkOffset && screenPoint.Y <= +checkOffset;
			} // if (result.z)

			// Point is not in front of camera, return false.
			return false;
		} // IsVisible(point)
		#endregion

		#region Line helper methods
		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPoint">Start point</param>
		/// <param name="endPoint">End point</param>
		/// <param name="color">Color</param>
		public static void DrawLine(Point startPoint, Point endPoint, Color color)
		{
			lineManager2D.AddLine(startPoint, endPoint, color);
		} // DrawLine(startPoint, endPoint, color)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPoint">Start point</param>
		/// <param name="endPoint">End point</param>
		public static void DrawLine(Point startPoint, Point endPoint)
		{
			lineManager2D.AddLine(startPoint, endPoint, Color.White);
		} // DrawLine(startPoint, endPoint)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="color">Color</param>
		public static void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
		{
			lineManager3D.AddLine(startPos, endPos, color);
		} // DrawLine(startPos, endPos, color)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="startColor">Start color</param>
		/// <param name="endColor">End color</param>
		public static void DrawLine(Vector3 startPos, Vector3 endPos,
			Color startColor, Color endColor)
		{
			lineManager3D.AddLine(startPos, startColor, endPos, endColor);
		} // DrawLine(startPos, endPos, startColor)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		public static void DrawLine(Vector3 startPos, Vector3 endPos)
		{
			lineManager3D.AddLine(startPos, endPos, Color.White);
		} // DrawLine(startPos, endPos)

		/// <summary>
		/// Flush line manager 2D. Renders all lines and allows more lines
		/// to be rendered. Used to render lines into textures and stuff.
		/// </summary>
		public static void FlushLineManager2D()
		{
			lineManager2D.Render();
		} // FlushLineManager2D()

		/// <summary>
		/// Flush line manager 3D. Renders all lines and allows more lines
		/// to be rendered.
		/// </summary>
		public static void FlushLineManager3D()
		{
			lineManager3D.Render();
		} // FlushLineManager3D()
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// Update all input states
			Input.Update();

			lastFrameTotalTimeMs = totalTimeMs;
			elapsedTimeThisFrameInMs =
				(float)gameTime.ElapsedRealTime.TotalMilliseconds;
			totalTimeMs += elapsedTimeThisFrameInMs;

			// Make sure elapsedTimeThisFrameInMs is never 0
			if (elapsedTimeThisFrameInMs <= 0)
				elapsedTimeThisFrameInMs = 0.001f;

			// Increase frame counter for FramesPerSecond
			frameCountThisSecond++;
			totalFrameCount++;

			// One second elapsed?
			if (totalTimeMs - startTimeThisSecond > 1000.0f)
			{
				// Calc fps
				fpsLastSecond = (int)(frameCountThisSecond * 1000.0f /
					(totalTimeMs - startTimeThisSecond));
				// Reset startSecondTick and repaintCountSecond
				startTimeThisSecond = totalTimeMs;
				frameCountThisSecond = 0;

				fpsInterpolated =
					MathHelper.Lerp(fpsInterpolated, fpsLastSecond, 0.1f);

				// Check out if our framerate is running very low. Then we can improve
				// rendering by reducing the number of objects we draw.
				if (fpsInterpolated < 5)
					Model.MaxViewDistance = 50;
				else if (fpsInterpolated < 12)
					Model.MaxViewDistance = 70;
				else if (fpsInterpolated < 16)
					Model.MaxViewDistance = 90;
				else if (fpsInterpolated < 20)
					Model.MaxViewDistance = 120;
				else if (fpsInterpolated < 25)
					Model.MaxViewDistance = 150;
				else if (fpsInterpolated < 30 ||
					HighDetail == false)
					Model.MaxViewDistance = 175;
			} // if (Math.Abs)

			// Update sound and music
			Sound.Update();
		} // Update()
		#endregion

		#region On activated and on deactivated
		// Check if app is currently active
		static bool isAppActive = true;
		/// <summary>
		/// Is app active
		/// </summary>
		/// <returns>Bool</returns>
		public static bool IsAppActive
		{
			get
			{
				return isAppActive;
			} // get
		} // IsAppActive

		/// <summary>
		/// On activated
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="args">Arguments</param>
		protected override void OnActivated(object sender, EventArgs args)
		{
			base.OnActivated(sender, args);
			isAppActive = true;
		} // OnActivated(sender, args)

		/// <summary>
		/// On deactivated
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="args">Arguments</param>
		protected override void OnDeactivated(object sender, EventArgs args)
		{
			base.OnDeactivated(sender, args);
			isAppActive = false;
		} // OnDeactivated(sender, args)
		#endregion

		#region Draw
#if !DEBUG
		int renderLoopErrorCount = 0;
#endif
		/// <summary>
		/// Draw
		/// </summary>
		/// <param name="gameTime">Game time</param>
		protected override void Draw(GameTime gameTime)
		{
			try
			{
				// Clear anyway, makes unit tests easier and fixes problems if
				// we don't have the z buffer cleared (some issues with line
				// rendering might happen else). Performance drop is not significant!
				ClearBackground();

				// Handle custom user render code
				Render();

				// Render all models we remembered this frame.
				meshRenderManager.Render();

				// Render all 3d lines
				lineManager3D.Render();

				// Render UI and font texts, this also handles all collected
				// screen sprites (on top of 3d game code)
				UIRenderer.Render(lineManager2D);

				PostUIRender();

				ui.RenderTextsAndMouseCursor();
			} // try
			// Only catch exceptions here in release mode, when debugging
			// we want to see the source of the error. In release mode
			// we want to play and not be annoyed by some bugs ^^
#if !DEBUG
			catch (Exception ex)
			{
				Log.Write("Render loop error: " + ex.ToString());
				if (renderLoopErrorCount++ > 100)
					throw;
			} // catch
#endif
			finally
			{
				// Dummy block to prevent error in debug mode
			} // finally

			base.Draw(gameTime);
		} // Draw()
		#endregion

		#region Render
		/// <summary>
		/// Render delegate for rendering methods, also used for many other
		/// methods.
		/// </summary>
		public delegate void RenderHandler();

		/// <summary>
		/// Render
		/// </summary>
		protected virtual void Render()
		{
			// Overwrite this for your custom render code ..
		} // Render()

		/// <summary>
		/// Post user interface rendering, in case we need it.
		/// Used for rendering the car selection 3d stuff after the UI.
		/// </summary>
		protected virtual void PostUIRender()
		{
			// Overwrite this for your custom render code ..
		} // PostUIRender()

		/// <summary>
		/// Clear background
		/// </summary>
		public static void ClearBackground()
		{
			//unsure if it clears depth correctly: Device.Clear(BackgroundColor);
			Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
				BackgroundColor, 1.0f, 0);
		} // ClearBackground()
		#endregion
	} // class BaseGame
} // namespace RacingGame.Graphics
