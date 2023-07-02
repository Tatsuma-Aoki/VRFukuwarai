using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スライド・マイク・ポップアップがHMD依存なので
/// それらを一括で変更させるためのやつ
/// </summary>
public class HMDChanger : MonoBehaviour
{
    public enum HMD
    {
        Vive,
        MetaQ2,
    }
    public HMD hmd;


}
