using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using osu;
using osu.Enums;
using osu.Memory.Objects.Player.Beatmaps;
using osu_rx.Configuration;
using osu_rx.Core.Relax;
using osu_rx.Core.Timewarp;
using SimpleDependencyInjection;
using WindowsInput.Native;

namespace osu_rx
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Program.osuManager = new OsuManager();
			if (Program.osuManager.Initialize())
			{
				Program.configManager = new ConfigManager();
				DependencyContainer.Cache<OsuManager>(Program.osuManager);
				DependencyContainer.Cache<ConfigManager>(Program.configManager);
				Program.relax = new Relax();
				Program.timewarp = new Timewarp();
				Program.defaultConsoleTitle = Console.Title;
				if (Program.configManager.UseCustomWindowTitle)
				{
					Console.Title = Program.configManager.CustomWindowTitle;
				}
				Program.DrawMainMenu();
				return;
			}
			Console.Clear();
			Console.WriteLine("osu!rx failed to initialize:\n");
			Console.WriteLine("Memory scanning failed! Try restarting osu!, osu!rx or your computer to fix this issue.");
			Console.WriteLine("If that didn't help, then report this on GitHub/MPGH.");
			Console.WriteLine("Please include as much info as possible (OS version, hack version, build source, debug info, etc.).");
			Console.WriteLine("\n\nDebug Info:\n");
			Console.WriteLine(Program.osuManager.DebugInfo);
			for (;;)
			{
				Thread.Sleep(1000);
			}
		}

		private static void DrawMainMenu()
		{
			string text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			text = text.Remove(text.LastIndexOf(".0"));
			Console.Clear();
			Console.WriteLine("osu!rx v" + text + " (MPGH release)");
			Console.WriteLine("\n---Main Menu---");
			Console.WriteLine("\n1. Start");
			Console.WriteLine("2. Settings\n");
			Console.WriteLine("3. Support development of osu!rx");
			switch (Console.ReadKey(true).Key)
			{
			case ConsoleKey.D1:
				Program.DrawPlayer();
				return;
			case ConsoleKey.D2:
				Program.DrawSettings();
				return;
			case ConsoleKey.D3:
				Program.DrawSupportInfo();
				return;
			default:
				Program.DrawMainMenu();
				return;
			}
		}

		private static void DrawPlayer()
		{
			bool shouldExit = false;
			Task.Run(delegate()
			{
				while (Console.ReadKey(true).Key != ConsoleKey.Escape)
				{
				}
				shouldExit = true;
				Program.relax.Stop();
				Program.timewarp.Stop();
			});
			while (!shouldExit)
			{
				Console.Clear();
				Console.WriteLine("Idling");
				Console.WriteLine("\nPress ESC to return to the main menu.");
				while (!Program.osuManager.CanLoad && !shouldExit)
				{
					Thread.Sleep(5);
				}
				if (shouldExit)
				{
					break;
				}
				OsuBeatmap beatmap = Program.osuManager.Player.Beatmap;
				Console.Clear();
				Console.WriteLine(string.Concat(new string[]
				{
					"Playing ",
					beatmap.Artist,
					" - ",
					beatmap.Title,
					" (",
					beatmap.Creator,
					") [",
					beatmap.Version,
					"]"
				}));
				Console.WriteLine("\nPress ESC to return to the main menu.");
				Task task = Task.Factory.StartNew(delegate()
				{
					if (Program.configManager.EnableRelax && Program.osuManager.Player.CurrentRuleset == Ruleset.Standard)
					{
						Program.relax.Start(beatmap);
					}
				});
				Task task2 = Task.Factory.StartNew(delegate()
				{
					if (Program.configManager.EnableTimewarp)
					{
						Program.timewarp.Start();
					}
				});
				Task.WaitAll(new Task[]
				{
					task,
					task2
				});
			}
			Program.DrawMainMenu();
		}

		private static void DrawSettings()
		{
			Console.Clear();
			Console.WriteLine("---Settings---\n");
			Console.WriteLine("1. Relax settings");
			Console.WriteLine("2. Timewarp settings");
			Console.WriteLine("3. Other settings");
			Console.WriteLine("\nESC. Back to main menu");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape)
			{
				Program.DrawMainMenu();
				return;
			}
			switch (key)
			{
			case ConsoleKey.D1:
				Program.DrawRelaxSettings();
				return;
			case ConsoleKey.D2:
				Program.DrawTimewarpSettings();
				return;
			case ConsoleKey.D3:
				Program.DrawOtherSettings();
				return;
			default:
				Program.DrawSettings();
				return;
			}
		}

		private static void DrawRelaxSettings()
		{
			Console.Clear();
			Console.WriteLine("---Relax Settings---\n");
			Console.WriteLine("1. Relax                      | [" + (Program.configManager.EnableRelax ? "ENABLED" : "DISABLED") + "]");
			Console.WriteLine(string.Format("2. Playstyle                  | [{0}]", Program.configManager.PlayStyle));
			Console.WriteLine(string.Format("3. Primary key                | [{0}]", Program.configManager.PrimaryKey));
			Console.WriteLine(string.Format("4. Secondary key              | [{0}]", Program.configManager.SecondaryKey));
			Console.WriteLine(string.Format("5. Double delay key           | [{0}]", Program.configManager.DoubleDelayKey));
			Console.WriteLine(string.Format("6. Max singletap BPM          | [{0}BPM]", Program.configManager.MaxSingletapBPM));
			Console.WriteLine(string.Format("7. AlternateIfLessThan        | [{0}ms]", Program.configManager.AlternateIfLessThan));
			Console.WriteLine(string.Format("8. Slider alternation binding | [{0}]", Program.configManager.SliderAlternationBinding));
			Console.WriteLine(string.Format("9. Audio offset               | [{0}ms]", Program.configManager.AudioOffset));
			Console.WriteLine(string.Format("0. HoldBeforeSpinner time     | [{0}ms]", Program.configManager.HoldBeforeSpinnerTime));
			Console.WriteLine("\nQ. HitTimings settings");
			Console.WriteLine("W. Hitscan settings");
			Console.WriteLine("\nESC. Back to settings");
			OsuKeys[] array = (OsuKeys[])Enum.GetValues(typeof(OsuKeys));
			SliderAlternationBinding[] array2 = (SliderAlternationBinding[])Enum.GetValues(typeof(SliderAlternationBinding));
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key <= ConsoleKey.D9)
			{
				if (key == ConsoleKey.Escape)
				{
					Program.DrawSettings();
					return;
				}
				switch (key)
				{
				case ConsoleKey.D0:
				{
					int holdBeforeSpinnerTime;
					do
					{
						Console.Clear();
						Console.Write("Enter new HoldBeforeSpinner time: ");
					}
					while (!int.TryParse(Console.ReadLine(), out holdBeforeSpinnerTime));
					Program.configManager.HoldBeforeSpinnerTime = holdBeforeSpinnerTime;
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D1:
					Program.configManager.EnableRelax = !Program.configManager.EnableRelax;
					Program.DrawRelaxSettings();
					return;
				case ConsoleKey.D2:
				{
					int num;
					do
					{
						Console.Clear();
						Console.WriteLine("Select new playstyle:\n");
						PlayStyles[] array3 = (PlayStyles[])Enum.GetValues(typeof(PlayStyles));
						for (int i = 0; i < array3.Length; i++)
						{
							Console.WriteLine(string.Format("{0}. {1}", i + 1, array3[i]));
						}
					}
					while (!int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out num) || num <= 0 || num >= 4);
					Program.configManager.PlayStyle = (PlayStyles)(num - 1);
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D3:
				{
					int num2;
					do
					{
						Console.Clear();
						Console.WriteLine("Enter new primary key:\n");
						for (int j = 0; j < array.Length; j++)
						{
							Console.WriteLine(string.Format("{0}. {1}", j + 1, array[j]));
						}
					}
					while (!int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out num2) || num2 <= 0 || num2 >= 5);
					Program.configManager.PrimaryKey = (OsuKeys)(num2 - 1);
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D4:
				{
					int num3;
					do
					{
						Console.Clear();
						Console.WriteLine("Enter new secondary key:\n");
						for (int k = 0; k < array.Length; k++)
						{
							Console.WriteLine(string.Format("{0}. {1}", k + 1, array[k]));
						}
					}
					while (!int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out num3) || num3 <= 0 || num3 >= 5);
					Program.configManager.SecondaryKey = (OsuKeys)(num3 - 1);
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D5:
					Console.Clear();
					Console.Write("Enter new double delay key: ");
					Program.configManager.DoubleDelayKey = (VirtualKeyCode)Console.ReadKey(true).Key;
					Program.DrawRelaxSettings();
					return;
				case ConsoleKey.D6:
				{
					int num4;
					do
					{
						Console.Clear();
						Console.Write("Enter new max singletap BPM: ");
					}
					while (!int.TryParse(Console.ReadLine(), out num4));
					Program.configManager.MaxSingletapBPM = num4;
					Program.configManager.AlternateIfLessThan = 60000 / num4;
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D7:
				{
					int num5;
					do
					{
						Console.Clear();
						Console.Write("Enter new AlternateIfLessThan: ");
					}
					while (!int.TryParse(Console.ReadLine(), out num5));
					Program.configManager.AlternateIfLessThan = num5;
					Program.configManager.MaxSingletapBPM = 60000 / num5;
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D8:
				{
					int num6;
					do
					{
						Console.Clear();
						Console.WriteLine("Select new slider alternation binding:\n");
						for (int l = 0; l < array2.Length; l++)
						{
							Console.WriteLine(string.Format("{0}. {1}", l + 1, array2[l]));
						}
					}
					while (!int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out num6) || num6 <= 0 || num6 >= 3);
					Program.configManager.SliderAlternationBinding = (SliderAlternationBinding)(num6 - 1);
					Program.DrawRelaxSettings();
					return;
				}
				case ConsoleKey.D9:
				{
					int audioOffset;
					do
					{
						Console.Clear();
						Console.Write("Enter new audio offset: ");
					}
					while (!int.TryParse(Console.ReadLine(), out audioOffset));
					Program.configManager.AudioOffset = audioOffset;
					Program.DrawRelaxSettings();
					return;
				}
				}
			}
			else
			{
				if (key == ConsoleKey.Q)
				{
					Program.DrawHitTimingsSettings();
					return;
				}
				if (key == ConsoleKey.W)
				{
					Program.DrawHitScanSettings();
					return;
				}
			}
			Program.DrawRelaxSettings();
		}

		private static void DrawHitTimingsSettings()
		{
			Console.Clear();
			Console.WriteLine("---HitTimings Settings---\n");
			Console.WriteLine(string.Format("1. Minimum offset           | [{0}%]", Program.configManager.HitTimingsMinOffset));
			Console.WriteLine(string.Format("2. Maximum offset           | [{0}%]", Program.configManager.HitTimingsMaxOffset));
			Console.WriteLine(string.Format("3. Minimum alternate offset | [{0}%]", Program.configManager.HitTimingsAlternateMinOffset));
			Console.WriteLine(string.Format("4. Maximum alternate offset | [{0}%]", Program.configManager.HitTimingsAlternateMaxOffset));
			Console.WriteLine(string.Format("5. Minimum hold time        | [{0}ms]", Program.configManager.HitTimingsMinHoldTime));
			Console.WriteLine(string.Format("6. Maximum hold time        | [{0}ms]", Program.configManager.HitTimingsMaxHoldTime));
			Console.WriteLine(string.Format("7. Minimum slider hold time | [{0}ms]", Program.configManager.HitTimingsMinSliderHoldTime));
			Console.WriteLine(string.Format("8. Maximum slider hold time | [{0}ms]", Program.configManager.HitTimingsMaxSliderHoldTime));
			Console.WriteLine(string.Format("9. Double delay factor      | [{0}x]", Program.configManager.HitTimingsDoubleDelayFactor));
			Console.WriteLine("\nESC. Back to relax settings");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape)
			{
				Program.DrawRelaxSettings();
				return;
			}
			switch (key)
			{
			case ConsoleKey.D1:
			{
				int hitTimingsMinOffset;
				do
				{
					Console.Clear();
					Console.Write("Enter new minimum offset: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsMinOffset));
				Program.configManager.HitTimingsMinOffset = hitTimingsMinOffset;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D2:
			{
				int hitTimingsMaxOffset;
				do
				{
					Console.Clear();
					Console.Write("Enter new maximum offset: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsMaxOffset));
				Program.configManager.HitTimingsMaxOffset = hitTimingsMaxOffset;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D3:
			{
				int hitTimingsAlternateMinOffset;
				do
				{
					Console.Clear();
					Console.Write("Enter new minimum alternate offset: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsAlternateMinOffset));
				Program.configManager.HitTimingsAlternateMinOffset = hitTimingsAlternateMinOffset;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D4:
			{
				int hitTimingsAlternateMaxOffset;
				do
				{
					Console.Clear();
					Console.Write("Enter new maximum alternate offset: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsAlternateMaxOffset));
				Program.configManager.HitTimingsAlternateMaxOffset = hitTimingsAlternateMaxOffset;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D5:
			{
				int hitTimingsMinHoldTime;
				do
				{
					Console.Clear();
					Console.Write("Enter new minimum hold time: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsMinHoldTime));
				Program.configManager.HitTimingsMinHoldTime = hitTimingsMinHoldTime;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D6:
			{
				int hitTimingsMaxHoldTime;
				do
				{
					Console.Clear();
					Console.Write("Enter new maximum hold time: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsMaxHoldTime));
				Program.configManager.HitTimingsMaxHoldTime = hitTimingsMaxHoldTime;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D7:
			{
				int hitTimingsMinSliderHoldTime;
				do
				{
					Console.Clear();
					Console.Write("Enter new minimum slider hold time: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsMinSliderHoldTime));
				Program.configManager.HitTimingsMinSliderHoldTime = hitTimingsMinSliderHoldTime;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D8:
			{
				int hitTimingsMaxSliderHoldTime;
				do
				{
					Console.Clear();
					Console.Write("Enter new maximum slider hold time: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitTimingsMaxSliderHoldTime));
				Program.configManager.HitTimingsMaxSliderHoldTime = hitTimingsMaxSliderHoldTime;
				Program.DrawHitTimingsSettings();
				return;
			}
			case ConsoleKey.D9:
			{
				float hitTimingsDoubleDelayFactor;
				do
				{
					Console.Clear();
					Console.Write("Enter double delay factor: ");
				}
				while (!float.TryParse(Console.ReadLine(), out hitTimingsDoubleDelayFactor));
				Program.configManager.HitTimingsDoubleDelayFactor = hitTimingsDoubleDelayFactor;
				Program.DrawHitTimingsSettings();
				return;
			}
			default:
				Program.DrawHitTimingsSettings();
				return;
			}
		}

		private static void DrawHitScanSettings()
		{
			Console.Clear();
			Console.WriteLine("---HitScan Settings---\n");
			Console.WriteLine("1. HitScan                              | [" + (Program.configManager.EnableHitScan ? "ENABLED" : "DISABLED") + "]");
			Console.WriteLine("2. Prediction                           | [" + (Program.configManager.EnableHitScanPrediction ? "ENABLED" : "DISABLED") + "]");
			Console.WriteLine(string.Format("3. Prediction direction angle tolerance | [{0}°]", Program.configManager.HitScanPredictionDirectionAngleTolerance));
			Console.WriteLine(string.Format("4. Prediction radius scale              | [{0}x]", Program.configManager.HitScanPredictionRadiusScale));
			Console.WriteLine(string.Format("5. Prediction max distance              | [{0}px]", Program.configManager.HitScanPredictionMaxDistance));
			Console.WriteLine(string.Format("6. Miss radius                          | [{0}px]", Program.configManager.HitScanMissRadius));
			Console.WriteLine(string.Format("7. Miss chance                          | [{0}%]", Program.configManager.HitScanMissChance));
			Console.WriteLine("8. Miss after HitWindow50               | [" + (Program.configManager.HitScanMissAfterHitWindow50 ? "ENABLED" : "DISABLED") + "]");
			Console.WriteLine("\nESC. Back to relax settings");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape)
			{
				Program.DrawRelaxSettings();
				return;
			}
			switch (key)
			{
			case ConsoleKey.D1:
				Program.configManager.EnableHitScan = !Program.configManager.EnableHitScan;
				Program.DrawHitScanSettings();
				return;
			case ConsoleKey.D2:
				Program.configManager.EnableHitScanPrediction = !Program.configManager.EnableHitScanPrediction;
				Program.DrawHitScanSettings();
				return;
			case ConsoleKey.D3:
			{
				int hitScanPredictionDirectionAngleTolerance;
				do
				{
					Console.Clear();
					Console.Write("Enter new direction angle tolerance: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitScanPredictionDirectionAngleTolerance));
				Program.configManager.HitScanPredictionDirectionAngleTolerance = hitScanPredictionDirectionAngleTolerance;
				Program.DrawHitScanSettings();
				return;
			}
			case ConsoleKey.D4:
			{
				float hitScanPredictionRadiusScale;
				do
				{
					Console.Clear();
					Console.Write("Enter new radius scale: ");
				}
				while (!float.TryParse(Console.ReadLine(), out hitScanPredictionRadiusScale));
				Program.configManager.HitScanPredictionRadiusScale = hitScanPredictionRadiusScale;
				Program.DrawHitScanSettings();
				return;
			}
			case ConsoleKey.D5:
			{
				int hitScanPredictionMaxDistance;
				do
				{
					Console.Clear();
					Console.Write("Enter new max distance: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitScanPredictionMaxDistance));
				Program.configManager.HitScanPredictionMaxDistance = hitScanPredictionMaxDistance;
				Program.DrawHitScanSettings();
				return;
			}
			case ConsoleKey.D6:
			{
				int hitScanMissRadius;
				do
				{
					Console.Clear();
					Console.Write("Enter new miss radius: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitScanMissRadius));
				Program.configManager.HitScanMissRadius = hitScanMissRadius;
				Program.DrawHitScanSettings();
				return;
			}
			case ConsoleKey.D7:
			{
				int hitScanMissChance;
				do
				{
					Console.Clear();
					Console.Write("Enter new miss chance: ");
				}
				while (!int.TryParse(Console.ReadLine(), out hitScanMissChance));
				Program.configManager.HitScanMissChance = hitScanMissChance;
				Program.DrawHitScanSettings();
				return;
			}
			case ConsoleKey.D8:
				Program.configManager.HitScanMissAfterHitWindow50 = !Program.configManager.HitScanMissAfterHitWindow50;
				Program.DrawHitScanSettings();
				return;
			default:
				Program.DrawHitScanSettings();
				return;
			}
		}

		private static void DrawTimewarpSettings()
		{
			Console.Clear();
			Console.WriteLine("---Timewarp Settings---\n");
			Console.WriteLine("1. Timewarp      | [" + (Program.configManager.EnableTimewarp ? "ENABLED" : "DISABLED") + "]");
			Console.WriteLine(string.Format("2. Timewarp rate | [{0}x]\n", Program.configManager.TimewarpRate));
			Console.WriteLine("---!!!---");
			Console.WriteLine("Please note that timewarp is only partially undetected at this moment.");
			Console.WriteLine("Bancho (and probably any other server) can't detect it and auto-restrict you.");
			Console.WriteLine("But third party anticheats, such as firedigger's replay analyzer and circleguard will detect timewarp presence.\n");
			Console.WriteLine("Use only at your own risk.");
			Console.WriteLine("---!!!---");
			Console.WriteLine("\nESC. Back to settings");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape)
			{
				Program.DrawSettings();
				return;
			}
			if (key == ConsoleKey.D1)
			{
				Program.configManager.EnableTimewarp = !Program.configManager.EnableTimewarp;
				Program.DrawTimewarpSettings();
				return;
			}
			if (key != ConsoleKey.D2)
			{
				Program.DrawTimewarpSettings();
				return;
			}
			double timewarpRate;
			do
			{
				Console.Clear();
				Console.Write("Enter new timewarp rate: ");
			}
			while (!double.TryParse(Console.ReadLine(), out timewarpRate));
			Program.configManager.TimewarpRate = timewarpRate;
			Program.DrawTimewarpSettings();
		}

		private static void DrawOtherSettings()
		{
			Console.Clear();
			Console.WriteLine("---Other Settings---\n");
			Console.WriteLine("1. Custom window title | [" + (Program.configManager.UseCustomWindowTitle ? ("ON | " + Program.configManager.CustomWindowTitle) : "OFF") + "]");
			Console.WriteLine("\nESC. Back to settings");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape)
			{
				Program.DrawSettings();
				return;
			}
			if (key == ConsoleKey.D1)
			{
				Console.Clear();
				Console.WriteLine("Use custom window title?\n");
				Console.WriteLine("1. Yes");
				Console.WriteLine("2. No");
				Program.configManager.UseCustomWindowTitle = (Console.ReadKey(true).Key == ConsoleKey.D1);
				if (Program.configManager.UseCustomWindowTitle)
				{
					Console.Clear();
					Console.Write("Enter new custom window title: ");
					Program.configManager.CustomWindowTitle = Console.ReadLine();
					Console.Title = Program.configManager.CustomWindowTitle;
				}
				else
				{
					Console.Title = Program.defaultConsoleTitle;
				}
				Program.DrawOtherSettings();
				return;
			}
			Program.DrawOtherSettings();
		}

		private static void DrawSupportInfo()
		{
			Console.Clear();
			Console.WriteLine("---What can i do to help osu!rx?---\n");
			Console.WriteLine("Glad you're interested!\n");
			Console.WriteLine("-----------------------------------\n");
			Console.WriteLine("If you like what i'm doing and are willing to support me financially - consider becoming a sponsor <3!");
			Console.WriteLine("Select any service below to proceed.\n");
			Console.WriteLine("1. Ko-fi");
			Console.WriteLine("2. Buy Me A Coffee");
			Console.WriteLine("3. PayPal");
			Console.WriteLine("4. Qiwi\n");
			Console.WriteLine("-----------------------------------\n");
			Console.WriteLine("If you can't or don't want to support me financially - that's totally fine!");
			Console.WriteLine("You can still help me by providing any feedback, reporting bugs and requesting features!");
			Console.WriteLine("Any help is highly appreciated!\n");
			Console.WriteLine("5. Provide feedback via MPGH\n");
			Console.WriteLine("-----------------------------------");
			Console.WriteLine("\nESC. Back to settings");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key >= ConsoleKey.D1 && key <= ConsoleKey.D5)
			{
				Process.Start(Program.links[key - ConsoleKey.D1]);
			}
			else if (key == ConsoleKey.Escape)
			{
				Program.DrawMainMenu();
			}
			Program.DrawSupportInfo();
		}

		private static OsuManager osuManager;

		private static ConfigManager configManager;

		private static Relax relax;

		private static Timewarp timewarp;

		private static string defaultConsoleTitle;

		private static string[] links = new string[]
		{
			"https://ko-fi.com/mrflashstudio",
			"https://www.buymeacoffee.com/mrflashstudio",
			"https://www.paypal.me/mrflashstudio",
			"https://qiwi.com/n/mrflashstudio",
			"https://www.mpgh.net/forum/showthread.php?t=1488076"
		};
	}
}
