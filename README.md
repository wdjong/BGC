# BGC
Backgammon C# WPF
Backgammon: originally written in BASIC in 1982 on a VZ200 based on the Tandy TRS80 2 player game. (Sold via Dick Smith shops in a cassette version).
Over the years, based on available hardware and software it's been ported to various platforms in order for me to learn new things. (I.e. As features became available: e.g. File system, Graphics, .NET, Database, VB -> C#, Winforms -> WPF) and as I developed ideas of how to improve the opponent; probability, machine learning concepts. e.g. Osborne, IBM PC (MBasic), Assembly language (DEC Rainbow CPM86), VB 4 1998, VB 6 2001, VB.Net 2004, VS2005 ~2009, VS2008 ~2010, 2012VB ~2015
Windows Store App (WPF / XAML) ~2016

Temperament
The computer opponent uses a basic method of scoring each possible move and then evaluating the resulting board situation weighting various criteria by a number of factor. These factor relate to strategy elements or qualities that more or less equate to temperaments. E.g. the tendency to want to take opponents pieces is called 'aggression'. By playing many games against itself and other computer opponents, these factors have been optimised for general play. 
In order to allow the computer to 'fine tune' its play to suite playing against you, there is a capacity to weight various characteristics, and prefer to use more successful strategies in response to success. The results of this can be seen in the Tools Settings screen. 
A history of all game results is stored in gamehist.txt. (This can be used as a machine learning dataset). The left column is the the temperament of the winner.
Commands
File menu
Save
Save the current state of a game.
Open
Load a saved game.
Dump
Save the dice throws and current board state as a text file. (For diagnostic purposes).
Exit
Close the program.
Play menu
Human
Play against another human... or yourself.
Computer
Play against the computer.
Auto
Let the computer play itself (for learning purposes)
Tools
Settings
Save your preferences for the following;
Number of games computers play against each other. This was used in trying out different weightings to optimise the strategy. (Reinforcement learning)
Number of seconds to pause after each move. In case you want to watch a game in slow motion.
If you don't want to see every computer move, choose false. This maximises the speed of computer vs computer games
If you don't want to see the results of every game choose false. Normally, when playing many computer games it's nice to view the progress. 
Player 0: (who you play against) 0 random. This is normal setting. It picks a temperament (from those below) based on their relative success in winning games. Apart from 7 (norm), the relevant characteristic is doubled. At the moment the 'adjacent / blocking / clumping' tendency has a negative value because that tendency resulted in poorer results so, when 5 is selected, it's less likely to try to make adjacent points.' , 1 running / speed, 2 make points / blocking, 3 blitz / aggression, 4 not blot / safety, 5 adjacent / blocking, 6 bear in / endgame, 7 norm / no weighting 
Player 1: (the other computer opponent) 0 random, 1 running / speed, 2 make points / blocking, 3 blitz / aggression, 4 not blot / safety, 5 adjacent / blocking, 6 bear in / endgame, 7 norm / no weighting
Reset Learning
Remove any learned information

Future
Reinforcement learning (Strategy). Deep Learning (Neural networks)
