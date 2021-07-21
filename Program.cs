using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KeyAI {
    class Program {
        const string trainingFileUrl = "https://www.gutenberg.org/files/11/11-0.txt";
        const string trainingFileName = "training.txt";
        const int ngramLength = 3;

        private static bool IsTrainingFileDownloaded() {
            return File.Exists(trainingFileName);
        }

        private static void DownloadTrainingFile() {
            using (WebClient client = new WebClient()) {
                client.DownloadFile(trainingFileUrl, trainingFileName);
            }
        }

        private static bool RunIteration(QLearn qLearn) {
            string target = qLearn.RandomString(80);
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
                Console.Write(new String(' ', Math.Max(0, 80 - target.Length)));

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
            using (StreamReader streamReader = new StreamReader(trainingFileName)) {
                trainingData = streamReader.ReadToEnd();
            }

            trainingData = Regex.Replace(trainingData, "[^a-zA-Z ]", "");
            return trainingData;
        }

        static void Main(string[] args) {
            if (!IsTrainingFileDownloaded()) {
                Console.Write("Attempting to download training file... ");
                DownloadTrainingFile();
                Console.WriteLine("Done");
            } else {
                Console.WriteLine("Training file exists.");
            }

            string trainingData = PreProcess();
            QLearn qLearn = new QLearn(trainingData, 0.3, 1.0, 5, 0.5, 0.95);

            bool endTraining = false;
            while (!endTraining) {
                if (RunIteration(qLearn)) endTraining = true;
            }
        }
    }
}
