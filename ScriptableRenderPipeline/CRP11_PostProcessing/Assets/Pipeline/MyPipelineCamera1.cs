using UnityEngine;

[ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class MyPipelineCamera1 : MonoBehaviour
{

    [SerializeField]
    MyPostProcessingStack1 postProcessingStack = null;

    public MyPostProcessingStack1 PostProcessingStack
    {
        get
        {
            return postProcessingStack;
        }
    }
}