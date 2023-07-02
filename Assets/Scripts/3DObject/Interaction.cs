using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// インタラクションを持つオブジェクトに入れて，状況に応じたインタラクションを行う
/// </summary>
public class Interaction : MonoBehaviour
{
    public bool isChanged = false;
    [Header("Slide")]
    public GameObject slide;//スライドチェンジャーを呼び出すためのスライド
    public void bootInteraction(GameObject controller)
    {
        //ポインティングしているオブジェクトのインタラクションを操作
        if (tag == "InteractableObject")
        {

            //ラジカセは、ボタンが押されたら音楽再生するか否かを切り替える
            if (gameObject.name == "Razikase")
            {


                if (SteamVR_Actions.default_ButtonA.GetState(SteamVR_Input_Sources.RightHand) || SteamVR_Actions.default_ButtonX.GetState(SteamVR_Input_Sources.LeftHand))
                {

                    if (!isChanged)
                    {
                        if (GetComponent<AudioSource>().mute)
                        {
                            GetComponent<AudioSource>().mute = false;
                            GetComponent<AudioClipPlayer>().randomPlayer();//適当に音楽を再生する関数です
                        }
                        else GetComponent<AudioSource>().mute = true;
                        isChanged = true;
                    }
                }
                else isChanged = false;
            }
        }



        //木琴の場合
        else if (tag == "Mokkin")
        {
            if (SteamVR_Actions.default_ButtonA.GetState(SteamVR_Input_Sources.RightHand) || SteamVR_Actions.default_ButtonX.GetState(SteamVR_Input_Sources.LeftHand))
            {


                if (!isChanged)
                {
                    GetComponent<AudioClipPlayer>().randomPlayer();//適当にSEを再生する関数です
                    isChanged = true;
                }
            }
            else isChanged = false;
        }

        //ボタンの場合
        else if (tag == "Button")
        {
            if (controller.GetComponent<laserInfo>().LaserGrabbed)
            {//ボタンを押したら
                if (!isChanged)
                {
                    controller.GetComponent<laserInfo>().pointedObject.GetComponent<Renderer>().materials = controller.GetComponent<laserInfo>().pointedObject.GetComponent<PartsTexture>().pointedMaterials;//マテリアル変換

                    isChanged = true;

                    if (controller.GetComponent<laserInfo>().pointedObject.GetComponent<ButtonID>().button == ButtonID.buttonKind.Front) slide.GetComponent<SlideChanger>().slideChange("Front");
                    else if (controller.GetComponent<laserInfo>().pointedObject.GetComponent<ButtonID>().button == ButtonID.buttonKind.Back) slide.GetComponent<SlideChanger>().slideChange("Back");
                }
            }
            else
            {
                isChanged = false;
                GetComponent<Renderer>().materials = GetComponent<PartsTexture>().normalMaterials;//マテリアル戻す
            }

        }

    }
}
