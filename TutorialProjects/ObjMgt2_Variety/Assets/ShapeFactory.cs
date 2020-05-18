using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ShapeFactory :ScriptableObject
{
    [SerializeField]
    Shape[] prefabs;

    [SerializeField]
    Material[] materials;

    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        Shape inst = Instantiate(prefabs[shapeId]);
        inst.ShapeId = shapeId;
        inst.SetMaterial(materials[materialId], materialId);
        return inst;
    }

    public Shape GetRandom()
    {
        return Get(Random.Range(0, prefabs.Length),
            Random.Range(0, materials.Length));
    }
}
