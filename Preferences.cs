using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace KeyAI {
    class Preferences {
        public string trainingFileUrl { get; set; }
        public string trainingFilePath { get; set; }
        public int lineLength { get; set; }
        public double explorationHigh { get; set; }
        public double explorationLow { get; set; }
        public int numRounds { get; set; }
        public double learningRate { get; set; }
        public double discount { get; set; }

        public Preferences() {
            // Use the default preferences.
            trainingFileUrl = "https://www.gutenberg.org/files/11/11-0.txt";
            trainingFilePath = "training.txt";
            lineLength = 80;
            explorationHigh = 1.0;
            explorationLow = 0.2;
            numRounds = 10;
            learningRate = 0.5;
            discount = 0.95;
        }

        public Preferences(string fileName) : base() {
            if (!File.Exists(fileName)) return;

            using (StreamReader streamReader = new StreamReader(fileName)) {
                string fileString = streamReader.ReadToEnd();
                Dictionary<string, dynamic> data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(fileString);

                if (data.ContainsKey("trainingFileUrl")) trainingFileUrl = data["trainingFileUrl"];
                if (data.ContainsKey("trainingFilePath")) trainingFilePath = data["trainingFilePath"];
                if (data.ContainsKey("lineLength")) lineLength = data["lineLength"];
                if (data.ContainsKey("explorationHigh")) explorationHigh = data["explorationHigh"];
                if (data.ContainsKey("explorationLow")) explorationLow = data["explorationLow"];
                if (data.ContainsKey("numRounds")) numRounds = data["numRounds"];
                if (data.ContainsKey("learningRate")) learningRate = data["learningRate"];
                if (data.ContainsKey("discount")) discount = data["discount"];
            }
        }
    }
}