using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
class PolicyIO
{
    public static void Serialize(List<Tuple<QLearning.CommandListAbstract, float>>[] input, string filePath)
    {
        using(var writer = new StreamWriter(filePath))
        {
            for (int i=0; i<input.Length; i++)
            {
                writer.WriteLine(input[i].Count);
                for (int j=0; j<input[i].Count; j++)
                {
                    Maneuver.ID maneuver = input[i][j].Item1[0];//Some c# garbage where this refuses to work like a c++ map with operator[].
                    float score = input[i][j].Item2;
                    writer.WriteLine(maneuver.ToString() + "," + score.ToString());
                }
            }
        }

        AssetDatabase.ImportAsset("Assets/Resources/policy.csv");
    }

    public static void Deserialize(List<Tuple<QLearning.CommandListAbstract, float>>[] output, string filePath)
    {
        if (output.Length != 0)
        {
            throw new Exception("You're gay. This array shouldn't be initialized unless you can justify being edgy.");
        }

        using(var reader = new StreamReader(filePath))
        {
            output = new List<Tuple<QLearning.CommandListAbstract, float>>[int.Parse(reader.ReadLine())];
            for(int i=0; i<output.Length; i++)
            {
                output[i] = new List<Tuple<QLearning.CommandListAbstract, float>>(int.Parse(reader.ReadLine()));
                for(int j=0; j<output[i].Count; j++)
                {
                    string line = reader.ReadLine();
                    Maneuver.ID maneuver = (Maneuver.ID)line[0];
                    float score = line[2];
                    QLearning.CommandListAbstract maneuvers = new QLearning.CommandListAbstract();
                    maneuvers.Add(0, maneuver);
                    Tuple< QLearning.CommandListAbstract, float> scoredManeuver = Tuple.Create(maneuvers, score);
                    output[i][j] = scoredManeuver;
                }
            }
        }
    }
}

//struct ScoredManeuver
//{
//    Maneuver.ID maneuver;
//    float score;
//}
//
//What the datastructure actually looks like:
//List<ScoredManeuver>[] policy;

//Simplifying the container:
//1. List<Tuple<CommandListAbstract, float>>[] policy;
//2. List<Tuple<Dictionary<int, Maneuver>, float>[] policy;
//3. Only 1 plane, so we can reduce to
//List<Tuple<Maneuver, float>[] policy;
//4. Tuples are gay so we define ScoredManeuver
//List<ScoredManeuver>[] policy;

//Sample file:
//3
//2
//2 0.3
//7 0.9
//1
//5 0.4
//2
//4 0.8
//6 0.1
//The first number is the number of states, the next part of the file specifies each state's maneuvers first by specifying the count then each row contains the id and score.
//The above file had 3 states, the 0th state had 2 maneuvers; a CLIMB with a 0.3 score and a RIGHT120 with a 0.9 score, the 1st state had 1 maneuver; a LEFT120 with a 0.4 score etc.
