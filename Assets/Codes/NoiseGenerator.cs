using UnityEditor.UI;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public int textureSize = 512; // 纹理的尺寸
    public float fequency = 0.05f; // 噪声的频率
    public int octaves = 4; // 噪声的倍频数
    public float lacunarity = 2.0f; // 粗糙度
    public float gain = 0.5f; // 增益
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;// 噪声类型
    public int seed = 114514; // 随机种子
    public Texture2D noiseTexture; // 生成的纹理
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start会在MonoBehaviour创建后第一次执行Update之前调用
    void Start()
    {
        生成噪声纹理();
    }

    [ContextMenu("生成噪声纹理")]
    public void 生成噪声纹理()
    {
        noiseTexture = new Texture2D(textureSize, textureSize);
        FastNoiseLite fnl = new FastNoiseLite(seed);
        fnl.SetNoiseType(noiseType);
        fnl.SetFrequency(fequency);
        fnl.SetFractalOctaves(octaves);
        fnl.SetFractalLacunarity(lacunarity);
        fnl.SetFractalGain(gain);
        fnl.SetFractalType(FastNoiseLite.FractalType.FBm);
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                // 计算噪声值
                float noiseValue = fnl.GetNoise(x, y);
                // 将噪声值映射到0-1范围
                Color color = new Color(noiseValue, noiseValue, noiseValue);
                noiseTexture.SetPixel(x, y, color);
            }
        }
        noiseTexture.Apply(); // 应用更改
        Debug.Log("噪声纹理生成完成");
    }

    // Update is called once per frame
    // Update每帧调用一次
    void Update()
    {
        
    }

    // FixedUpdate is called every fixed framerate frame
    // FixedUpdate每个固定帧率的帧调用一次
    private void FixedUpdate()
    {
        
    }
}
