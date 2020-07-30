using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TargetControllerFactory : ScriptableObject
{
    [SerializeField]
    ShapeController[] prefabs;

    [SerializeField]
    Material[] materials;

    [SerializeField]
    bool recycle;

    List<ShapeController>[] pools;  //each prefab has a pool 

    void CreatePools()
    {
        pools = new List<ShapeController>[prefabs.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<ShapeController>();
        }

    }

    public ShapeController Get(int shapeId = 0, int materialId = 0)
    {
        ShapeController inst;
        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }
            List<ShapeController> pool = pools[shapeId];
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

    public ShapeController GetRandom()
    {
        return Get(Random.Range(0, prefabs.Length),
            Random.Range(0, materials.Length));
    }

    public void Reclaim(ShapeController shapeToRecycle)
    {
        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }
            shapeToRecycle.gameObject.GetComponent<ShapeController>().Reclaim();
            pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
            shapeToRecycle.gameObject.SetActive(false);
        }
        else
        {
            Destroy(shapeToRecycle.gameObject);
        }
    }
}