using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : PersistableObject
{
    [SerializeField]
    SpawnZone spawnZone;

    public static GameLevel CurrentGameLevel { get; private set; }

    [SerializeField]
    PersistableObject[] persistentObjects;

    //an interface to spawnZone.SpawnPoint
    public Vector3 SpawnPoint
    {
       get{ return spawnZone.SpawnPoint; }
    }

    //OnEnable is useless, unity 2018
    private void Awake()
    {
        CurrentGameLevel = this;
        if(persistentObjects == null)
        {
            persistentObjects = new PersistableObject[0];
        }
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(persistentObjects.Length);
        for(int i=0; i<persistentObjects.Length; i++)
        {
            persistentObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for(int i=0; i<savedCount; i++)
        {
            persistentObjects[i].Load(reader);
        }
    }

    public Shape SpawnShape()
    {
        return spawnZone.SpawnShape();
    }
}
