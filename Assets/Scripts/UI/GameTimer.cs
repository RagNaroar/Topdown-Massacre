using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static float SurvivalTime { get; private set; }

    private void Start()
    {
        SurvivalTime = 0f;
    }

    private void Update()
    {
        SurvivalTime += Time.deltaTime;
    }
}