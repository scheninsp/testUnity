using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    [SerializeField]
    SpawnZone spawnZone;

    void Start()
    {
        // Debug.Log(spawnZone.name);
       // Debug.Log(Game_2.Instance.SpawnZoneOfLevel);
        Game_2.Instance.SpawnZoneOfLevel = spawnZone;
    }


}
