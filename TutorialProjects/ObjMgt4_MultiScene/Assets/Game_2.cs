using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_2 : PersistableObject
{
    public PersistableStorage storage;

    public ShapeFactory shapeFactory;

    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public KeyCode destroyKey = KeyCode.X;
    public KeyCode testKey = KeyCode.T;

    List<Shape> shapes;

    const int saveVersion = 1;

    public float CreationSpeed { get; set; }
    float creationProgress;

    public float DestructionSpeed { get; set; }
    float destructionProgress;

    private void Awake()
    {
        shapes = new List<Shape>();
        Debug.Log(Application.persistentDataPath);
        Application.targetFrameRate = 60;

        //StartCoroutine(LoadLevel());

    }

    private void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            creationProgress = 0;
            CreateShape();
        }
        else if (Input.GetKey(createKey))
        {
            //automatic continuous creation
            creationProgress += Time.deltaTime * CreationSpeed;
            while (creationProgress >= 1)
            {
                creationProgress -= 1;
                CreateShape();
            }
        }

        else if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
            storage.Load(this);
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            destructionProgress = 0;
            DestroyShape();
        }
        else if (Input.GetKey(destroyKey))
        {
            //automatic continuous creation
            destructionProgress += Time.deltaTime * DestructionSpeed;
            while (destructionProgress >= 1)
            {
                destructionProgress -= 1;
                DestroyShape();
            }
        }
        else if (Input.GetKeyDown(testKey))
        {
            StartCoroutine(LoadLevel());
            //SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);
            //Scene sceneTmp = SceneManager.GetSceneByName("Level 1");
            //Debug.Log("GetSceneByName : " + sceneTmp.name);
            //LoadLevel();
        }


    }

    void BeginNewGame()
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            //Destroy(shapes[i].gameObject);
            shapeFactory.Reclaim(shapes[i]);

        }
        shapes.Clear();
    }

    void CreateShape()
    {
        Shape o = shapeFactory.GetRandom();
        Transform t = o.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        o.SetColor(Random.ColorHSV(hueMin : 0f, hueMax : 1f, 
            saturationMin : 0.5f, saturationMax : 1f, valueMin : 0.25f,valueMax : 1f,
            alphaMin : 1f, alphaMax : 1f));
        shapes.Add(o);
        //Debug.Log("created : " + o.ShapeId);

    }

    //destroy shape randomly
    void DestroyShape() {
        if(shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count);
            //Destroy(shapes[index].gameObject);
            shapeFactory.Reclaim(shapes[index]);
            Debug.Log("destroy:" + index);

            int lastIndex = shapes.Count - 1;
            //move last to deleted position, fill in array
            shapes[index] = shapes[lastIndex]; 
            shapes.RemoveAt(lastIndex);

        }
    }


    public override void Save(GameDataWriter writer)
    {
        writer.Write(saveVersion);
        writer.Write(shapes.Count);
        for (int i = 0; i < shapes.Count; i++)
        {
            //Debug.Log("writing : " + shapes[i].ShapeId);
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        Debug.Log("Loading...");
        int version = reader.ReadInt();
        if(version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        int count = reader.ReadInt();
        //Debug.Log(count);

        for (int i = 0; i < count; i++)
        {
            int shapeId = reader.ReadInt();
            int materialId = reader.ReadInt();
            // Debug.Log(shapeId);

            Shape o = shapeFactory.Get(shapeId, materialId);
            o.Load(reader);
            shapes.Add(o);
        }
    }

    IEnumerator LoadLevel()
    {
        SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);

        Scene sceneTmp = SceneManager.GetSceneByName("Level 1");
        //Debug.Log("GetSceneByName : " + sceneTmp.name);

        //while (!sceneTmp.isLoaded) { yield return null; }//wait for loading scene

        yield return null;
        SceneManager.SetActiveScene(sceneTmp);
    }
}
