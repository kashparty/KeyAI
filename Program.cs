using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;

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

        private static bool RunIteration(Model model) {
            string target = model.RandomString(80);
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

            if (!escaped) model.UpdateTimes(target, keyTimes);

            Console.ResetColor();
            Console.WriteLine();

            return escaped;
        }

        static void Main(string[] args) {
            if (!IsTrainingFileDownloaded()) {
                Console.Write("Attempting to download training file... ");
                DownloadTrainingFile();
                Console.WriteLine("Done");
            } else {
                Console.WriteLine("Training file exists.");
            }

            Model model = new Model(trainingFileName, ngramLength);
            bool endTraining = false;
            while (!endTraining) {
                if (RunIteration(model)) endTraining = true;
            }
        }
    }
}
