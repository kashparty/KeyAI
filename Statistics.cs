using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KeyAI {
    [Serializable]
    class Statistics {
        public long numLines { get; set; }
        public long numChars { get; set; }
        public long numMistakes { get; set; }
        public double maxWPM { get; set; }
        public double maxAccuracy { get; set; }


        public Statistics() {
            numLines = 0;
            numChars = 0;
            numMistakes = 0;
            maxWPM = 0;
            maxAccuracy = 0;
        }

        public bool TryRead() {
            bool success;

            if (!File.Exists("keyai_stats")) return false;
            using (FileStream fileStream = new FileStream("keyai_stats", FileMode.Open)) {
                try {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Statistics data = (Statistics)binaryFormatter.Deserialize(fileStream);

                    numLines = data.numLines;
                    numChars = data.numChars;
                    numMistakes = data.numMistakes;
                    maxWPM = data.maxWPM;
                    maxAccuracy = data.maxAccuracy;

                    success = true;
                } catch (SerializationException e) {
                    Console.WriteLine(e.Message);
                    success = false;
                }
            }

            return success;
        }

        public bool TryWrite() {
            bool success;
            using (FileStream fileStream = new FileStream("keyai_stats", FileMode.Create)) {
                try {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, this);
                    success = true;
                } catch (SerializationException) {
                    success = false;
                }
            }

            return success;
        }
    }
}