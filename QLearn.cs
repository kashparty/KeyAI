using System;
using System.IO;
using System.Collections.Generic;

namespace KeyAI {
    class QLearn {
        string trainingData;
        double[,,] table;
        int[,,] occurrences;

        double explorationRate;
        double explorationHigh;
        double explorationLow;
        int currentRound;
        int numRounds;

        double learningRate;
        double discount;

        static Random random = new Random();

        Dictionary<char, int> charToInt;
        Dictionary<int, char> intToChar;

        public QLearn(string trainingData, double explorationLow, double explorationHigh, int numRounds, double learningRate, double discount) {
            this.trainingData = trainingData;
            this.explorationLow = explorationLow;
            this.explorationHigh = explorationHigh;
            this.numRounds = numRounds;
            this.learningRate = learningRate;
            this.discount = discount;

            explorationRate = explorationHigh;
            currentRound = 0;

            charToInt = new Dictionary<char, int>();
            intToChar = new Dictionary<int, char>();
            int currentIndex = 0;
            foreach (char c in trainingData) {
                if (!charToInt.ContainsKey(c)) {
                    charToInt.Add(c, currentIndex);
                    intToChar.Add(currentIndex, c);
                    currentIndex++;
                }
            }

            table = new double[charToInt.Count, charToInt.Count, charToInt.Count];
            occurrences = new int[charToInt.Count, charToInt.Count, charToInt.Count];

            string current = "";
            for (int i = 0; i < trainingData.Length; i++) {
                if (i >= 3) current = current.Substring(1);
                if (current.Length == 2) {
                    occurrences[charToInt[current[0]], charToInt[current[1]], charToInt[trainingData[i]]]++;
                }
                current += trainingData[i];
            }

            LoadModelIfExists();
        }

        private void LoadModelIfExists() {
            if (File.Exists("model.txt")) {
                using (StreamReader streamReader = new StreamReader("model.txt")) {
                    string[] lines = streamReader.ReadToEnd().Split("\n");
                    foreach (string line in lines) {
                        if (line.Length <= 4) continue;
                        string nGram = line.Substring(0, 3);
                        double qValue = double.Parse(line.Substring(4));

                        int charIndex1 = charToInt[nGram[0]];
                        int charIndex2 = charToInt[nGram[1]];
                        int charIndex3 = charToInt[nGram[2]];

                        table[charIndex1, charIndex2, charIndex3] = qValue;
                    }
                }

                Console.WriteLine("Using model stored in file.");
            } else {
                Console.WriteLine("Using new model.");
            }
        }

        public char GreedyChar(string previous) {
            previous = previous.Substring(previous.Length - 2);

            int charIndex1 = charToInt[previous[0]];
            int charIndex2 = charToInt[previous[1]];

            char bestChar = '\0';
            double bestScore = 0;

            for (int i = 0; i < intToChar.Count; i++) {
                if (charIndex1 == charIndex2 && charIndex2 == i) continue;
                if (table[charIndex1, charIndex2, i] > bestScore) {
                    bestChar = intToChar[i];
                    bestScore = table[charIndex1, charIndex2, i];
                }
            }

            if (bestChar == '\0') return RandomChar(previous);

            return bestChar;
        }

        public char RandomChar(string previous) {
            previous = previous.Substring(previous.Length - 2);

            int charIndex1 = charToInt[previous[0]];
            int charIndex2 = charToInt[previous[1]];

            int sumOfScores = 0;
            for (int i = 0; i < intToChar.Count; i++) {
                sumOfScores += occurrences[charIndex1, charIndex2, i];
            }

            int randomScore = random.Next(1, sumOfScores + 1);
            int j;
            for (j = 0; j < intToChar.Count; j++) {
                if (charIndex1 == charIndex2 && charIndex2 == j) continue;
                randomScore -= occurrences[charIndex1, charIndex2, j];
                if (randomScore <= 0) return intToChar[j];
            }

            return intToChar[random.Next(intToChar.Count)];
        }

        public char NextChar(string previous) {
            if (random.NextDouble() > explorationRate) return GreedyChar(previous);
            else return RandomChar(previous);
        }

        public string RandomString(int maxLength) {
            string current = trainingData.Substring(random.Next(trainingData.Length - 1), 2);
            char newChar = NextChar(current);

            while (current.Length < maxLength) {
                current += newChar;
                newChar = NextChar(current.Substring(current.Length - 2));
            }

            return current.Trim();
        }

        public void UpdateModel(string target, List<long> times) {
            string current = "";
            for (int i = 0; i < target.Length; i++) {
                if (i >= 3) current = current.Substring(1);
                if (current.Length == 2) {
                    int charIndex1 = charToInt[current[0]];
                    int charIndex2 = charToInt[current[1]];
                    int targetIndex = charToInt[target[i]];
                    table[charIndex1, charIndex2, targetIndex] -= learningRate * table[charIndex1, charIndex2, targetIndex];

                    double futureBest = 0;
                    for (int j = 0; j < intToChar.Count; j++) {
                        futureBest = Math.Max(futureBest, table[charIndex2, targetIndex, j]);
                    }

                    table[charIndex1, charIndex2, targetIndex] += learningRate * (times[i] + discount * futureBest);
                }

                current += target[i];
            }
        }

        public void FinishRound() {
            Console.Write($"    Round {currentRound + 1} of {numRounds}");
            currentRound++;
            if (currentRound >= numRounds) currentRound = 0;
            
            double roundCompletion = (double)currentRound / (double)numRounds;
            explorationRate = roundCompletion * explorationLow + (1 - roundCompletion) * explorationHigh;

            using (StreamWriter streamWriter = new StreamWriter("model.txt")) {
                for (int i = 0; i < intToChar.Count; i++) {
                    for (int j = 0; j < intToChar.Count; j++) {
                        for (int k = 0; k < intToChar.Count; k++) {
                            if (table[i, j, k] == 0) continue;
                            char char1 = intToChar[i];
                            char char2 = intToChar[j];
                            char char3 = intToChar[k];
                            streamWriter.WriteLine($"{char1}{char2}{char3} {table[i, j, k]}");
                        }
                    }
                }
            }
        }
    }
}