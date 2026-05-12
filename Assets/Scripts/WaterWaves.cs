using UnityEngine;

public class WaterWaves : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveHeight = 0.2f;
    public float waveFrequency = 0.5f;
    public float waveSpeed = 2.0f;
    
    [Header("Shader Property")]
    public string waveProperty = "_WaveStrength";
    public string speedProperty = "_WaveSpeed";
    
    private Material waterMaterial;
    
    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            waterMaterial = renderer.material;
        }
    }
    
    void Update()
    {
        AnimateMaterial();
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
    

}
