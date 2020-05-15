using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PersistableStorage : MonoBehaviour
{
    string savePath;
    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    public void Save(PersistableObject o)
    {
        using (  //automatically handle exception of closing file
          BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create))
      )
        {
            o.Save(new GameDataWriter(writer));
        }
    }

    public void Load(PersistableObject o)
    {
        using (  //automatically handle exception of closing file
          BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open))
      )
        {
            o.Load(new GameDataReader(reader));
        }
    }
}
