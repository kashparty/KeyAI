using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace KeyAI {
    class NgramData {
        static Random random = new Random();
        public Dictionary<char, int> nextChars { get; set; }
        public long totalTime { get; set; }
        public int measurements { get; set; }

        public NgramData() {
            nextChars = new Dictionary<char, int>();
            totalTime = random.Next(50);
            measurements = 1;
        }

        public void UpdateOccurrence(char nextChar) {
            if (nextChars.ContainsKey(nextChar)) nextChars[nextChar]++;
            else nextChars[nextChar] = 1;
        }

        public void UpdateTime(long measuredTime) {
            totalTime += measuredTime;
            measurements++;
        }

        public double GetMean() {
            return (double)totalTime / (double)measurements;
        }
    }

    class Model {
        int ngramLength;
        Dictionary<string, NgramData> modelData;
        Random random = new Random();
        public Model(string trainingFileName, int ngramLength) {
            this.ngramLength = ngramLength;
            modelData = new Dictionary<string, NgramData>();

            string trainingData;
            using (StreamReader streamReader = new StreamReader(trainingFileName)) {
                trainingData = streamReader.ReadToEnd();
            }

            trainingData = Regex.Replace(trainingData, "[^a-zA-Z ]", "");

            string currentNgram = "";
            for (int pos = 0; pos < trainingData.Length - 1; pos++) {
                currentNgram += trainingData[pos];
                if (pos >= ngramLength) {
                    currentNgram = currentNgram.Substring(1);
                }

                if (!modelData.ContainsKey(currentNgram)) modelData.Add(currentNgram, new NgramData());

                char nextAfterNgram = trainingData[pos + 1];
                modelData[currentNgram].UpdateOccurrence(nextAfterNgram);
            }

            Console.WriteLine($"There are {modelData.Count} ngrams in the model.");
        }

        public char RandomNext(string current) {
            // NgramData choices = modelData[current];
            // int totalOccurrences = choices.nextChars.Values.Sum();
            // int randomTarget = random.Next(1, totalOccurrences + 1);

            // List<char> choicesList = choices.nextChars.Keys.ToList().OrderByDescending(c => modelData[$"{current.Substring(1)}{c}"].GetMean()).ToList();
            // foreach (char c in choicesList) {
            //     randomTarget -= choices.nextChars[c];
            //     if (randomTarget <= 0) return c;
            // }

            // return choices.nextChars.Keys.First();

            NgramData ngramData = modelData[current];
            List<string> possibleNextNgrams = ngramData.nextChars.Keys.Select(c => $"{current.Substring(1)}{c}").ToList();
            possibleNextNgrams = possibleNextNgrams.OrderByDescending(p => modelData[p].GetMean() * ngramData.nextChars[p.Last()]).ToList();

            double totalScore = possibleNextNgrams.Aggregate(0.0, (t, s) => t + modelData[s].GetMean() * ngramData.nextChars[s.Last()]);
            double randomDouble = random.NextDouble() * totalScore;
            foreach (string p in possibleNextNgrams) {
                randomDouble -= modelData[p].GetMean() * ngramData.nextChars[p.Last()];
                if (randomDouble <= 0) return p.Last();
            }

            return possibleNextNgrams[0].Last();
        }

        public string RandomString(int maxLength) {
            string current = modelData.Keys.ToList()[random.Next(modelData.Count)];
            char newChar = RandomNext(current.Substring(current.Length - ngramLength, ngramLength));

            while (current.Length <= maxLength) {
                current += newChar;
                newChar = RandomNext(current.Substring(current.Length - ngramLength, ngramLength));
            }

            return current.Trim();
        }

        public void UpdateTimes(string targetString, List<long> times) {
            string currentNgram = "";
            for (int pos = 0; pos < targetString.Length; pos++) {
                currentNgram += targetString[pos];
                if (pos >= ngramLength) {
                    currentNgram = currentNgram.Substring(1);
                }

                if (modelData.ContainsKey(currentNgram)) {
                    modelData[currentNgram].UpdateTime(times[pos - 1] + times[pos]);
                }
            }
        }
    }
}