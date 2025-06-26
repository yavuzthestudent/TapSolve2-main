using System;
using UnityEngine;

[Serializable]
public class CubeData : MonoBehaviour
{
    public Direction Direction;
    public Color Color;

    public CubeData(Direction direction, Color color)
    {
        this.Direction = direction;
        this.Color = color;
    }
}
