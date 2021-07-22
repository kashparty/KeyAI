using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KeyAI {
    class Program {
        // URL from which to download the training corpus used for text generation.
        const string trainingFileUrl = "https://www.gutenberg.org/files/11/11-0.txt";
        
        // Where the training corpus should be stored.
        const string trainingFileName = "training.txt";

        private static bool IsTrainingFileDownloaded() {
            return File.Exists(trainingFileName);
        }

        private static void DownloadTrainingFile() {
            using (WebClient client = new WebClient()) {
                client.DownloadFile(trainingFileUrl, trainingFileName);
            }
        }

        // Use the model to generate another line of text, 80 chars long. Run one training round.
        private static bool RunRound(QLearn qLearn) {
            // Display a randomly generated target string from the model.
            string target = qLearn.RandomString(80);
            Console.Write(target);

            // Place the cursor at the start of the line and set the typing color.
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Green;

            // The user can press the Escape key to end the program from within a round.
            bool doneTyping = false;
            bool escaped = false;
            int currentPos = 0;

            // Used for measuring and storing the time taken to press each key.
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

                // If the user has correctly typed the entire string, then end the round.
                if (currentPos >= target.Length) doneTyping = true;
            }

            Console.ResetColor();

            if (!escaped) {
                // Manual padding (it has to go here because we moved the cursor) to make things look nice.
                Console.Write(new String(' ', Math.Max(0, 80 - target.Length)));

                // Calculate the user's typing speed in words per minute.
                double wordsPerMinute = 0;
                for (int i = 1; i < keyTimes.Count; i++) wordsPerMinute += keyTimes[i];
                wordsPerMinute = ((double)(keyTimes.Count - 1) / 5.0) / (wordsPerMinute / (1000 * 60));

                // Display the WPM score with some padding to make things align nicely.
                Console.Write($"    WPM: {Math.Round(wordsPerMinute)}".PadRight(11));

                // Update the model based on the data from this round.
                qLearn.UpdateModel(target, keyTimes);
                qLearn.FinishRound();
            }

            Console.WriteLine();

            // Needs to be returned to indicate that the program should end.
            return escaped;
        }

        // Read the training corpus from the file and remove unwanted characters.
        private static string PreProcess() {
            string trainingData;
            using (StreamReader streamReader = new StreamReader(trainingFileName)) {
                trainingData = streamReader.ReadToEnd();
            }

            // Remove all characters that are not a-z, A-Z or space.
            trainingData = Regex.Replace(trainingData, "[^a-zA-Z ]", "");
            return trainingData;
        }

        static void Main(string[] args) {
            // Download the training corpus.
            if (!IsTrainingFileDownloaded()) {
                Console.Write("Attempting to download training file... ");
                DownloadTrainingFile();
                Console.WriteLine("Done");
            } else {
                Console.WriteLine("Training file exists.");
            }

            // Load the data and the Q-learning model.
            string trainingData = PreProcess();
            QLearn qLearn = new QLearn(trainingData, 0.3, 1.0, 5, 0.5, 0.95);
            
            // Run training rounds until the user finishes training.
            bool endTraining = false;
            while (!endTraining) endTraining = RunRound(qLearn);
        }
    }
}
