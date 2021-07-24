# KeyAI
[![](https://tokei.rs/b1/github/kashparty/keyai)](https://github.com/KashParty/KeyAI)

KeyAI is a typing tutor with "intelligent learning capabilities". As you learn to type faster, it learns the patterns that slow you down. It then gives you those patterns to practise more often.

KeyAI is currently a console-only application. It uses three files: the first two are `training.txt` and `model.txt`. If these files don't already exist, they will be created by the program. The `training.txt` file contains a corpus used to train the basic text-generation model. The `model.txt` file contains Q-learning values for many n-grams, updated when the user uses the program. Another optional user-created file is `keyai.json`, which contains user preferences for the operation of the program.

The program generates text through a random process. The "words" that are generated sound like English but are usually not real words.

User options now exist! Create a `keyai.json` file in the same directory as the program and change preferences from there. Preferences will take their default values unless specified. I will make a list of preferences soon.
