using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_2 : PersistableObject
{
    [SerializeField]
    PersistableStorage storage;

    [SerializeField]
    KeyCode createKey = KeyCode.C;
    [SerializeField]
    KeyCode newGameKey = KeyCode.N;
    [SerializeField]
    KeyCode saveKey = KeyCode.S;
    [SerializeField]
    KeyCode loadKey = KeyCode.L;
    [SerializeField]
    KeyCode destroyKey = KeyCode.X;
    [SerializeField]
    KeyCode testKey = KeyCode.T;

    List<Shape> shapes;
    bool inGameUpdateLoop;  //a lock for shapes list

    List<ShapeInstance> killList;  //temporarily saving shapes to kill

    int dyingShapeCount;
    List<ShapeInstance> markAsDyingList;

    const int saveVersion = 7;
    //ver.2 with level saved
    //ver.3 with random state saved
    //ver.4 with level state saved
    //ver.5 shape configuration added
    //ver.6 multiple factory
    //ver.7 spawnZone self generation

    public float CreationSpeed { get; set; }
    float creationProgress;
    [SerializeField] Slider creationSpeedSlider;

    public float DestructionSpeed { get; set; }
    float destructionProgress;
    [SerializeField] Slider destructionSpeedSlider;

    [SerializeField] float destroyDuration;

    [SerializeField]
    int levelCount;

    int LoadedLevelBuildIndex;

    public GameLevel CurrentGameLevel;

    Random.State mainRandomState;
    [SerializeField]
    bool reseedOnLoad;

    [SerializeField] ShapeFactory[] shapeFactories;

    public static Game_2 Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if(shapeFactories[0].FactoryId != 0)  //game has reloaded
        {
            for (int i = 0; i < shapeFactories.Length; i++)
            {
                shapeFactories[i].FactoryId = i;
            }
        }

    }

    private void Start()
    {
        mainRandomState = Random.state;

        shapes = new List<Shape>();
        killList = new List<ShapeInstance>();
        markAsDyingList = new List<ShapeInstance>();
        Debug.Log(Application.persistentDataPath);
        Application.targetFrameRate = 60;

        //if some level has loaded, no others should load
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene sceneTmp = SceneManager.GetSceneAt(i);
            if (sceneTmp.name.Contains("Level "))
            {
                LoadedLevelBuildIndex = sceneTmp.buildIndex;
                SceneManager.SetActiveScene(sceneTmp);
                return;   //no need to load again
            }
        }
        
        StartCoroutine(LoadLevel(1));

    }

    private void Update()
    {
        if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
            StartCoroutine(LoadLevel(LoadedLevelBuildIndex));
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
        else if (Input.GetKeyDown(testKey))
        {
            //StartCoroutine(LoadLevel());
            //SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);
            //Scene sceneTmp = SceneManager.GetSceneByName("Level 1");
            //Debug.Log("GetSceneByName : " + sceneTmp.name);
            //LoadLevel();
        }
        else
        {
            //load select level
            for(int i=1; i<=levelCount; i++)
            {
                if(Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    Debug.Log("Select Level : " + i);
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }

        }


    }

    private void FixedUpdate()
    {
        inGameUpdateLoop = true;
        for(int i=0; i<shapes.Count; i++)
        {
            shapes[i].GameUpdate();
        }
        inGameUpdateLoop = false;

        if (Input.GetKeyDown(createKey))
        {
            creationProgress = 0;
            CreateShape();
        }
        if (Input.GetKey(createKey))
        {
            //automatic continuous creation
            creationProgress += Time.deltaTime * CreationSpeed;
            while (creationProgress >= 1)
            {
                creationProgress -= 1;
                CreateShape();
            }
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

        int limit = GameLevel.CurrentGameLevel.PopulationLimit;
        if(limit > 0)
        {
            while(shapes.Count - dyingShapeCount > limit)
            {
                DestroyShape();
            }
        }

        if (killList.Count > 0)
        {
            for (int i = 0; i < killList.Count; i++)
            {
                if (killList[i].isValid)
                {
                    KillImmediately(killList[i].Shape);
                }
            }
            killList.Clear();
        }

        if (markAsDyingList.Count > 0)
        {
            for (int i = 0; i < markAsDyingList.Count; i++)
            {
                if (markAsDyingList[i].isValid)
                {
                    MarkAsDyingImmediately(markAsDyingList[i].Shape);
                }
            }
            markAsDyingList.Clear();
        }
    }

    void BeginNewGame()
    {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        mainRandomState = Random.state;  //Random.state changes everytime
        Random.InitState(seed);
        CreationSpeed = 0;
        creationSpeedSlider.value = 0;
        DestructionSpeed = 0;
        destructionSpeedSlider.value = 0;

        for (int i = 0; i < shapes.Count; i++)
        {
            shapes[i].Recycle();
        }
        shapes.Clear();
        dyingShapeCount = 0;
    }

    public void AddShape(Shape shape) {
        shape.SaveIndex = shapes.Count;  //current index in shapes list
        shapes.Add(shape);
    }

    void CreateShape()
    {
        GameLevel.CurrentGameLevel.SpawnShapes();
    }

    //destroy shape randomly
    void DestroyShape()
    {
        if (shapes.Count - dyingShapeCount > 0)
        {
            Shape shape = shapes[Random.Range(dyingShapeCount, shapes.Count)];
            if (destroyDuration <= 0f)
            {
                KillImmediately(shape);
            }
            else
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(
                    shape, destroyDuration
                );
            }
        }
    }

    public void KillImmediately(Shape shape)
    {
        int index = shape.SaveIndex;
        shape.Recycle();

        //adjust shape's position in shapes list

        //need to move it to position [dyingShapeCount-1]
        //the dying part length - 1
        if (index < dyingShapeCount && index < --dyingShapeCount)
        {
            shapes[dyingShapeCount].SaveIndex = index;
            shapes[index] = shapes[dyingShapeCount];
            index = dyingShapeCount;
        }

        int lastIndex = shapes.Count - 1;
        if(index < lastIndex)
        {
            shapes[lastIndex].SaveIndex = index;
            shapes[index] = shapes[lastIndex];
        }
        shapes.RemoveAt(lastIndex);
    }

    public void Kill(Shape shape)
    {
        if (inGameUpdateLoop)
        {
            killList.Add(shape);
        }
        else
        {
            KillImmediately(shape);
        }
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(saveVersion);
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(LoadedLevelBuildIndex);
        GameLevel.CurrentGameLevel.Save(writer);

        for (int i = 0; i < shapes.Count; i++)
        {
            writer.Write(shapes[i].OriginFactory.FactoryId);
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        //Debug.Log("Loading...");
        int version = reader.ReadInt();
        if(version > saveVersion)
        {
            Debug.LogError("Unsupported future save version " + version);
            return;
        }

        StartCoroutine(LoadGame(reader));
    }

    IEnumerator LoadGame(GameDataReader reader)
    {

        int count = reader.ReadInt();
        Random.State state = reader.ReadRandomState();
        if (!reseedOnLoad)
        {
            Random.state = state;
        }
        creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
        creationProgress = reader.ReadFloat();
        destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
        destructionProgress = reader.ReadFloat();

        //Debug.Log(count);
        yield return LoadLevel(reader.ReadInt());

        //Level info and objects must be loaded after LoadLevel, 
        // so, using a coroutine
        GameLevel.CurrentGameLevel.Load(reader);
        
        for (int i = 0; i < count; i++)
        {
            int factoryId = reader.ReadInt();
            int shapeId = reader.ReadInt();
            int materialId = reader.ReadInt();
            // Debug.Log(shapeId);

            Shape o = shapeFactories[factoryId].Get(shapeId, materialId);
            o.Load(reader);
        }

        //after all objects were loaded, resolve their relations in behavior
        for(int i=0; i<shapes.Count; i++)
        {
            shapes[i].ResolveShapeInstances();
        }

    }
    
    /*
    IEnumerator LoadLevel()
    {
        enabled = false;  //disable current game components while loading

        //SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);
        yield return SceneManager.LoadSceneAsync("Level 1", LoadSceneMode.Additive);

        Scene sceneTmp = SceneManager.GetSceneByName("Level 1");
        //Debug.Log("GetSceneByName : " + sceneTmp.name);

        //while (!sceneTmp.isLoaded) { yield return null; }//wait for loading scene
        //yield return null;
        SceneManager.SetActiveScene(sceneTmp);

        enabled = true;
    }
    */

    IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;
        if(LoadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(
           LoadedLevelBuildIndex);
        }

        yield return SceneManager.LoadSceneAsync(
            levelBuildIndex, LoadSceneMode.Additive);

        SceneManager.SetActiveScene(
            SceneManager.GetSceneByBuildIndex(levelBuildIndex)
        );
        LoadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }

    public Shape GetShape(int index)
    {
        return shapes[index];
    }

    //split shapes list to two parts
    //[0, dyingShapeCount-1] contains dying shapes
    //[dyingShapeCount ,end] contains living shapes
    //when shape is marked as dying, move it to front
    void MarkAsDyingImmediately(Shape shape)
    {
        int index = shape.SaveIndex;
        if (index < dyingShapeCount)
        {
            return;
        }
        else
        {
            shapes[dyingShapeCount].SaveIndex = index;
            shapes[index] = shapes[dyingShapeCount];
            shape.SaveIndex = dyingShapeCount;
            shapes[dyingShapeCount++] = shape;
        }
    }

    public void MarkAsDying(Shape shape)
    {
        if (inGameUpdateLoop)
        {
            markAsDyingList.Add(shape);
        }
        else
        {
            MarkAsDyingImmediately(shape);
        }
    }

    public bool IsMarkedAsDying(Shape shape)
    {
        return shape.SaveIndex < dyingShapeCount;
    }
}
