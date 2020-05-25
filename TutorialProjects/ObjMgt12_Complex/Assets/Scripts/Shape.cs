using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    [SerializeField]
    MeshRenderer[] meshRenderers;

    List<ShapeBehavior> behaviorList = new List<ShapeBehavior>();

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

    public float Age { get; set; }

    public int InstanceId { get; set; }
    public int SaveIndex { get; set; }

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

        writer.Write(Age);
        writer.Write(behaviorList.Count);
        for(int i=0; i<behaviorList.Count; i++)
        {
            writer.Write((int)behaviorList[i].BehaviorType);
            behaviorList[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        LoadColors(reader);

        Age = reader.ReadFloat();
        int behaviorCount = reader.ReadInt();
        for(int i=0; i<behaviorCount; i++)
        {
            //add behavior and load behavior parameters
            ShapeBehavior behavior = ((ShapeBehaviorType)reader.ReadInt()).GetInstance();
            behaviorList.Add(behavior);
            behavior.Load(reader);
        }
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

    public T AddBehavior<T>() where T : ShapeBehavior, new()
    {
        T behavior = ShapeBehaviorPool<T>.Get();
        behaviorList.Add(behavior);
        return behavior;
    }

    public void GameUpdate()
    {
        Age += Time.deltaTime;
      for(int i=0; i<behaviorList.Count; i++)
        {
            if (!behaviorList[i].GameUpdate(this)) {
                //some behavior's attached shape has been recycled
                behaviorList[i].Recycle();
                behaviorList.RemoveAt(i--);  //after removal, i should adjust
            }
        }
    }

    public void Recycle()
    {
        Age = 0f;
        InstanceId += 1;
        for(int i=0; i<behaviorList.Count; i++)
        {
            behaviorList[i].Recycle();
            //Destroy(behaviorList[i]);
        }
        behaviorList.Clear();
        OriginFactory.Reclaim(this);
    }

    public void ResolveShapeInstances()
    {
        for(int i=0; i<behaviorList.Count; i++)
        {
            behaviorList[i].ResolveShapeInstances();
        }
    }

    //because Game keeps theses states of shape object
    public void Die()
    {
        Game_2.Instance.Kill(this);
    }

    public void MarkAsDying()
    {
        Game_2.Instance.MarkAsDying(this);
    }

    public bool IsMarkedAsDying
    {
        get
        {
            return Game_2.Instance.IsMarkedAsDying(this);
        }
    }
}
