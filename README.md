# KeyAI

[![](https://tokei.rs/b1/github/kashparty/keyai)](https://github.com/kashparty/keyai)

KeyAI is a typing tutor with "intelligent learning capabilities". As you learn to type faster, it learns the patterns that slow you down. It then gives you those patterns to practise more often.

## Usage

You can access the typing tutor by running the program and selecting the `Typing tutor` option in the main menu. You will see a line of randomly generated text appear. Type out the words and the cursor will move accordingly - the cursor will only move from its position once you have correctly typed the character at that position. By default, a line contains only English letters (lowercase and uppercase) and spaces, and is at most 80 characters long.

Once you begin typing, the time taken to correctly type each character is measured. This data is used to calculate your typing speed for that line, in Words Per Minute (WPM) - this will be shown to you once you have completed the line. Furthermore, after completing the line, the data will also be used to train the Q-learning AI model. Over time, the model will be able to identify which sequences of characters are the most difficult for you, and you will see these sequences more often.

The tutor performs its training in a sequence of rounds. In each round, you will type out one line of text. A sequence of rounds forms a cycle - by default, one cycle is equivalent to 10 rounds. Once a full cycle is complete, the next cycle begins. This process continues indefinitely (until the user decides to stop).

Over the course of one cycle, the behaviour of the tutor changes. At the beginning of a cycle, the tutor will give you text that is entirely randomly generated (but still English-like). This will not take into account your typing performance on various patterns of characters. As the cycle goes on, the tutor becomes more and more likely to challenge you by giving you sequences of characters that you have struggled with before.

You can exit the typing tutor at any point by pressing the `Escape` key on your keyboard.

## User preferences

You can customise a variety of aspects of the typing tutor's behaviour. In order to change some of these values from their defaults, create a file `keyai.json` in the same directory as the program executable, and change them as shown:

```json
{
    "trainingFileUrl": "https://www.gutenberg.org/files/98/98-0.txt",
    "lineLength": 100,
    "learningRate": 0.25,
    "color": "Magenta"
}
```

The following table describes the parameters that the user can change. At the moment, I haven't tested the default parameters very much, so there is no guarantee that these are good defaults. The program will currently not work if you set `includeUppercase`, `includeLowercase`, `includeDigits` and `includePunctuation` all to false.

| Name                 | Type    | Description                                                  | Default                                       |
| :------------------- | ------- | ------------------------------------------------------------ | --------------------------------------------- |
| `trainingFileUrl`    | String  | The URL from which to download the corpus used by the model to generate English-like text. | `https://www.gutenberg.org/files/11/11-0.txt` |
| `trainingFilePath`   | String  | The location of the text file where the corpus should be stored. | `training.txt`                                |
| `lineLength`         | Integer | The maximum length (in characters) of a line of text that the user types in one round. | `80`                                          |
| `explorationHigh`    | Double  | The probability of a character being produced randomly (from the corpus) as opposed to via the Q-learning algorithm at the beginning of a cycle. | `1.0`                                         |
| `explorationLow`     | Double  | The probability of a character being produced randomly (from the corpus) as opposed to via the Q-learning algorithm at the end of a cycle. | `0.2`                                         |
| `numRounds`          | Integer | The number of rounds in a cycle.                             | `10`                                          |
| `learningRate`       | Double  | The learning rate of the Q-learning algorithm, used in the Q-learning update equation. | `0.5`                                         |
| `discount`           | Double  | The discount factor used in the Q-learning update equation.  | `0.9`                                         |
| `color`              | String  | The color used to show completed characters in the typing tutor. One of `Black`, `Blue`, `Cyan`, `DarkBlue`, `DarkCyan`, `DarkGray`, `DarkGreen`, `DarkMagenta`, `DarkRed`, `DarkYellow`, `Gray`, `Green`, `Magenta`, `Red`, `White` or `Yellow`. | `Green`                                       |
| `includeUppercase`   | Boolean | Whether to include uppercase characters in the typing tutor. | `true`                                        |
| `includeLowercase`   | Boolean | Whether to include lowercase characters in the typing tutor. You probably want to keep this set to `true`. | `true`                                        |
| `includeDigits`      | Boolean | Whether to include the digits 0 to 9 in the typing tutor.    | `false`                                       |
| `includePunctuation` | Boolean | Whether to include the characters `.,!:;"'-()` in the typing tutor. | `false`                                       |
