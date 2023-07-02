using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPopup : MonoBehaviour
{
    //ポップアップを表示したり消したりします
    [Header("PopUp")]
    public Transform popup;
    public GameObject popupOBJ;//上二つはポップアップオブジェクトを入れる
    public GameObject image;//表示する画像を入れるオブジェクトを入れる
    private Vector3 movePos;
    public Sprite[] sprites, metaSprites, viveSprites;//説明画像のスプライトを格納するスプライト群(Meta用、Vive用)
    public GameObject popupOpposite;//アタッチしているオブジェクトが右手用なら左手用ポップアップ、左手用なら右手用ポップアップを入れる
    [Header("VR Camera")]
    public Transform vrCamera;//VRカメラを入れる
    public bool isAppear = false;//ポップアップが表示されているかを判断します
    [Header("HMD")]
    public HMDChanger hmdChanger;//HMDチェンジャを使ってポップアップの種類を変化させる

    public void Start()
    {
        //HMDの種類で表示するスプライトを変える
        if (hmdChanger.hmd == HMDChanger.HMD.MetaQ2) sprites = metaSprites;
        else if (hmdChanger.hmd == HMDChanger.HMD.Vive) sprites = viveSprites;
    }

    public void popupAppear(Transform target, GameObject controller)
    {
        if (target == null)
        {
            return;
        }
        var oppositPointingObject = controller.GetComponent<laserInfo>().OtherHand.GetComponent<laserInfo>().pointedObject;
        //反対側のコントローラのターゲットと、自分側のコントローラのターゲットが重複していなければ
        if (oppositPointingObject != null)
        {
            if (oppositPointingObject.GetComponent<ObjectInfo>() != null && target.GetComponent<ObjectInfo>() != null)
            {
                if (oppositPointingObject.GetComponent<ObjectInfo>().id != target.GetComponent<ObjectInfo>().id)
                {
                    popupOBJ.SetActive(true);
                    if (popup != null)
                    {
                        //ここで、targetの種類に応じて表示する内容を変える
                        imageChanger(target);
                        popup.LookAt(vrCamera.position);//VRカメラの方を向くようにする
                        popup.Rotate(0.0f, 180.0f, 0.0f);
                    }

                    movePos = target.position;//照射オブジェクトのところに表示
                    transform.position = movePos;
                    isAppear = true;//表示フラグを立てる
                }
            }
        }
        else
        {

            popupOBJ.SetActive(true);
            if (popup)
            {
                //ここで、targetの種類に応じて表示する内容を変える
                imageChanger(target);
                popup.LookAt(vrCamera.position);//VRカメラの方を向くようにする
                popup.Rotate(0.0f, 180.0f, 0.0f);
            }

            movePos = target.position;//照射オブジェクトのところに表示
            transform.position = movePos;
            isAppear = true;//表示フラグを立てる

        }

    }

    public void popupDisappear()
    {//ポップアップを消します
        Debug.Log("DisAppear");
        isAppear = false;//表示フラグを折る
        popupOBJ.SetActive(false);
    }

    private void imageChanger(Transform target)
    {
        var img = image.GetComponent<Image>();
        var info = target.GetComponent<ObjectInfo>();
        if (img && info)
        {
            img.sprite = sprites[info.id];//そのオブジェクトのIDに応じた画像を表示する
        }
    }
}
