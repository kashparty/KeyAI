# KeyAI

KeyAI is a typing tutor with "intelligent learning capabilities". As you learn to type faster, it learns the patterns that slow you down. It then gives you those patterns to practise more often.

KeyAI is currently a console-only application. It uses two files: `training.txt` and `model.txt`. If the files don't already exist, they will be created by the program. The `training.txt` file contains a corpus used to train the basic text-generation model. The `model.txt` file contains Q-learning values for many n-grams, updated when the user uses the program.

The program generates text through a random process. The "words" that are generated sound like English but are usually not real words.

User options are nonexistent at the moment. The default options are baked into the program, and can only be changed by altering the source code.