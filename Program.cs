using System;
using System.Linq;
using System.Collections.Generic;

class SnakeGame
{
    private static int[][] map;
    private static List<int[]> snakeBody;
    private static int[] snakeHead;
    private static int score;
    private static bool isEnd;
    private static Random rand = new Random();
    private static SnakeAI ai;
    private const int maxMatric = 4;

    static async Task Main(string[] args)
    {
        // await LinearEquationAutoPlay();
        await TrainProcess(false);
    }

    static async Task LinearEquationAutoPlay()
    {
        QLearningApproximator qFunc = new QLearningApproximator("./train_2.json");
        int episodes = 10; // 訓練的回合數
        for (int i = 0; i < episodes; i++)
        {
            Console.WriteLine($"Episode {i + 1}/{episodes}");
            StartGame();
            bool isWin = false;
            while (!isEnd)
            {
                // 獲取當前狀態
                string state = GetState();
                int[] previousHeadPos = snakeHead;
                // AI 選擇行動
                // int action = ai.ChooseAction(state);
                int action = qFunc.ChooseAction(state);
                // 執行行動並獲取結果
                var result = NextStep(action);
                map = result.map;
                int reward = result.reward;
                isEnd = result.isEnd;
                score = result.score;
                if (score >= (maxMatric * maxMatric - 3))
                {
                    isWin = true;
                }
                // 獲取下一個狀態
                string nextState = GetState();
                printMap();
                await Task.Delay(500);
            }
            Console.WriteLine($"Episode {i + 1} ended with score: {score}");
        }
        Console.WriteLine("Training completed.");
    }

    static async Task TrainProcess(bool isTrain = false)
    {
        double randomRate = 1;
        ai = new SnakeAI(isTrain ? randomRate : 0);

        if (isTrain)
        {
            int winCount = 0;
            Console.WriteLine("Starting Snake Game with AI...");
            for (int round = 0; round < 100; round++)
            {
                int episodes = 10000; // 訓練的回合數
                for (int i = 0; i < episodes; i++)
                {
                    Console.WriteLine($"Episode {i + 1}/{episodes}");
                    StartGame();
                    int noScoreStep = 0;
                    bool isWin = false;
                    while (!isEnd)
                    {
                        int previousScore = score;
                        // 獲取當前狀態
                        string state = GetState();
                        int[] previousHeadPos = snakeHead;
                        // AI 選擇行動
                        int action = ai.ChooseAction(state);
                        // 執行行動並獲取結果
                        var result = NextStep(action);
                        map = result.map;
                        int reward = result.reward;
                        isEnd = result.isEnd;
                        score = result.score;
                        if (score == previousScore)
                        {
                            noScoreStep++;
                        }
                        else
                        {
                            noScoreStep = 0;
                        }
                        if (noScoreStep > 100)
                        {
                            isEnd = true;
                        }
                        if (score >= (maxMatric * maxMatric - 3))
                        {
                            isWin = true;
                            winCount++;
                        }
                        // 獲取下一個狀態
                        string nextState = GetState();
                        // 更新 Q 值
                        ai.UpdateQValue(state, action, reward, nextState, map, previousHeadPos, isEnd, isWin, noScoreStep, score);
                        if (isWin)
                        {
                            break;
                        }
                    }
                    Console.WriteLine($"Episode {i + 1} ended with score: {score}");
                }
            }
            ai.SaveQTable();
            Console.WriteLine($"win:{winCount}");
            Console.WriteLine("Training completed.");
        }
        else
        {
            int episodes = 10; // 訓練的回合數
            for (int i = 0; i < episodes; i++)
            {
                Console.WriteLine($"Episode {i + 1}/{episodes}");
                StartGame();
                bool isWin = false;
                while (!isEnd)
                {
                    // 獲取當前狀態
                    string state = GetState();
                    int[] previousHeadPos = snakeHead;
                    // AI 選擇行動
                    int action = ai.ChooseAction(state);
                    // 執行行動並獲取結果
                    var result = NextStep(action);
                    map = result.map;
                    int reward = result.reward;
                    isEnd = result.isEnd;
                    score = result.score;
                    if (score >= (maxMatric * maxMatric - 3))
                    {
                        isWin = true;
                    }
                    // 獲取下一個狀態
                    string nextState = GetState();
                    printMap();
                    await Task.Delay(1000);
                }
                Console.WriteLine($"Episode {i + 1} ended with score: {score}");
            }
            Console.WriteLine("Training completed.");
        }
    }

    static void StartGame()
    {
        map = new int[maxMatric][];
        for (int i = 0; i < maxMatric; i++)
            map[i] = new int[maxMatric];

        // 初始化蛇的位置
        snakeHead = new int[] { 2, 2 };
        snakeBody = new List<int[]> { new int[] { 2, 1 } };

        // 將蛇頭和身體放在地圖上
        map[snakeHead[0]][snakeHead[1]] = 2;
        map[snakeBody[0][0]][snakeBody[0][1]] = 1;

        // 放置初始食物
        PlaceFood();

        score = 0;
        isEnd = false;
    }

    static void PlaceFood()
    {
        int foodX, foodY;
        do
        {
            foodX = rand.Next(maxMatric);
            foodY = rand.Next(maxMatric);
        }
        while (map[foodX][foodY] != 0); // 確保食物不與蛇重疊

        map[foodX][foodY] = 3; // 食物
    }

    static string GetState()
    {
        // 將地圖轉換為字符串作為狀態表示
        return string.Join(",", map.SelectMany(row => row));
    }

    static (int[][] map, int reward, bool isEnd, int score) NextStep(int direction)
    {
        // 根據當前方向確定下一個蛇頭位置
        int[] newHead = (int[])snakeHead.Clone();
        switch (direction)
        {
            case 0: newHead[0]--; break; // 上
            case 1: newHead[1]++; break; // 右
            case 2: newHead[0]++; break; // 下
            case 3: newHead[1]--; break; // 左
        }

        // 檢查新蛇頭位置是否越界或撞到蛇身
        if (newHead[0] < 0 || newHead[0] >= maxMatric || newHead[1] < 0 || newHead[1] >= maxMatric || snakeBody.Any(s => s.SequenceEqual(newHead)))
        {
            return (map, -100, true, score); // 遊戲結束，給予大的負獎勵
        }

        // 檢查新蛇頭位置是否有食物
        bool isFood = map[newHead[0]][newHead[1]] == 3;

        // 移動蛇
        snakeBody.Insert(0, (int[])snakeHead.Clone());
        map[snakeHead[0]][snakeHead[1]] = 1; // 將舊的蛇頭設為蛇身
        snakeHead = newHead;
        map[snakeHead[0]][snakeHead[1]] = 2; // 設置新的蛇頭

        if (isFood)
        {
            score++;
            PlaceFood();
            return (map, 10, false, score); // 吃到食物，給予正獎勵
        }
        else
        {
            // 移除蛇尾
            int[] tail = snakeBody.Last();
            map[tail[0]][tail[1]] = 0;
            snakeBody.RemoveAt(snakeBody.Count - 1);
            return (map, -1, false, score); // 每移動一步，給予小的負獎勵
        }
    }

    static void printMap()
    {
        Console.Clear();
        for (int i = 0; i < maxMatric; i++)
        {
            for (int j = 0; j < maxMatric; j++)
            {
                switch (map[i][j])
                {
                    case 0:
                        Console.Write(" □ ");
                        break;
                    case 1:
                        Console.Write(" ■ ");
                        break;
                    case 2:
                        Console.Write(" ▣ ");
                        break;
                    case 3:
                        Console.Write(" ◈ ");
                        break;
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Score: {score}");
    }
}
