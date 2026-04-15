using UnityEngine;

public class WaterWaves : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveHeight = 0.2f;
    public float waveFrequency = 0.5f;
    public float waveSpeed = 2.0f;
    
    [Header("Direction")]
    public Vector3 waveDirection = new Vector3(1, 0, 1);
    
    [Header("Shader Property")]
    public string waveProperty = "_WaveStrength";
    public string speedProperty = "_WaveSpeed";
    
    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private Mesh clonedMesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    private Material waterMaterial;
    
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        
        if (meshFilter != null)
        {
            // Clone mesh to avoid modifying original asset
            originalMesh = meshFilter.sharedMesh;
            clonedMesh = Instantiate(originalMesh);
            meshFilter.mesh = clonedMesh;
            
            originalVertices = clonedMesh.vertices;
            modifiedVertices = new Vector3[originalVertices.Length];
        }
        
        // Get material
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            waterMaterial = renderer.material;
        }
        
        // Normalize direction
        waveDirection.Normalize();
    }
    
    void Update()
    {
        // Method 1: Animate vertices directly
        AnimateVertices();
        
        // Method 2: Modify shader properties
        AnimateMaterial();
    }
    
    void AnimateVertices()
    {
        if (clonedMesh == null) return;
        
        float time = Time.time * waveSpeed;
        
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            
            // Calculate wave based on position and time
            float wave = Mathf.Sin(vertex.x * waveFrequency + time) * waveHeight;
            wave += Mathf.Sin(vertex.z * waveFrequency * 0.5f + time * 0.8f) * waveHeight * 0.5f;
            
            // Apply to Y axis
            modifiedVertices[i] = new Vector3(vertex.x, vertex.y + wave, vertex.z);
        }
        
        clonedMesh.vertices = modifiedVertices;
        clonedMesh.RecalculateNormals();
    }
    
    void AnimateMaterial()
    {
        if (waterMaterial == null) return;
        
        // Animate shader properties if they exist
        if (waterMaterial.HasProperty(waveProperty))
        {
            float waveStrength = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
            waterMaterial.SetFloat(waveProperty, waveStrength * waveHeight);
        }
        
        if (waterMaterial.HasProperty(speedProperty))
        {
            waterMaterial.SetFloat(speedProperty, waveSpeed);
        }
        
        // Animate offset/scroll
        Vector2 offset = waterMaterial.mainTextureOffset;
        offset.x += Time.deltaTime * waveSpeed * 0.1f;
        offset.y += Time.deltaTime * waveSpeed * 0.05f;
        waterMaterial.mainTextureOffset = offset;
    }
    
    void OnDestroy()
    {
        // Clean up cloned mesh
        if (clonedMesh != null)
        {
            Destroy(clonedMesh);
        }
    }
}
