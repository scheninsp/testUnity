using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class ShapeFactory :ScriptableObject
{
    [SerializeField]
    Shape[] prefabs;

    [SerializeField]
    Material[] materials;

    [SerializeField]
    bool recycle;

    List<Shape>[] pools;  //each prefab has a pool and shapeId

    Scene poolScene;

    void CreatePools()
    {

        pools = new List<Shape>[prefabs.Length];
        for(int i=0; i<pools.Length; i++)
        {
            pools[i] = new List<Shape>();
        }

        //play mode recompilation will mess up unserialized files,
        //such as pools and poolScene, use this to avoid 
        if (Application.isEditor)
        {
            poolScene = SceneManager.GetSceneByName(name);
            if (poolScene.isLoaded)
            {
                GameObject[] rootObjects = poolScene.GetRootGameObjects();
                for(int i=0; i<rootObjects.Length; i++)
                {
                    Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                    if (!pooledShape.gameObject.activeSelf)
                    {
                        pools[pooledShape.ShapeId].Add(pooledShape);
                    }
                }
                return;
            }
        }

        poolScene = SceneManager.CreateScene(name);
    }

    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        Shape inst;
        if (recycle)
        {
            if(pools == null)
            {
                CreatePools();
            }
            List<Shape> pool = pools[shapeId];
            int lastIndex = pool.Count - 1;
            if (lastIndex > 0)
            {
                inst = pool[lastIndex];
                inst.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                inst = Instantiate(prefabs[shapeId]);
                inst.ShapeId = shapeId;
                SceneManager.MoveGameObjectToScene(inst.gameObject, poolScene);
            }
        }
        else
        {
            inst = Instantiate(prefabs[shapeId]);
            inst.ShapeId = shapeId;
        }
        inst.SetMaterial(materials[materialId], materialId);
        return inst;
    }

    public Shape GetRandom()
    {
        return Get(Random.Range(0, prefabs.Length),
            Random.Range(0, materials.Length));
    }

    public void Reclaim(Shape shapeToRecycle)
    {
        if (recycle)
        {
            if(pools == null)
            {
                CreatePools();
            }
            pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
            shapeToRecycle.gameObject.SetActive(false);
        }
        else
        {
            Destroy(shapeToRecycle.gameObject);
        }
    }
}
