using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonID : MonoBehaviour
{
    //ボタンの種類を指定する
    public enum buttonKind
    {
        Front,
        Back
    }

    public buttonKind button;
}
