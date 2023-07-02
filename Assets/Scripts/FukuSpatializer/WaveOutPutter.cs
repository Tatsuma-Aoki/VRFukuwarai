using UnityEngine;
using System;
using System.Linq;
using System.Numerics;


public class WaveOutPutter : MonoBehaviour
{

    //音響テスト用のスクリプトなので後で消してください
    private float[] waveData_ = new float[1024];
    private AudioSource audiosource;
    public GameObject LeftMusic, RightMusic;
    public float scale = 4.0f;


    private float ratioL = 0f, ratioR = 0f;


    void Start()
    {

        audiosource = gameObject.GetComponent<AudioSource>();


    }

    void Update()
    {
        audiosource.GetOutputData(waveData_, 1);
        var volume = waveData_.Select(x => x * x).Sum() / waveData_.Length;
        LeftMusic.transform.localScale = UnityEngine.Vector3.one * (float)Math.Exp(ratioL) * (volume + 0.5f) * scale;
        RightMusic.transform.localScale = UnityEngine.Vector3.one * (float)Math.Exp(ratioR) * (volume + 0.5f) * scale;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        int n = data.Length / channels;

        for (int i = 0; i < data.Length; i += channels)
        {
            if (!Double.IsNaN(data[i]))
                ratioL = data[i];

            if (!Double.IsNaN(data[i + 1]))
                ratioR = data[i + 1];
        }

        // ratioL = Mathf.Min(Mathf.Pow(lenL / lenStd, -2), 1);
        // ratioR = Mathf.Min(Mathf.Pow(lenR / lenStd, -2), 1);
    }

}