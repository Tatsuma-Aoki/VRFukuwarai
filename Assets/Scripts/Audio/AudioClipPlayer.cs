using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipPlayer : MonoBehaviour
{
    public AudioClip[] audioClipsList;
    private AudioSource audioSource;
    private System.Random r;

    public void randomPlayer()
    {
        r = new System.Random();//乱数を生成する
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClipsList[r.Next(0, audioClipsList.Length)];//ランダムにClipを切り替える
        audioSource.Play();//再生

    }
}
