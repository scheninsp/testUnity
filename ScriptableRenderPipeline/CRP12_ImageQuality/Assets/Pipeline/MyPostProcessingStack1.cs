using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Post-Processing Stack")]
public class MyPostProcessingStack1 : ScriptableObject
{
    static Mesh fullScreenTriangle;

    static Material material;

    static int mainTexId = Shader.PropertyToID("_MainTex");
    static int tempTexId = Shader.PropertyToID("_MyPostProcessingStackTempTex");
    static int depthTexId = Shader.PropertyToID("_DepthTex");

    enum Pass { Copy, Blur, DepthStripes };

    [SerializeField, Range(0, 10)]
    int blurStrength;

    [SerializeField]
    bool depthStripes;




    static void InitializeStatic()
    {
        if (fullScreenTriangle)
        {   //already initialized
            return;
        }

        fullScreenTriangle = new Mesh
        {
            name = "My Post-Processing Stack Full-Screen Triangle",
            vertices = new Vector3[] {
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f,  3f, 0f),
                new Vector3( 3f, -1f, 0f)
            },
            triangles = new int[] { 0, 1, 2 },
        };
        fullScreenTriangle.UploadMeshData(true);

        material =
            new Material(Shader.Find("My Pipeline/PostEffectStack1"))
            {
                name = "My Post-Processing Stack material",
                hideFlags = HideFlags.HideAndDontSave
            };
    }

    public void RenderAfterOpaque(
        CommandBuffer cb, int cameraColorId, int cameraDepthId,
        int width, int height
    )
    {
        InitializeStatic();
        if (depthStripes) { 
            DepthStripes(cb, cameraColorId, cameraDepthId, width, height);
        }
    }

    public void RenderAfterTransparent(CommandBuffer cb, int cameraColorId, 
        int cameraDepthId, int width, int height)
    {
        //blur
        if (blurStrength > 0)
        {
            Blur(cb, cameraColorId, width, height);
        }
        else
        {
            Blit(cb, cameraColorId, BuiltinRenderTextureType.CameraTarget);
        }
    }

    void Blur(CommandBuffer cb, int cameraColorId, int width, int height)
    {
        cb.BeginSample("Blur");
        if (blurStrength == 1)  //blur once
        {
            Blit(cb, cameraColorId, BuiltinRenderTextureType.CameraTarget, Pass.Blur);
            cb.EndSample("Blur");
            return;
        }

        cb.GetTemporaryRT(tempTexId, width, height, 0, FilterMode.Bilinear);
        int passesLeft;
        //blur twice each loop until no pass left
        for (passesLeft = blurStrength; passesLeft > 2; passesLeft -= 2)
        {
            Blit(cb, cameraColorId, tempTexId, Pass.Blur);
            Blit(cb, tempTexId, cameraColorId, Pass.Blur);
        }
        if (passesLeft > 1) {
			Blit(cb, cameraColorId, tempTexId, Pass.Blur);
            Blit(cb, tempTexId, BuiltinRenderTextureType.CameraTarget, Pass.Blur);
        }
		else {
			Blit(cb, cameraColorId, BuiltinRenderTextureType.CameraTarget, Pass.Blur);
		}
		cb.ReleaseTemporaryRT(tempTexId);
        cb.EndSample("Blur");
    }

    void Blit(CommandBuffer cb, RenderTargetIdentifier sourceId, 
        RenderTargetIdentifier destinationId, 
        Pass pass = Pass.Copy)
    {

        cb.SetGlobalTexture(mainTexId, sourceId);

        cb.SetRenderTarget(
        destinationId,
        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

        cb.DrawMesh(fullScreenTriangle, Matrix4x4.identity, material,
            0, (int)pass);  //submesh index = 0
    }

    void DepthStripes(
    CommandBuffer cb, int cameraColorId, int cameraDepthId,
    int width, int height
)
    {
        cb.BeginSample("Depth Stripes");
        cb.GetTemporaryRT(tempTexId, width, height);

        //camera depth to near-plane depth
        cb.SetGlobalTexture(depthTexId, cameraDepthId);

        Blit(cb, cameraColorId, tempTexId, Pass.DepthStripes);

        //blend with color
        Blit(cb, tempTexId, cameraColorId);
        cb.ReleaseTemporaryRT(tempTexId);
        cb.EndSample("Depth Stripes");
    }
}