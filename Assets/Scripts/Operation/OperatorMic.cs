using UnityEngine;
using System.Collections;

public class OperatorMic : MonoBehaviour
{

    public string targetMic;
    private string micName;

    void Start()
    {
        micName = "PnP";

        AudioSource aud = GetComponent<AudioSource>();
        targetMic = null;

        foreach (var device in Microphone.devices)
        {
            if (device.Contains(micName))//相手のマイク音源は、ここで指定する
                targetMic = device;
        }

        // マイク名、ループするかどうか、AudioClipの秒数、サンプリングレート を指定する
        aud.clip = Microphone.Start(targetMic, true, 3000, 48000);
        aud.Play();
    }
}
