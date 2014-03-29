// Project: RacingGame, File: Program.cs
// Namespace: RacingGame, Class: Program
// Path: C:\code\RacingGame, Author: Abi
// Code lines: 65, Size of file: 1,50 KB
// Creation date: 07.09.2006 03:58
// Last modified: 20.10.2006 15:54
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using RacingGame.Helpers;
using RacingGame.Properties;
#endregion

namespace RacingGame
{
	/// <summary>
	/// Program
	/// </summary>
	static class Program
	{
		#region RestartGameAfterOptionsChange
		/// <summary>
		/// Restart if user changes something in options.
		/// </summary>
		public static bool RestartGameAfterOptionsChange = false;
		#endregion

		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// <param name="args">Arguments</param>
#if DEBUG
		static void Main(string[] args)
		{
			StartGame();
			//UnitTests.StartTest(args);
#else
		static void Main()
		{
			StartGame();
#endif

			// Make sure settings are saved (will only be executed if any setting
			// changed).
			GameSettings.Save();

#if !XBOX360
			// Restarting does only work on the windows platform, isn't required
			// for the Xbox 360 anyways.
			if (RestartGameAfterOptionsChange)
				System.Diagnostics.Process.Start("RacingGame.exe");
#endif
		} // Main(args)
		#endregion

		#region StartGame
		/// <summary>
		/// Start game, is in a seperate method for 2 reasons: We want to catch
		/// any exceptions here, but not for the unit tests and we also allow
		/// the unit tests to call this method if we don't want to unit test
		/// in debug mode.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification = "App should not crash in release mode. User will " +
			"not understand why, instead show user exception in log file!")]
		public static void StartGame()
		{
			// Normal start without exception checking in debug mode
#if DEBUG
			using (RacingGameManager game = new RacingGameManager())
			{
				game.Run();
			} // using (game)
#elif XBOX360
			// On the Xbox 360 we can't display message boxes.
			using (RacingGameManager game = new RacingGameManager())
			{
				game.Run();
			} // using (game)
#else
			// Can be useful if you don't want to crash in the users face,
			// show him a more meaningful exception (see log file, message boxes
			// are not supported right now).
			// But FxCop rules do not really like catching general exceptions.
			try
			{
				using (RacingGameManager game = new RacingGameManager())
				{
					game.Run();
				} // using (game)
			} // try
			catch (Exception ex)
			{
				Log.Write("Fatal error, application crashed: " + ex.ToString());
			} // catch
#endif
		} // StartGame()
		#endregion				
	} // class Program
} // namespace RacingGame
