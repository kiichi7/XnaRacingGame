// Project: RacingGame, File: RacingGameManager.cs
// Namespace: RacingGame, Class: RacingGame
// Path: C:\code\RacingGame, Author: Abi
// Code lines: 410, Size of file: 10,56 KB
// Creation date: 07.09.2006 05:56
// Last modified: 20.10.2006 16:21
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
using RacingGame.GameLogic;
using RacingGame.GameScreens;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Landscapes;
using RacingGame.Sounds;
using Model = RacingGame.Graphics.Model;
using Texture = RacingGame.Graphics.Texture;
using RacingGame.Properties;
using RacingGame.Shaders;
#endregion

namespace RacingGame
{
	/// <summary>
	/// This is the main entry class our game. Handles all game screens,
	/// which themself handle all the game logic.
	/// As you can see this class is very simple, which is really cool.
	/// </summary>
	public class RacingGameManager : BaseGame
	{
		#region Variables
		/// <summary>
		/// Game screens stack. We can easily add and remove game screens
		/// and they follow the game logic automatically. Very cool.
		/// </summary>
		private static Stack<IGameScreen> gameScreens = new Stack<IGameScreen>();

		/// <summary>
		/// Player for the game, also allows us to control the car and contains
		/// all the required code for the car physics, chase camera and basic
		/// player values and the game time because this is the top class
		/// of many derived classes. Player, car and camera position is set
		/// when the game starts depending on the selected level.
		/// </summary>
		private static Player player = new Player(new Vector3(0, 0, 0));

		/// <summary>
		/// Car model and selection plate for the car selection screen.
		/// </summary>
		private static Model carModel = null,
			carSelectionPlate = null;

		/// <summary>
		/// Car textures we exchange for our car model.
		/// </summary>
		private static Texture[] carTextures = null;

		/// <summary>
		/// The player can select between the 3 cars: 0 (white), 1 (red) and
		/// 2 (yellow).
		/// </summary>
		public static int currentCarNumber = 0;

		/// <summary>
		/// The player can also select a car color, which will be used to
		/// recolor the car. Looks best for the first car (white).
		/// </summary>
		public static int currentCarColor;// Color carColor = Color.White;

		/// <summary>
		/// Helper texture for color selection
		/// </summary>
		public static Texture colorSelectionTexture = null;

		/// <summary>
		/// Material for brake tracks on the road.
		/// </summary>
		private static Material brakeTrackMaterial = null;

		/// <summary>
		/// Car colors for the car selection screen.
		/// </summary>
		private static readonly List<Color> carColors = new List<Color>(
			new Color[]
			{
				Color.White,
				Color.Yellow,
				Color.Blue,
				Color.Purple,
				Color.Red,
				Color.Green,
				Color.Teal,
				Color.Gray,
				Color.Brown,
				new Color(24, 24, 24),
				Color.SeaGreen,
			});

		/// <summary>
		/// Colors for car type 0 (RacerCar.dds), only used for displaying the
		/// color box in the car selection screen. The car color in the game and
		/// on the car will be computed by a hue color change formula.
		/// </summary>
		private static readonly List<Color> carType0Colors = new List<Color>(
			new Color[]
			{
				// Hue: Color: RGB
  			// 0.0: Orange (normal): 250, 80, 0
				new Color(250, 80, 0),
  			// 0.1: Red: 252, 6, 71
				new Color(252, 6, 71),
  			// 0.2: Pink: 208, 0, 154
				new Color(208, 0, 154),
  			// 0.3: Purple: 134, 0, 224
				new Color(134, 0, 224),
  			// 0.4: Blue: 51, 22, 255
				new Color(51, 22, 255),
  			// 0.5: LightBlue: 0, 101, 242
				new Color(0, 101, 242),
  			// 0.6: Teal: 0, 182, 185
				new Color(0, 182, 185),
  			// 0.7: TealGreen: 0, 240, 104
				new Color(0, 240, 104),
  			// 0.8: LightGreen: 48, 255, 25
				new Color(48, 255, 25),
  			// 0.9: GreenYellow: 131, 225, 0
				new Color(131, 225, 0),
  			// 1.0: Yellow: 207, 158, 0
				new Color(207, 158, 0),
			});

		/// <summary>
		/// Colors for car type 1 (RacerCar2.dds), only used for displaying the
		/// color box in the car selection screen. The car color in the game and
		/// on the car will be computed by a hue color change formula.
		/// </summary>
		private static readonly List<Color> carType1Colors = new List<Color>(
			new Color[]
			{
				// Hue: Color: RGB
  			// 0.0: Yellow+Blue (normal): 227, 213, 41
				new Color(227, 213, 41),
  			// 0.1: Orange+Teal: 255, 144, 64
				new Color(255, 144, 64),
  			// 0.2: Pink+Green: 255, 84, 120
				new Color(255, 84, 120),
  			// 0.3: LightPurple+Green: 249, 46, 187
				new Color(249, 46, 187),
  			// 0.4: DarkPuple+DarkGreen: 189, 45, 245
				new Color(189, 45, 245),
  			// 0.5: Blue+DarkBrown: 120, 80, 255
				new Color(120, 80, 255),
  			// 0.6: LightBlue+Red: 67, 145, 255
				new Color(67, 145, 255),
  			// 0.7: LightTeal+DarkRed: 41, 209, 229
				new Color(41, 209, 229),
  			// 0.8: GreenTeal+Purple: 54, 255, 166
				new Color(54, 255, 166),
  			// 0.9: Green+DarkPurple: 100, 255, 97
				new Color(100, 255, 97),
  			// 1.0: GreenYellow: 166, 255, 52
				new Color(166, 255, 52),
			});

		/// <summary>
		/// Car colors for the car selection screen.
		/// </summary>
		public static List<Color> CarColors
		{
			get
			{
				return currentCarNumber == 0 ? carType0Colors :
					currentCarNumber == 1 ? carType1Colors :
					carColors;
			} // get
		} // CarColors

		/// <summary>
		/// Landscape we are currently using.
		/// </summary>
		private static Landscape landscape = null;
		
		/// <summary>
		/// Level we use for our track and landscape
		/// </summary>
		public enum Level
		{
			Beginner,
			Advanced,
			Expert,
		} // enum Level
		
		/// <summary>
		/// Load level
		/// </summary>
		/// <param name="setNewLevel">Set new level</param>
		public static void LoadLevel(Level setNewLevel)
		{
			landscape.ReloadLevel(setNewLevel);
		} // LoadLevel(setNewLevel)
		#endregion

		#region Properties
		/// <summary>
		/// In menu
		/// </summary>
		/// <returns>Bool</returns>
		public static bool InMenu
		{
			get
			{
				return gameScreens.Count > 0 &&
					gameScreens.Peek().GetType() != typeof(GameScreen);
			} // get
		} // InMenu

		/// <summary>
		/// In game?
		/// </summary>
		public static bool InGame
		{
			get
			{
				return gameScreens.Count > 0 &&
					gameScreens.Peek().GetType() == typeof(GameScreen);
			} // get
		} // InGame

		/// <summary>
		/// ShowMouseCursor
		/// </summary>
		/// <returns>Bool</returns>
		public static bool ShowMouseCursor
		{
			get
			{
				// Only if not in Game, not in logo or splash screen!
				return gameScreens.Count > 0 &&
					gameScreens.Peek().GetType() != typeof(GameScreen) &&
					gameScreens.Peek().GetType() != typeof(SplashScreen) &&
					gameScreens.Peek().GetType() != typeof(LogoScreens);
			} // get
		} // ShowMouseCursor

		/// <summary>
		/// In car selection screen
		/// </summary>
		/// <returns>Bool</returns>
		public static bool InCarSelectionScreen
		{
			get
			{
				return gameScreens.Count > 0 &&
					gameScreens.Peek().GetType() == typeof(CarSelection);
			} // get
		} // InCarSelectionScreen

		/// <summary>
		/// Player for the game, also allows us to control the car and contains
		/// all the required code for the car physics, chase camera and basic
		/// player values and the game time because this is the top class
		/// of many derived classes.
		/// Easy access here with a static property in case we need the player
		/// somewhere in the game.
		/// </summary>
		/// <returns>Player</returns>
		public static Player Player
		{
			get
			{
				return player;
			} // get
		} // Player

		/// <summary>
		/// Car model
		/// </summary>
		/// <returns>Model</returns>
		public static Model CarModel
		{
			get
			{
				return carModel;
			} // get
		} // CarModel

		/// <summary>
		/// Car color
		/// </summary>
		/// <returns>Color</returns>
		public static Color CarColor
		{
			get
			{
				return CarColors[currentCarColor % CarColors.Count];
			} // get
		} // CarColor

		/// <summary>
		/// Number of car colors
		/// </summary>
		/// <returns>Int</returns>
		public static int NumberOfCarColors
		{
			get
			{
				return CarColors.Count;
			} // get
		} // NumberOfCarColors

		/// <summary>
		/// Number of car texture types
		/// </summary>
		/// <returns>Int</returns>
		public static int NumberOfCarTextureTypes
		{
			get
			{
				return carTextures.Length;
			} // get
		} // NumberOfCarTextureTypes

		/// <summary>
		/// Car texture
		/// </summary>
		/// <param name="carNumber">Car number</param>
		/// <returns>Texture</returns>
		public static Texture CarTexture(int carNumber)
		{
			return carTextures[carNumber % carTextures.Length];
		} // CarTexture(carNumber)

		/// <summary>
		/// Brake track material
		/// </summary>
		/// <returns>Material</returns>
		public static Material BrakeTrackMaterial
		{
			get
			{
				return brakeTrackMaterial;
			} // get
		} // BrakeTrackMaterial

		/// <summary>
		/// Car selection plate
		/// </summary>
		/// <returns>Model</returns>
		public static Model CarSelectionPlate
		{
			get
			{
				return carSelectionPlate;
			} // get
		} // CarSelectionPlate

		/// <summary>
		/// Landscape we are currently using, used for several things (menu
		/// background, the game, some other classes outside the landscape class).
		/// </summary>
		/// <returns>Landscape</returns>
		public static Landscape Landscape
		{
			get
			{
				return landscape;
			} // get
		} // Landscape
		#endregion

		#region Constructor
		/// <summary>
		/// Create Racing game
		/// </summary>
		public RacingGameManager()
			: base("RacingGame")
		{
			// Start playing the menu music
			Sound.Play(Sound.Sounds.MenuMusic);

			// Create main menu at our main entry point
			gameScreens.Push(new MainMenu());

			// But start with splash screen, if user clicks or presses Start,
			// we are back in the main menu.
			gameScreens.Push(new SplashScreen());

			// And finally add the first time logos we see when starting the game.
			gameScreens.Push(new LogoScreens());
		} // RacingGame()

		/// <summary>
		/// Create Racing game for unit tests, not used for anything else.
		/// </summary>
		public RacingGameManager(string unitTestName)
			: base(unitTestName)
		{
			// Don't add game screens here
		} // RacingGame(unitTestName)

		/// <summary>
		/// Load car stuff
		/// </summary>
		protected override void Initialize()
		{
 			base.Initialize();

			// Load models
			carModel = new Model("Car");
			carSelectionPlate = new Model("CarSelectionPlate");

			// Load landscape
			landscape = new Landscape(Level.Beginner);

			// Load textures, first one is grabbed from the imported one through
			// the car.x model, the other two are loaded seperately.
			carTextures = new Texture[3];
			carTextures[0] = new Texture("RacerCar%0");
			carTextures[1] = new Texture("RacerCar2");
			carTextures[2] = new Texture("RacerCar3");
			colorSelectionTexture = new Texture("ColorSelection");
			brakeTrackMaterial = new Material("track");

      //For testing:
			//Directly start the game and load a certain level, this code is
      // only used in the debug mode. Remove it when you are don with
      // testing! If you press Esc you will also quit the game and not
      // just end up in the menu again.
      //gameScreens.Clear();
      //gameScreens.Push(new GameScreen());
		} // LoadCarStuff()
		#endregion

		#region Add game screen
		/// <summary>
		/// Add game screen
		/// </summary>
		/// <param name="gameScreen">Game screen</param>
		public static void AddGameScreen(IGameScreen gameScreen)
		{
			// Play sound for screen click
			Sound.Play(Sound.Sounds.ScreenClick);

			// Add the game screen
			gameScreens.Push(gameScreen);
		} // AddGameScreen(gameScreen)
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			// Update game engine
			base.Update(gameTime);

			// Update player and game logic
			player.Update();
		} // Update()
		#endregion

		#region Render
		/// <summary>
		/// Render
		/// </summary>
		protected override void Render()
		{
			// No more game screens?
			if (gameScreens.Count == 0)
			{
				// Before quiting, stop music and play crash sound :)
				Sound.PlayCrashSound(true);
				Sound.StopMusic();

				// Then quit
				Exit();
				return;
			} // if (gameScreens.Count)

			// Handle current screen
			if (gameScreens.Peek().Render())
			{
				// If this was the options screen and the resolution has changed,
				// restart the game!
				if (gameScreens.Peek().GetType() == typeof(Options) &&
					(BaseGame.Width != GameSettings.Default.ResolutionWidth ||
					BaseGame.Height != GameSettings.Default.ResolutionHeight))
				{
					// Restart if resolution was changed!
					Program.RestartGameAfterOptionsChange = true;
					this.Exit();
				} // if

				// Play sound for screen back
				Sound.Play(Sound.Sounds.ScreenBack);

				gameScreens.Pop();
			} // if (gameScreens.Peek)
		} // Render()

		/// <summary>
		/// Post user interface rendering, in case we need it.
		/// Used for rendering the car selection 3d stuff after the UI.
		/// </summary>
		protected override void PostUIRender()
		{
			// Enable depth buffer again
			BaseGame.Device.RenderState.DepthBufferEnable = true;

			// Currently in car selection screen?
			if (gameScreens.Count > 0 &&
				gameScreens.Peek().GetType() == typeof(CarSelection))
				((CarSelection)gameScreens.Peek()).PostUIRender();

			// Do menu shader after everything
			if (RacingGameManager.InMenu &&
				PostScreenMenu.Started)
				UI.PostScreenMenuShader.Show();
		} // PostUIRender()
		#endregion
	} // class RacingGameManager
} // namespace RacingGame
