using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KeyAI {
    class Program {
        static Preferences preferences;
        const string versionNumber = "0.1.0";

        static string[] menuOptions = { "Exit", "Typing tutor", "Help" };

        private static bool IsTrainingFileDownloaded() {
            return File.Exists(preferences.trainingFilePath);
        }

        private static void DownloadTrainingFile() {
            using (WebClient client = new WebClient()) {
                client.DownloadFile(preferences.trainingFileUrl, preferences.trainingFilePath);
            }
        }

        private static bool RunRound(QLearn qLearn) {
            string target = qLearn.RandomString(preferences.lineLength);
            Console.Write(target);

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Green;

            bool doneTyping = false;
            bool escaped = false;
            int currentPos = 0;

            Stopwatch stopwatch = new Stopwatch();
            List<long> keyTimes = new List<long>();

            stopwatch.Start();

            while (!doneTyping) {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);

                if (pressedKey.Key == ConsoleKey.Escape) {
                    doneTyping = true;
                    escaped = true;
                }

                if (pressedKey.KeyChar == target[currentPos]) {
                    stopwatch.Stop();

                    keyTimes.Add(stopwatch.ElapsedMilliseconds);
                    Console.Write(pressedKey.KeyChar);
                    currentPos++;

                    stopwatch.Restart();
                }

                if (currentPos >= target.Length) doneTyping = true;
            }

            Console.ResetColor();

            if (!escaped) {
                Console.Write(new String(' ', Math.Max(0, preferences.lineLength - target.Length)));

                double wordsPerMinute = 0;
                for (int i = 1; i < keyTimes.Count; i++) wordsPerMinute += keyTimes[i];
                wordsPerMinute = ((double)(keyTimes.Count - 1) / 5.0) / (wordsPerMinute / (1000 * 60));

                Console.Write($"    WPM: {Math.Round(wordsPerMinute)}".PadRight(11));

                qLearn.UpdateModel(target, keyTimes);
                qLearn.FinishRound();
            }

            Console.WriteLine();

            return escaped;
        }

        private static string PreProcess() {
            string trainingData;
            using (StreamReader streamReader = new StreamReader(preferences.trainingFilePath)) {
                trainingData = streamReader.ReadToEnd();
            }

            trainingData = Regex.Replace(trainingData, "[^a-zA-Z ]", "");
            return trainingData;
        }

        static void TrainingLoop() {
            Console.WriteLine();

            if (!IsTrainingFileDownloaded()) {
                Console.Write("Attempting to download training file... ");
                DownloadTrainingFile();
                Console.WriteLine("Done");
            } else {
                Console.WriteLine("Training file exists.");
            }

            string trainingData = PreProcess();
            QLearn qLearn = new QLearn(
                trainingData,
                preferences.explorationLow,
                preferences.explorationHigh,
                preferences.numRounds,
                preferences.learningRate,
                preferences.discount
            );
            Console.WriteLine();

            bool endTraining = false;
            while (!endTraining) endTraining = RunRound(qLearn);
        }

        private static void DisplayMainMenu() {
            Console.WriteLine("\nSelect an option:");

            for (int i = 0; i < menuOptions.Length; i++) {
                Console.WriteLine($"{i}. {menuOptions[i]}");
            }
        }

        private static int GetUserOption() {
            bool validOption = false;
            int userOption = -1;

            while (!validOption) {
                Console.Write("\nEnter your chosen option number: ");

                if (int.TryParse(Console.ReadLine(), out userOption)) {
                    if (userOption >= 0 && userOption < menuOptions.Length) validOption = true;
                }

                if (!validOption) Console.WriteLine("Sorry, that's not a valid option number. Try again.");
            }

            return userOption;
        }

        private static void ShowHelp() {
            Console.WriteLine($"This is KeyAI version {versionNumber}. Opening help page...");
            string url = "https://github.com/KashParty/KeyAI/blob/master/README.md";
            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            });
        }

        static void Main(string[] args) {
            preferences = new Preferences("keyai.json");
            Console.WriteLine($"KeyAI {versionNumber}.");

            bool done = false;
            while (!done) {
                DisplayMainMenu();
                int userOption = GetUserOption();

                switch (userOption) {
                    case 0:
                        done = true;
                        break;
                    case 1:
                        TrainingLoop();
                        break;
                    case 2:
                        ShowHelp();
                        break;
                    default:
                        Console.WriteLine("Nothing here yet.");
                        break;
                }
            }
        }
    }
}
