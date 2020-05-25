﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeSpawnZone : SpawnZone
{
    [SerializeField]
    SpawnZone[] spawnZones;

    [SerializeField]
    bool sequential;
    int nextSequentialIndex = 0;

    [SerializeField]
    bool overrideSpawnConfig;

    public override Vector3 SpawnPoint
    {
        get{
            int index;
            if (sequential)   //sequentially using spawnZones
            {
                Debug.Log("get called");
                index = nextSequentialIndex++;
                if(nextSequentialIndex >= spawnZones.Length)
                {
                    nextSequentialIndex = 0;
                }
            }
            else{
                index = Random.Range(0, spawnZones.Length);
            }
            return spawnZones[index].SpawnPoint;
        }

    }

   public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(nextSequentialIndex);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        nextSequentialIndex = reader.ReadInt();
    }

    public override void SpawnShapes()
    {
        Shape shape;
        if (overrideSpawnConfig)
        {
            //use default configure
            base.SpawnShapes();
        }
        else
        {
            int index;
            if (sequential)   //sequentially using spawnZones
            {
                Debug.Log("spawnZone Index : " + nextSequentialIndex);
                index = nextSequentialIndex++;
                if (nextSequentialIndex >= spawnZones.Length)
                {
                    nextSequentialIndex = 0;
                }
            }
            else
            {
                index = Random.Range(0, spawnZones.Length);
            }
            spawnZones[index].SpawnShapes();
        }
    }
}
