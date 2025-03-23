using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Analysis;

class QLearningApproximator
{
    private Dictionary<string, Dictionary<int, double>> qTable;
    private MultivariateLinearRegression regression;

    public QLearningApproximator(string jsonFilePath)
    {
        // 加載 JSON 文件
        string json = System.IO.File.ReadAllText(jsonFilePath);
        qTable = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, double>>>(json);

        // 準備訓練資料
        var features = new List<double[]>();
        var targets = new List<double[]>();
        // var targets = new List<List<double>> { new List<double>(), new List<double>(), new List<double>(), new List<double>() };

        foreach (var kvp in qTable)
        {
            // 將地圖狀態字符串解析為特徵向量
            double[] featureVector = kvp.Key.Split(',').Select(double.Parse).ToArray();
            features.Add(featureVector);

            targets.Add(kvp.Value.Select(x => x.Value).ToArray());

            // // 添加對應的 Q 值
            // for (int action = 0; action < 4; action++)
            // {
            //     targets[action].Add(kvp.Value[action]);
            // }
        }

        // 將列表轉換為陣列
        double[][] featureArray = features.ToArray();
        double[][] targetArrays = targets.ToArray();
        OrdinaryLeastSquares ols = new OrdinaryLeastSquares()
        {
            UseIntercept = true // 設為 false 以排除截距項
        };

        regression = ols.Learn(featureArray, targetArrays);

        // // 訓練每個動作的回歸模型
        // for (int action = 0; action < 4; action++)
        // {
        //     regressions[action] = ols.Learn(featureArray, targetArrays[action]);
        // }
    }

    public int ChooseAction(string state)
    {
        double[] stateFeatures = state.Split(',').Select(double.Parse).ToArray();
        double[] qValues = regression.Transform(stateFeatures);
        return Array.IndexOf(qValues, qValues.Max());
    }
}

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Newtonsoft.Json;
// using Accord.Statistics.Models.Regression.Linear;
// using Accord.MachineLearning;

// class QLearningApproximator
// {
//     private Dictionary<string, Dictionary<int, double>> qTable;
//     private MultipleLinearRegression[] regressions;

//     public QLearningApproximator(string jsonFilePath)
//     {
//         // 加载 JSON 文件
//         string json = System.IO.File.ReadAllText(jsonFilePath);
//         qTable = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, double>>>(json);

//         // 初始化回归模型数组
//         regressions = new MultipleLinearRegression[4];

//         // 准备训练数据
//         var features = new List<double[]>();
//         var targets = new List<List<double>> { new List<double>(), new List<double>(), new List<double>(), new List<double>() };

//         foreach (var kvp in qTable)
//         {
//             // 将状态字符串解析为特征向量
//             double[] featureVector = kvp.Key.Split(',').Select(double.Parse).ToArray();
//             features.Add(featureVector);

//             // 添加对应的 Q 值
//             for (int action = 0; action < 4; action++)
//             {
//                 targets[action].Add(kvp.Value[action]);
//             }
//         }

//         // 将列表转换为数组
//         double[][] featureArray = features.ToArray();
//         double[][] targetArrays = targets.Select(t => t.ToArray()).ToArray();
//         OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

//         // 训练每个动作的回归模型
//         for (int action = 0; action < 4; action++)
//         {
//             regressions[action] = ols.Learn(featureArray, targetArrays[action]);
//         }
//     }

//     public int ChooseAction(string state)
//     {
//         double[] stateFeatures = state.Split(',').Select(double.Parse).ToArray();
//         double[] qValues = new double[4];
//         for (int action = 0; action < 4; action++)
//         {
//             qValues[action] = regressions[action].Transform(stateFeatures);
//         }
//         return Array.IndexOf(qValues, qValues.Max());
//     }
// }
