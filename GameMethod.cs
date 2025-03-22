using System;
using System.Linq;
using System.Collections.Generic;

public class Game
{
    public  int[][] map;
    public  List<int[]> snakeBody;
    public  int[] snakeHead;
    public  int score;
    public  bool isEnd;
    public  Random rand = new Random();


    public int[][] startGame()
    {
        map = new int[6][];
        for (int i = 0; i < 6; i++)
            map[i] = new int[6];

        // Initialize snake
        snakeHead = new int[] { 2, 2 };
        snakeBody = new List<int[]> {  };
        snakeBody.Insert(0,snakeHead);

        // Place snake head and body on the map
        map[snakeHead[0]][snakeHead[1]] = 2;
        // map[snakeBody[0][0]][snakeBody[0][1]] = 1;

        // Place initial food
        placeFood();

        score = 0;
        isEnd = false;

        return map;
    }

    public void placeFood()
    {
        int foodX, foodY;
        do
        {
            foodX = rand.Next(6);
            foodY = rand.Next(6);
        }
        while (map[foodX][foodY] != 0); // Ensure the food doesn't overlap with snake

        map[foodX][foodY] = 3; // Food
    }

    public void printMap()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                switch (map[i][j])
                {
                    case 0:
                        Console.Write(" 0 ");
                        break;
                    case 1:
                        Console.Write(" 1 ");
                        break;
                    case 2:
                        Console.Write(" 2 ");
                        break;
                    case 3:
                        Console.Write(" 3 ");
                        break;
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Score: {score}");
    }

    public int getDirectionFromKey(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                return 0; // Up
            case ConsoleKey.RightArrow:
                return 1; // Right
            case ConsoleKey.DownArrow:
                return 2; // Down
            case ConsoleKey.LeftArrow:
                return 3; // Left
            default:
                return -1; // No movement
        }
    }

    public (int[][], int, bool) nextStep(int direction)
    {
        if (direction == -1) return (map, score, isEnd); // No movement if invalid direction

        // Determine next head position based on current direction
        int[] newHead = (int[])snakeHead.Clone();
        switch (direction)
        {
            case 0: newHead[0]--; break; // Up
            case 1: newHead[1]++; break; // Right
            case 2: newHead[0]++; break; // Down
            case 3: newHead[1]--; break; // Left
        }

        // Check if the new head position is out of bounds or collides with the snake's body
        if (newHead[0] < 0 || newHead[0] >= 6 || newHead[1] < 0 || newHead[1] >= 6 || snakeBody.Any(s => s.SequenceEqual(newHead)))
        {
            return (map, score, true); // Game over
        }

        // Check if the new head position is food
        bool isFood = map[newHead[0]][newHead[1]] == 3;

        // Move the snake
        snakeBody.Insert(0, newHead);
        map[snakeHead[0]][snakeHead[1]] = 1; // Set old head as body
        map[newHead[0]][newHead[1]] = 2; // Set new head

        // If it ate food, increase the score and grow the snake
        if (isFood)
        {
            score++;
            placeFood();
        }
        else
        {
            // Remove the tail of the snake if no food is eaten
            int[] tail = snakeBody.Last();
            map[tail[0]][tail[1]] = 0;
            snakeBody.RemoveAt(snakeBody.Count - 1);
        }

        snakeHead = newHead;

        return (map, score, false); // Game not over
    }
}
