// using System;
// using System.Linq;
// using System.Collections.Generic;

// class SnakeGame
// {
//     static void Main(string[] args)
//     {
//         Console.WriteLine("Starting Snake Game...");
//         var game = new Game();
//         game.startGame();
//         while (!game.isEnd)
//         {
//             Console.Clear();
//             game.printMap();
//             ConsoleKeyInfo keyInfo = Console.ReadKey(true);
//             int direction = game.getDirectionFromKey(keyInfo.Key);
//             var result = game.nextStep(direction);
//             game.map = result.Item1;
//             game.score = result.Item2;
//             game.isEnd = result.Item3;
//         }
//         Console.Clear();
//         game.printMap();
//         Console.WriteLine($"Game Over! Score: {game.score}");
//     }
// }
