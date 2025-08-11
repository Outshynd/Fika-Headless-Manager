using FikaHeadlessManager.Models;
using System.Diagnostics;
using System.Text.Json;

namespace FikaHeadlessManager
{
    public class Program
    {
        static Settings? Settings { get; set; }
        static string? StartArguments
        {
            get
            {
                if (Settings == null)
                {
                    Log("Settings were null when trying to generate StartArguments?", ConsoleColor.Red);
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(Settings.ProfileId))
                {
                    Log("ProfileId was null!", ConsoleColor.Red);
                    return string.Empty;
                }

                if (Settings.BackendUrl == null)
                {
                    Log("BackendUrl was null!", ConsoleColor.Red);
                    return string.Empty;
                }

                return $"-token={Settings.ProfileId} -config={{'BackendUrl':'{Settings.BackendUrl.OriginalString}','Version':'live'}}{(WithGraphics ? string.Empty : " -nographics -batchmode")} --enable-console true";
            }
        }
        static bool WithGraphics { get; set; }
        static Process? TarkovProcess { get; set; }

        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            string configPath = "HeadlessConfig.json";
            if (!File.Exists(configPath))
            {
                Log("Unable to find the configuration file 'HeadlessConfig.json'.\nMake sure that you have configured the headless correctly!", ConsoleColor.Red);
                Console.ReadKey(true);
            }

            using (FileStream fileStream = File.OpenRead(configPath))
            {
                Settings = await JsonSerializer.DeserializeAsync<Settings>(fileStream);
            }

            WithGraphics = WaitForGraphicsInput();

            _ = Task.Run(StartGame);
            await Task.Delay(-1);
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            TarkovProcess?.Kill();
        }

        private static void StartGame()
        {
            Log($"Starting headless client {(WithGraphics ? "with" : "without")} graphics.");

            ProcessStartInfo startInfo = new()
            {
                Arguments = StartArguments,
                UseShellExecute = true,
                FileName = "EscapeFromTarkov.exe",
            };

            Process? gameProcess = Process.Start(startInfo);
            
            if (gameProcess == null)
            {
                Log("Could not start the headless client!", ConsoleColor.Red);
                Console.ReadKey();
                Environment.Exit(1);
            }

            gameProcess.EnableRaisingEvents = true;
            gameProcess.Exited += GameProcess_Exited;
            TarkovProcess = gameProcess;
        }

        private static void GameProcess_Exited(object? sender, EventArgs e)
        {
            if (sender is Process process)
            {
                process.Exited -= GameProcess_Exited;
            }

            TarkovProcess = null;

            Log("Game exited, restarting...");
            WithGraphics = WaitForGraphicsInput();
            StartGame();
        }

        private static bool WaitForGraphicsInput()
        {
            Log("Press 'g' to start with graphics, otherwise wait 3 seconds...");
            DateTime delayTime = DateTime.Now.AddSeconds(3);
            while (DateTime.Now < delayTime)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key is ConsoleKey.G)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (color is not ConsoleColor.White)
            {
                Console.ForegroundColor = color;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
