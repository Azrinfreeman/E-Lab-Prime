using UnityEngine;

public class BeakerCondensation3D : MonoBehaviour
{
    [Tooltip("The Renderer of the condensation cylinder mesh.")]
    public Renderer condensationRenderer;
    [Tooltip("The Beaker transform to track physical dimensions.")]
    public Transform beakerTransform;

    [Header("Condensation State")]
    [Range(0f, 1f)]
    public float opacity = 0f;

    private MaterialPropertyBlock propBlock;
    private GameObject[] droplets;
    private float[] dropletProgress;
    private float[] dropletSpeeds;
    private float[] dropletAngles;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
        
        // Spawn 4 trickling droplets around the beaker inner wall
        droplets = new GameObject[4];
        dropletProgress = new float[4];
        dropletSpeeds = new float[4];
        dropletAngles = new float[4];

        // Create transparent droplet material
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        Material dropMat;
        if (urpUnlit != null)
        {
            dropMat = new Material(urpUnlit);
            dropMat.SetColor("_BaseColor", new Color(0.9f, 0.95f, 1.0f, 0.35f));
            dropMat.SetFloat("_Surface", 1f); // 1 = Transparent
            dropMat.SetInt("_Blend", 0); // 0 = Alpha blend
            dropMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            dropMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            dropMat.SetInt("_ZWrite", 0);
            dropMat.SetInt("_Cull", 2); // Double-sided
            dropMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            dropMat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            dropMat.renderQueue = 3003; // Render on top of beaker condensation overlay
        }
        else
        {
            dropMat = new Material(Shader.Find("Sprites/Default"));
            if (dropMat.shader == null) dropMat = new Material(Shader.Find("Standard"));
            dropMat.color = new Color(0.9f, 0.95f, 1.0f, 0.35f);
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            drop.name = "CondensationDrop_" + i;
            drop.transform.SetParent(transform, false);
            
            // Remove collider so it doesn't block physics or raycasts
            DestroyImmediate(drop.GetComponent<Collider>());
            drop.GetComponent<MeshRenderer>().sharedMaterial = dropMat;
            
            droplets[i] = drop;
            ResetDroplet(i);
            // Stagger initial progress so they don't trickle down all at once
            dropletProgress[i] = Random.value * 0.6f; 
        }
    }

    void ResetDroplet(int i)
    {
        dropletProgress[i] = 0f;
        dropletSpeeds[i] = 0.12f + Random.value * 0.18f;
        dropletAngles[i] = Random.value * Mathf.PI * 2f;
        if (droplets[i] != null) droplets[i].SetActive(false);
    }

    void Update()
    {
        if (condensationRenderer != null)
        {
            condensationRenderer.GetPropertyBlock(propBlock);
            Color mistColor = new Color(0.95f, 0.96f, 0.98f, opacity);
            propBlock.SetColor("_BaseColor", mistColor);
            propBlock.SetColor("_Color", mistColor); // Fallback
            condensationRenderer.SetPropertyBlock(propBlock);
        }

        // Only animate droplets once condensation is visible (opacity > 0.08f)
        for (int i = 0; i < 4; i++)
        {
            if (droplets[i] == null) continue;

            if (opacity > 0.08f)
            {
                droplets[i].SetActive(true);
                dropletProgress[i] += dropletSpeeds[i] * Time.deltaTime;

                // Simple sine curve to make the droplet swell and shrink
                float scaleFactor = Mathf.Sin(dropletProgress[i] * Mathf.PI);

                // Local Y position slides from top (0.8f) to bottom (-0.8f)
                float localY = Mathf.Lerp(0.8f, -0.8f, dropletProgress[i]);
                float angle = dropletAngles[i];
                
                // Local radius slightly inside beaker inner wall (0.485f)
                float r = 0.485f;
                float lx = r * Mathf.Cos(angle);
                float lz = r * Mathf.Sin(angle);

                droplets[i].transform.localPosition = new Vector3(lx, localY, lz);
                droplets[i].transform.localScale = new Vector3(0.02f, 0.08f * scaleFactor, 0.01f);

                if (dropletProgress[i] >= 1.0f)
                {
                    ResetDroplet(i);
                }
            }
            else
            {
                droplets[i].SetActive(false);
            }
        }
    }
}
