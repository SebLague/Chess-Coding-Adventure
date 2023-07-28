using UnityEngine;
using Chess;

public class AIDifficulty : MonoBehaviour
{
    public AISettings aiSettings;
    public int difficulty;

    // Call this method to set the AI settings based on the chosen difficulty
    public void SetDifficulty()
    {
        if (difficulty == 1)
        {
            // Random difficulty aka easy
            aiSettings.depth = 1;
            aiSettings.useIterativeDeepening = false;
            aiSettings.useTranspositionTable = false;
            aiSettings.useThreading = false;
            aiSettings.useFixedDepthSearch = false;
            aiSettings.searchTimeMillis = 100;
        }
        else if (difficulty == 2)
        {
            // Weakened difficulty aka medium
            aiSettings.depth = 2;
            aiSettings.useIterativeDeepening = true;
            aiSettings.useTranspositionTable = true;
            aiSettings.useThreading = true;
            aiSettings.useFixedDepthSearch = true;
            aiSettings.searchTimeMillis = 500;
        }
        else if (difficulty == 3)
        {
            // Normal difficulty aka hard
            aiSettings.depth = 7;
            aiSettings.useIterativeDeepening = true;
            aiSettings.useTranspositionTable = true;
            aiSettings.useThreading = true;
            aiSettings.useFixedDepthSearch = true;
            aiSettings.searchTimeMillis = 1000;
        }
    }
}
