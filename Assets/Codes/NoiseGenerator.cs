using UnityEditor.UI;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public int textureSize = 512; // ����ĳߴ�
    public float fequency = 0.05f; // ������Ƶ��
    public int octaves = 4; // �����ı�Ƶ��
    public float lacunarity = 2.0f; // �ֲڶ�
    public float gain = 0.5f; // ����
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;// ��������
    public int seed = 114514; // �������
    public Texture2D noiseTexture; // ���ɵ�����
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start����MonoBehaviour�������һ��ִ��Update֮ǰ����
    void Start()
    {
        ������������();
    }

    [ContextMenu("������������")]
    public void ������������()
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
                // ��������ֵ
                float noiseValue = fnl.GetNoise(x, y);
                // ������ֵӳ�䵽0-1��Χ
                Color color = new Color(noiseValue, noiseValue, noiseValue);
                noiseTexture.SetPixel(x, y, color);
            }
        }
        noiseTexture.Apply(); // Ӧ�ø���
        Debug.Log("���������������");
    }

    // Update is called once per frame
    // Updateÿ֡����һ��
    void Update()
    {
        
    }

    // FixedUpdate is called every fixed framerate frame
    // FixedUpdateÿ���̶�֡�ʵ�֡����һ��
    private void FixedUpdate()
    {
        
    }
}
