using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityFunctions
{
    public static Vector3 GenerateRandomDirectionXZ()
    {
        float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
        float x = Mathf.Cos(randomAngle);
        float z = Mathf.Sin(randomAngle);
        return new Vector3(x, 0, z);
    }

}

