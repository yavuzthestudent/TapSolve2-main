using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<int> OnMoveChanged; //When move count changes

    public static event Action<int> OnLevelNumberChanged; //When level number changes

    public static event Action<CubeController> OnCubeCleared; //When cube destroyed 

    public static event Action OnLevelFail; //When game is over

    public static event Action OnLevelCompleted; //When level is completed

    public static event Action OnMoveRequested; //When move is requested by player

    public static void RaiseMoveChanged(int newMoves)
    {
        OnMoveChanged?.Invoke(newMoves);
    }
    
    public static void RaiseLevelNumberChanged(int levelNumber)
    {
        OnLevelNumberChanged?.Invoke(levelNumber);
    }

    public static void RaiseCubeCleared(CubeController cube)
    {
        OnCubeCleared?.Invoke(cube);
    }   

    public static void RaiseLevelFail()
    {
        OnLevelFail?.Invoke();
    }

    public static void RaiseLevelComplete()
    {
        OnLevelCompleted?.Invoke();
    }

    public static void RaiseMoveRequested()
    {
        OnMoveRequested?.Invoke();
    }
}
