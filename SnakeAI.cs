using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class SnakeAI
{
    private SqliteConnection dbConnection;
    private Dictionary<string, Dictionary<int, double>> qTable;
    private double learningRate = 0.1;
    private double discountFactor = 0.9;
    private double explorationRate = 1.0;
    private double minExplorationRate = 0.1;
    private double explorationDecay = 0.995;
    private Random rand = new Random();

    public SnakeAI(double ExplorationRate)
    {
        explorationRate = ExplorationRate;
        dbConnection = new SqliteConnection("Data Source=./snake_ai.db;");
        dbConnection.Open();
        CreateTable();
        LoadQTable();
    }

    private void CreateTable()
    {
        string sql = "CREATE TABLE IF NOT EXISTS QTable (State TEXT PRIMARY KEY, Actions TEXT);";
        using var command = new SqliteCommand(sql, dbConnection);
        command.ExecuteNonQuery();
    }

    private void LoadQTable()
    {
        qTable = new Dictionary<string, Dictionary<int, double>>();
        string sql = "SELECT * FROM QTable";
        using var command = new SqliteCommand(sql, dbConnection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string state = reader.GetString(0);
            var actions = reader.GetString(1).Split(',').Select(double.Parse).ToList();
            qTable[state] = new Dictionary<int, double> { { 0, actions[0] }, { 1, actions[1] }, { 2, actions[2] }, { 3, actions[3] } };
        }
    }

    public void SaveQTable()
    {
        foreach (var state in qTable)
        {
            string actions = string.Join(",", state.Value.Values);
            string sql = "INSERT OR REPLACE INTO QTable (State, Actions) VALUES (@state, @actions)";
            using var command = new SqliteCommand(sql, dbConnection);
            command.Parameters.AddWithValue("@state", state.Key);
            command.Parameters.AddWithValue("@actions", actions);
            command.ExecuteNonQuery();
        }
    }

    public int ChooseAction(string state)
    {
        if (!qTable.ContainsKey(state))
            qTable[state] = new Dictionary<int, double> { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };

        if (rand.NextDouble() < explorationRate)
            return rand.Next(4);
        else
            return qTable[state].OrderByDescending(a => a.Value).First().Key;
    }

    // public void UpdateQValue1(string state, int action, double reward, string nextState)
    // {
    //     if (!qTable.ContainsKey(state))
    //         qTable[state] = new Dictionary<int, double> { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };

    //     double oldQ = qTable[state][action];
    //     double maxFutureQ = qTable.ContainsKey(nextState) ? qTable[nextState].Values.Max() : 0;
    //     double newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);
    //     qTable[state][action] = newQ;

    //     explorationRate = Math.Max(minExplorationRate, explorationRate * explorationDecay);
    //     SaveQTable();
    // }
    public void UpdateQValue(string state, int action, double reward, string nextState, int[][] map, int[] previousHeadPosition, bool isGameOver)
    {
        // 初始化狀態的 Q 值
        if (!qTable.ContainsKey(state))
            qTable[state] = new Dictionary<int, double> { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };

        // 計算蛇頭與餅乾的距離變化
        int[] currentHeadPosition = GetSnakeHeadPosition(map);
        int[] foodPosition = GetFoodPosition(map);
        double previousDistance = CalculateDistance(previousHeadPosition, foodPosition);
        double currentDistance = CalculateDistance(currentHeadPosition, foodPosition);

        // 如果蛇頭比上一步更接近餅乾，則增加獎勵
        if (currentDistance < previousDistance)
        {
            reward += 1;
        }

        // 如果遊戲結束，則給予大的負獎勵
        if (isGameOver)
        {
            reward -= 10;
        }

        // 獲取當前狀態下該行動的舊 Q 值
        double oldQ = qTable[state][action];

        // 獲取下一狀態的最大 Q 值
        double maxFutureQ = qTable.ContainsKey(nextState) ? qTable[nextState].Values.Max() : 0;

        // 計算新的 Q 值
        double newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);
        qTable[state][action] = newQ;

        // 更新探索率，逐漸減少隨機探索
        explorationRate = Math.Max(minExplorationRate, explorationRate * explorationDecay);

        // 保存 Q 表到資料庫
        // SaveQTable();
    }

    // 計算兩點之間的歐幾里得距離
    private double CalculateDistance(int[] pos1, int[] pos2)
    {
        return Math.Sqrt(Math.Pow(pos1[0] - pos2[0], 2) + Math.Pow(pos1[1] - pos2[1], 2));
    }

    // 獲取蛇頭在地圖上的位置
    private int[] GetSnakeHeadPosition(int[][] map)
    {
        for (int i = 0; i < map.Length; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                if (map[i][j] == 2) // 2 代表蛇頭
                {
                    return new int[] { i, j };
                }
            }
        }
        return null;
    }

    // 獲取餅乾在地圖上的位置
    private int[] GetFoodPosition(int[][] map)
    {
        for (int i = 0; i < map.Length; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                if (map[i][j] == 3) // 3 代表餅乾
                {
                    return new int[] { i, j };
                }
            }
        }
        return null;
    }


}
