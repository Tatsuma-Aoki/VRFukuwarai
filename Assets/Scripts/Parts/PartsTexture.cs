using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>///
///レーザーを照射したときにテクスチャを変えるので，変える用のテクスチャを保管する場所
///オブジェクト側でレーザを照射されたかは判定できないのでこれで対応
///</summary>///

public class PartsTexture : MonoBehaviour
{
    public Material[] normalMaterials;//そのパーツ/ボタンのもとのマテリアル群
    public Material[] pointedMaterials;//パーツをポイントしたとき/ボタンを押したときに張り替えるマテリアル群
}
