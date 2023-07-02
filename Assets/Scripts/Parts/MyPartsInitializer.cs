using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// 自分のパーツを初期化するのに使う
/// </summary>

public class MyPartsInitializer : MonoBehaviour
{
    [Header("Initializer")]
    //初期設定する座標系
    [SerializeField] private Vector3 firstPosition;//初期の親ボーンとの相対座標
    [SerializeField] private Quaternion firstRotation;//初期の回転
    [SerializeField] private Transform firstParentBone;//初期の親ボーン　Startで取得します

    [Header("SteamVR")]
    //SteamVRの入力系　
    public SteamVR_Action_Boolean pushedA = SteamVR_Actions.default_ButtonA;//Aボタンが押されたか確認
    public SteamVR_Action_Boolean pushedX = SteamVR_Actions.default_ButtonX;//Xボタンが押されたか確認
    private Boolean isPushedA = false, isPushedX = false;
    public Boolean initialized;
    //今回はAとX同時押しで起動するため、GetState(SteamVR_Input_Sources.LeftHand or RightHand)でのBool値のandで判断する

    void Start()
    {
        //個々のパーツの位置情報と回転情報を記録
        firstParentBone = transform.parent;
        firstPosition = transform.position;
        firstRotation = transform.rotation;
    }

    private void Update()
    {
        isPushedA = pushedA.GetState(SteamVR_Input_Sources.RightHand);//右手のAが押されたか？
        isPushedX = pushedX.GetState(SteamVR_Input_Sources.LeftHand);//左手のXが押されたか？

        if (isPushedA && isPushedX && !initialized)//どっちも押されたら&まだ初期化してなかったら
        {
            facePartsInitialize();
            initialized = true;//初期化したよ！
        }
        else initialized = false;

    }
    private void facePartsInitialize()
    {
        //位置を初期化する
        transform.parent = firstParentBone;//初期の親ボーンを親にする
        transform.position = firstPosition;//相対座標に移動する
        transform.rotation = firstRotation;//回転させる
    }
}
