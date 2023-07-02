using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

public class laserInfo : MonoBehaviour
{

    public SteamVR_Action_Boolean grab = SteamVR_Actions.default_GrabLeft;//掴むボタンを指定
    public enum controllerLR
    {
        Left,
        Right
    }

    public controllerLR controller;
    public bool LaserGrabbed = false;
    public GameObject OtherHand;//反対の手
    public Transform pointedObject;//ポインティングしているオブジェクトの情報を取得
    public Transform grabbingObject;//掴んでいるオブジェクトの情報を取得
    public bool triggerPushed;//トリガーを押したかの確認

    void Update()
    {
        if ((grab.GetState(SteamVR_Input_Sources.LeftHand) && controller == controllerLR.Left) || (grab.GetState(SteamVR_Input_Sources.RightHand) && controller == controllerLR.Right))
        {
            LaserGrabbed = true;
        }
        else LaserGrabbed = false;

    }

}
