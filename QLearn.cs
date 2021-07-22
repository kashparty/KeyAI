using System;
using System.IO;
using System.Collections.Generic;

namespace KeyAI {
    class QLearn {
        string trainingData;

        // Two tables: the first for Q-learning values, the second for occurrences of n-grams.
        double[,,] table;
        int[,,] occurrences;

        // Exploration rate varies between the exploration high (at the beginning of a round) and
        // the exploration low (at the end of a round). Higher exploration makes the model rely 
        // more on random generation based on occurrences of n-grams, lower exploration makes the
        // model prioritise the Q-learning values.
        double explorationRate;
        double explorationHigh;
        double explorationLow;

        // The model trains in rounds, so we store the current round and the number of rounds in
        // a full cycle of training.
        int currentRound;
        int numRounds;

        // Used in the Q-learning update equation.
        double learningRate;
        double discount;

        static Random random = new Random();
        
        // We need a two-way mapping from characters to integers and integers to characters.
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

            // Set up the two-way mapping dictionaries: charToInt and intToChar.
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

            // Create the Q-learning table and the n-gram occurrences table, and set up the occurrences.
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

                        // Lines in the stored model are of the form "abc 12.345"
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
            // We are using 3-grams so we only want to consider the previous 2 characters.
            previous = previous.Substring(previous.Length - 2);

            int charIndex1 = charToInt[previous[0]];
            int charIndex2 = charToInt[previous[1]];

            // Greedily choose the character whose Q-learning value is highest.
            char bestChar = '\0';
            double bestScore = 0;

            for (int i = 0; i < intToChar.Count; i++) {
                // Avoid having 3 of the same character in a row, or 2 spaces in a row.
                if (charIndex1 == charIndex2 && charIndex2 == i) continue;
                if (charIndex2 == charToInt[' '] && charIndex2 == i) continue;

                if (table[charIndex1, charIndex2, i] > bestScore) {
                    bestChar = intToChar[i];
                    bestScore = table[charIndex1, charIndex2, i];
                }
            }

            // If all the Q-learning values are zero, then no "best" character exists - just choose randomly.
            if (bestChar == '\0') return RandomChar(previous);

            return bestChar;
        }

        public char RandomChar(string previous) {
            // We are using 3-grams so we only want to consider the previous 2 characters.
            previous = previous.Substring(previous.Length - 2);

            int charIndex1 = charToInt[previous[0]];
            int charIndex2 = charToInt[previous[1]];

            // We use weighted random choice. N-grams that occur more frequently are prioritised.
            int sumOfScores = 0;
            for (int i = 0; i < intToChar.Count; i++) {
                sumOfScores += occurrences[charIndex1, charIndex2, i];
            }

            int randomScore = random.Next(1, sumOfScores + 1);
            int j;
            for (j = 0; j < intToChar.Count; j++) {
                // Avoid having 3 of the same character in a row, or 2 spaces in a row.
                if (charIndex1 == charIndex2 && charIndex2 == j) continue;
                if (charIndex2 == charToInt[' '] && charIndex2 == j) continue;

                // If the random score runs down to zero on this character, then choose this character.
                randomScore -= occurrences[charIndex1, charIndex2, j];
                if (randomScore <= 0) return intToChar[j];
            }

            // If there are no occurrences of the n-gram (unreachable I think) then choose a random character.
            return intToChar[random.Next(intToChar.Count)];
        }

        public char NextChar(string previous) {
            // Based on the exploration rate, pursue a greedy or random strategy for the next character.
            if (random.NextDouble() > explorationRate) return GreedyChar(previous);
            else return RandomChar(previous);
        }

        public string RandomString(int maxLength) {
            // Choose a random prompt string from the original training corpus.
            string current = trainingData.Substring(random.Next(trainingData.Length - 1), 2);
            char newChar = NextChar(current);

            while (current.Length < maxLength) {
                current += newChar;
                newChar = NextChar(current.Substring(current.Length - 2));
            }

            // Leading and trailing whitespace is a hindrance to the user.
            return current.Trim();
        }

        public void UpdateModel(string target, List<long> times) {
            // For each n-gram in the target, update the Q-learning value using the time taken to type the last character.
            string current = "";
            for (int i = 0; i < target.Length; i++) {
                if (i >= 3) current = current.Substring(1);
                if (current.Length == 2) {
                    int charIndex1 = charToInt[current[0]];
                    int charIndex2 = charToInt[current[1]];
                    int targetIndex = charToInt[target[i]];

                    // We break the update equation into parts. First we "forget" some of the old value.
                    table[charIndex1, charIndex2, targetIndex] -= learningRate * table[charIndex1, charIndex2, targetIndex];

                    // Then we calculate the estimated best-case future value.
                    double futureBest = 0;
                    for (int j = 0; j < intToChar.Count; j++) {
                        futureBest = Math.Max(futureBest, table[charIndex2, targetIndex, j]);
                    }

                    // Finally, we update using the reward (time taken) and the discounted best-case future value.
                    table[charIndex1, charIndex2, targetIndex] += learningRate * (times[i] + discount * futureBest);
                }

                current += target[i];
            }
        }

        public void FinishRound() {
            // Display which round it is - this goes here because the model knows which round it is.
            Console.Write($"    Round {currentRound + 1} of {numRounds}");
            currentRound++;

            // Reset the rounds when the final round is over.
            if (currentRound >= numRounds) currentRound = 0;
            
            // Set the new exploration rate using the round number.
            double roundCompletion = (double)currentRound / (double)numRounds;
            explorationRate = roundCompletion * explorationLow + (1 - roundCompletion) * explorationHigh;

            // Write the model data to a text file.
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