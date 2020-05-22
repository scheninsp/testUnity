using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    [SerializeField]
    MeshRenderer[] meshRenderers;

    public Vector3 AngularVelocity { get; set; }
    public Vector3 Velocity { get; set; }

    Color[] colors;  //for saving color
    public int ColorCount { get{return colors.Length; } }

    //mark creation factory for recycle of this shape
    public ShapeFactory OriginFactory
    {
        get
        {
            return originFactory;
        }
        set
        {
            if (originFactory == null)
            {
                originFactory = value;
            }
            else
            {
                Debug.LogError("Not allowed to change originFactory");
            }
        }
    }

    ShapeFactory originFactory;

    private void Awake()
    {
        colors = new Color[meshRenderers.Length];
    }

    //make a property
    public int ShapeId {
        get {
            return shapeId;
        }
        set {
            if (shapeId == int.MinValue && value != int.MinValue) {
                shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId");
            }
        }
    }
    int shapeId = int.MinValue;


    public int MaterialId
    {
        get;
        private set; //forbidden
    }
    int materialId = int.MinValue;

    public void SetMaterial(Material material, int materialId)
    {
        for(int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = material;
        }
        MaterialId = materialId;
    }


    //use PropertyID to generate only one Id throughout a session 
    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock shaderPropertyBlock;

    public void SetColor(Color color, int index)
    {
        //meshRenderer.material.color = color;  //this create a new material everytime

        //use propertyBlock to avoid making new materials
        if(shaderPropertyBlock == null)
        {
            shaderPropertyBlock = new MaterialPropertyBlock();
        }
        shaderPropertyBlock.SetColor(colorPropertyId, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(shaderPropertyBlock);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(colors.Length);
        for(int i=0; i<colors.Length; i++)
        {
            writer.Write(colors[i]);
        }
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        LoadColors(reader);
        AngularVelocity = reader.ReadVector3();
        Velocity = reader.ReadVector3();
    }

    void LoadColors(GameDataReader reader)
    {
        int count = reader.ReadInt();
        int max = count <= colors.Length ? count : colors.Length;
        for (int i = 0; i < max; i++)
        {
            SetColor(reader.ReadColor(), i);
        }
        if(count > colors.Length)
        {
            for (int i = max; i < count; i++)  //read extra colors remained in savefile
            {
                reader.ReadColor();
            }
        }
        else if(count < colors.Length) {
            for(int i=count; i<colors.Length; i++)
            {
                SetColor(Color.white, i);
            }
        }
    }

    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity * Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }

    public void Recycle()
    {
        OriginFactory.Reclaim(this);
    }

}
