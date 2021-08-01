using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace KeyAI {
    class Program {
        static Preferences preferences;
        const string versionNumber = "0.2.0";

        static string[] menuOptions = { "Exit", "Typing tutor", "Statistics", "Help" };

        private static bool IsTrainingFileDownloaded() {
            return File.Exists(preferences.trainingFilePath);
        }

        private static void DownloadTrainingFile() {
            using (WebClient client = new WebClient()) {
                client.DownloadFile(preferences.trainingFileUrl, preferences.trainingFilePath);
            }
        }

        // private static bool RunRound(QLearn qLearn) {
        //     string target = qLearn.RandomString(preferences.lineLength);
        //     Console.Write(target);

        //     Console.SetCursorPosition(0, Console.CursorTop);
        //     Console.ForegroundColor = preferences.color;

        //     bool doneTyping = false;
        //     bool escaped = false;
        //     int currentPos = 0;
        //     int mistakes = 0;
        //     bool currentMistake = false;

        //     Stopwatch stopwatch = new Stopwatch();
        //     List<long> keyTimes = new List<long>();

        //     stopwatch.Start();

        //     while (!doneTyping) {
        //         ConsoleKeyInfo pressedKey = Console.ReadKey(true);

        //         if (pressedKey.Key == ConsoleKey.Escape) {
        //             doneTyping = true;
        //             escaped = true;
        //         }

        //         if (pressedKey.KeyChar == target[currentPos]) {
        //             stopwatch.Stop();

        //             keyTimes.Add(stopwatch.ElapsedMilliseconds);
        //             Console.Write(pressedKey.KeyChar);
        //             currentPos++;

        //             if (currentMistake) {
        //                 mistakes++;
        //                 currentMistake = false;
        //             }

        //             stopwatch.Restart();
        //         } else {
        //             currentMistake = true;
        //         }

        //         if (currentPos >= target.Length) doneTyping = true;
        //     }

        //     Console.ResetColor();

        //     if (!escaped) {
        //         Console.Write(new String(' ', Math.Max(0, preferences.lineLength - target.Length)));

        //         double wordsPerMinute = 0;
        //         for (int i = 1; i < keyTimes.Count; i++) wordsPerMinute += keyTimes[i];
        //         wordsPerMinute = ((double)(keyTimes.Count - 1) / 5.0) / (wordsPerMinute / (1000 * 60));
        //         Console.Write($"    WPM: {Math.Round(wordsPerMinute)}".PadRight(11));

        //         double accuracy = (double)(keyTimes.Count - mistakes) * 100.0 / keyTimes.Count;
        //         Console.Write($"    Accuracy: {Math.Round(accuracy)}".PadRight(17));

        //         qLearn.UpdateModel(target, keyTimes);
        //         qLearn.FinishRound();
        //     }

        //     Console.WriteLine();

        //     return escaped;
        // }

        private static bool RunRound(QLearn qLearn) {
            string target = qLearn.RandomString(preferences.lineLength);
            Console.Write(target);

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = preferences.color;

            bool doneTyping = false;
            bool escaped = false;
            int pos = 0;
            int mistakes = 0;

            Stopwatch stopwatch = new Stopwatch();
            List<Tuple<string, long>> keyTimes = new List<Tuple<string, long>>();
            List<Tuple<char, long>> tempTimes = new List<Tuple<char, long>>();

            stopwatch.Start();

            while (!doneTyping) {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                stopwatch.Stop();

                if (pressedKey.Key == ConsoleKey.Escape) {
                    doneTyping = true;
                    escaped = true;
                }

                char keyChar = pressedKey.KeyChar;
                tempTimes.Add(new Tuple<char, long>(keyChar, stopwatch.ElapsedMilliseconds));

                if (keyChar == target[pos]) {
                    Console.Write(target[pos]);
                    if (pos >= 2) keyTimes.Add(new Tuple<string, long>(target.Substring(pos - 2, 3), tempTimes.Last().Item2));
                    pos++;

                    if (tempTimes.Count > 1) {
                        mistakes++;
                    }

                    tempTimes.Clear();
                    stopwatch.Reset();
                } else {
                    if (preferences.allowSkip && tempTimes.Count >= preferences.skipLength) {
                        if (pos + preferences.skipLength < target.Length) {
                            string subtarget = target.Substring(pos + 1, preferences.skipLength);
                            List<Tuple<char, long>> newTimes = new List<Tuple<char, long>>();
                            long prevTime = 0;
                            int tempMistakes = 0;
                            int subpos = 0;
                            bool found = false;

                            foreach (char c in subtarget) {
                                bool mistakeMade = false;
                                found = false;
                                while (subpos < tempTimes.Count && !found) {
                                    if (tempTimes[subpos].Item1 == c) {
                                        found = true;
                                        newTimes.Add(new Tuple<char, long>(tempTimes[subpos].Item1, tempTimes[subpos].Item2 - prevTime));
                                        prevTime = tempTimes[subpos].Item2;

                                        if (mistakeMade) {
                                            tempMistakes++;
                                            mistakeMade = false;
                                        }
                                    } else mistakeMade = true;
                                    subpos++;
                                }
                                if (!found) break;
                            }

                            if (found) {
                                mistakes += 1 + tempMistakes;
                                Console.Write(target[pos]);
                                pos++;

                                foreach (Tuple<char, long> time in newTimes) {
                                    if (pos >= 2) keyTimes.Add(new Tuple<string, long>(target.Substring(pos - 2, 3), time.Item2));
                                    Console.Write(time.Item1);
                                    pos++;
                                }

                                tempTimes.Clear();
                                stopwatch.Reset();
                            }
                        }
                    }
                }

                if (pos >= target.Length) doneTyping = true;
                else stopwatch.Start();
            }

            Console.ResetColor();

            if (!escaped) {
                Console.Write(new String(' ', Math.Max(0, preferences.lineLength - target.Length)));

                double wordsPerMinute = 0;
                for (int i = 1; i < keyTimes.Count; i++) wordsPerMinute += keyTimes[i].Item2;
                wordsPerMinute = ((double)(keyTimes.Count - 1) / 5.0) / (wordsPerMinute / (1000 * 60));
                Console.Write($"    WPM: {Math.Round(wordsPerMinute)}".PadRight(11));

                double accuracy = (double)(keyTimes.Count - mistakes) * 100.0 / keyTimes.Count;
                Console.Write($"    Accuracy: {Math.Round(accuracy)}".PadRight(17));

                qLearn.UpdateModel(keyTimes);
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

            string regexString = "[^";
            if (preferences.includeLowercase) regexString += "a-z";
            if (preferences.includeUppercase) regexString += "A-Z";
            if (preferences.includeDigits) regexString += "0-9";
            if (preferences.includePunctuation) regexString += ".,!:;\"\'-()";
            regexString += " ]";

            trainingData = Regex.Replace(trainingData, regexString, "");
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

        private static void ShowStatistics() {
            if (!File.Exists("model.txt")) {
                Console.WriteLine("No data to show yet. Come back after using the typing tutor.");
                return;
            }

            using (StreamReader streamReader = new StreamReader("model.txt")) {
                List<Tuple<string, double>> modelData = new List<Tuple<string, double>>();
                string[] lines = streamReader.ReadToEnd().Split("\n");

                foreach (string line in lines) {
                    if (line.Length <= 4) continue;

                    string nGram = line.Substring(0, 3);
                    double qValue = double.Parse(line.Substring(4));

                    modelData.Add(new Tuple<string, double>(nGram, qValue));
                }

                modelData.OrderByDescending(d => d.Item2);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("These are your slowest patterns:");
                for (int i = 0; i < Math.Min(modelData.Count, 5); i++) {
                    Console.WriteLine($"{i + 1}. {modelData[i].Item1}");
                }
                Console.ResetColor();
            }
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
                        ShowStatistics();
                        break;
                    case 3:
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
