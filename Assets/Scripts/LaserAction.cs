using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;
using System;
public class LaserAction : MonoBehaviour
{

    //レーザーを用いたアクションについて行います

    ////////////////////////////////////////////////////
    //コントローラとボタン、レーザーポインター関連

    [Header("Controller")]
    public SteamVR_LaserPointer laserPointerL;//左手のレーザーポインターを入れる
    public SteamVR_LaserPointer laserPointerR;//右手のレーザーポインターを入れる
    public GameObject ControllerLeft;//左手のコントローラを入れる
    public GameObject ControllerRight;//右手のコントローラを入れる

    [Header("Action")]
    public SteamVR_Action_Vector2 MoveActionL, MoveActionR;//SteamVRでのスティックアクションを格納　\actions\default\in\Moveがデフォルトになる
    public float Speed = 2.0f;//オブジェクトが移動するスピードを変える
    public SteamVR_Action_Boolean pushedA = SteamVR_Actions.default_ButtonA;//Aボタンが押されたか確認
    public SteamVR_Action_Boolean pushedX = SteamVR_Actions.default_ButtonX;//Xボタンが押されたか確認
    private bool isChanged = false, isSounded = false, triggerPushed = false;//ラジカセオンオフ切り替えできたかを判断するのと、木琴が音をならせたかを判断する
    //↑の仕様だと、押している間ずっと切り替え続けるため、それを防ぐために一回だけ反応するようにする
    private VelocityEstimator VE;//Velocity Estimator
    public GameObject popupObjLeft;//レーザーを照射したときに出るポップアップ(左手用)　主にここのコンポーネントから関数を呼び出す
    public GameObject popupObjRight;//レーザーを照射したときに出るポップアップ(右手用)　主にここのコンポーネントから関数を呼び出す
    private Vector3 prevControllerLPos, prevControllerRPos, curControllerLPos, curControllerRPos;
    private float prevTime, curTime;
    private Vector3 curVelocityL, curVelocityR;
    public SteamVR_Action_Boolean grabL = SteamVR_Actions.default_GrabLeft;//左手の掴むボタン
    public SteamVR_Action_Boolean grabR = SteamVR_Actions.default_GrabRight;//右手の掴むボタン


    void Awake()
    {
        laserPointerL.PointerIn += (object sender, PointerEventArgs e) => PointerInside(sender, e, ControllerLeft);
        laserPointerL.PointerOut += (object sender, PointerEventArgs e) => PointerOutside(sender, e, ControllerLeft);

        laserPointerR.PointerIn += (object sender, PointerEventArgs e) => PointerInside(sender, e, ControllerRight);
        laserPointerR.PointerOut += (object sender, PointerEventArgs e) => PointerOutside(sender, e, ControllerRight);
    }

    private Transform GetChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }

    void Update()
    {
        prevControllerLPos = curControllerLPos;
        prevControllerRPos = curControllerRPos;
        curControllerLPos = ControllerLeft.transform.position;
        curControllerRPos = ControllerRight.transform.position;
        prevTime = curTime;
        curTime = Time.time;
        curVelocityL = (curControllerLPos - prevControllerLPos) / (curTime - prevTime);
        curVelocityR = (curControllerRPos - prevControllerRPos) / (curTime - prevTime);
        ////////////////////////////////////////////////////////////////////
        //スティック操作系のスクリプト
        //掴んでいるなら、スティックでポインタ方向にオブジェクトを操作できる

        //左手用
        var infoL = ControllerLeft.GetComponent<laserInfo>();
        if (infoL.LaserGrabbed)
        {
            // GameObject Target = GameObject.FindWithTag("InteractableObject");
            Transform TargetIOL = GetChildWithTag(ControllerLeft.transform, "InteractableObject");
            Transform TargetFPL = GetChildWithTag(ControllerLeft.transform, "FaceParts");


            //InteractableObject用
            if (TargetIOL)
            {
                float moveLen = -Speed * MoveActionL.axis.y * Time.deltaTime;
                if (moveLen < (ControllerLeft.transform.position - TargetIOL.position).magnitude)
                    TargetIOL.position += (ControllerLeft.transform.position - TargetIOL.position).normalized * moveLen;
            }//FaceParts用
            else if (TargetFPL)
            {
                float moveLen = -Speed * MoveActionL.axis.y * Time.deltaTime;
                if (moveLen < (ControllerLeft.transform.position - TargetFPL.position).magnitude)
                    TargetFPL.position += (ControllerLeft.transform.position - TargetFPL.position).normalized * moveLen;
            }
        }

        //右手用
        var infoR = ControllerRight.GetComponent<laserInfo>();
        if (infoR.LaserGrabbed)
        {
            // GameObject Target = GameObject.FindWithTag("InteractableObject");
            Transform TargetIOR = GetChildWithTag(ControllerRight.transform, "InteractableObject");
            Transform TargetFPR = GetChildWithTag(ControllerRight.transform, "FaceParts");


            //InteractableObject用
            if (TargetIOR)
            {
                float moveLen = -Speed * MoveActionR.axis.y * Time.deltaTime;
                if (moveLen < (ControllerRight.transform.position - TargetIOR.position).magnitude)
                    TargetIOR.position += (ControllerRight.transform.position - TargetIOR.position).normalized * moveLen;
            }//FaceParts用
            else if (TargetFPR)
            {
                float moveLen = -Speed * MoveActionR.axis.y * Time.deltaTime;
                if (moveLen < (ControllerRight.transform.position - TargetFPR.position).magnitude)
                    TargetFPR.position += (ControllerRight.transform.position - TargetFPR.position).normalized * moveLen;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        //ポインティングしているオブジェクトのインタラクション
        if (infoR.pointedObject) pointerInteraction(infoR.pointedObject, ControllerRight);
        if (infoL.pointedObject) pointerInteraction(infoL.pointedObject, ControllerLeft);

        ////////////////////////////////////////////////////////////////////////////
        //ポインティングしているときにグラブボタンを押したときの処理
        //ポイントしているオブジェクトがあるときにトリガーを押すと起動して別関数に移動
        if (infoL.pointedObject && SteamVR_Actions.default_GrabLeft.GetState(SteamVR_Input_Sources.LeftHand)) pointerGrab(infoL, ControllerLeft);
        else if (infoL.grabbingObject && !SteamVR_Actions.default_GrabLeft.GetState(SteamVR_Input_Sources.LeftHand)) pointerRelease(infoL, ControllerLeft);
        if (infoR.pointedObject && SteamVR_Actions.default_GrabRight.GetState(SteamVR_Input_Sources.RightHand)) pointerGrab(infoR, ControllerRight);
        else if (infoR.grabbingObject && !SteamVR_Actions.default_GrabRight.GetState(SteamVR_Input_Sources.RightHand)) pointerRelease(infoR, ControllerRight);
    }

    ////////////////////////////////////////////////////////////////
    //ポインタで掴んだときの動作
    public void pointerGrab(laserInfo info, GameObject controller)
    {//infoにはポイントしているモノとか掴んでいるかの情報が入ってる
        if (!info.LaserGrabbed)
        {//掴んでなければ

            if (info.pointedObject.tag == "FaceParts")
            {//FacePartsタグであれば
                var parts = info.pointedObject.GetComponent<Parts>();//ターゲットのパーツスクリプト

                info.pointedObject.transform.parent = controller.transform;//親をコントローラにする
                parts.canRaySee = true;//パーツの判定レイを表示する
                var sm = info.pointedObject.GetComponent<SkinnedMeshRenderer>();//スキンメッシュ
                if (sm != null && sm.rootBone != null)
                {
                    sm.rootBone.parent = controller.transform;
                }
                info.LaserGrabbed = true;//掴んでいるという情報で上書き
                info.grabbingObject = info.pointedObject;//掴んでいるオブジェクトの情報を登録

                //既に反対の手で掴んでいるものを掴もうとするとき、反対の手の掴み状況をリセットする
                var OtherHand = info.OtherHand;
                var OtherGrabbed = OtherHand.GetComponent<laserInfo>();
                OtherGrabbed.LaserGrabbed = false;
            }
            else if (info.pointedObject.tag == "InteractableObject")//interactableObjectは重力依存しているので別で処理したい
            {
                info.pointedObject.transform.parent = controller.transform;

                Rigidbody RBofpointedObject = info.pointedObject.GetComponent<Rigidbody>();
                RBofpointedObject.isKinematic = true;//キネマティック有効化
                info.LaserGrabbed = true;//掴んでいる情報で上書き
                info.grabbingObject = info.pointedObject;//掴んでいるオブジェクトの情報を登録

                //既に反対の手で掴んでいるものを掴もうとするとき、反対の手の掴み状況をリセットする
                var OtherHand = info.OtherHand;
                var OtherGrabbed = OtherHand.GetComponent<laserInfo>();
                OtherGrabbed.LaserGrabbed = false;
                //ポップアップ削除
                var popup = selectPopup(controller.GetComponent<laserInfo>().controller);
                if (popup) popup.GetComponent<DisplayPopup>().popupDisappear();//ポップアップを消す
            }
        }
    }

    ////////////////////////////////////////////////////////////////
    //トリガーはずしたとき
    public void pointerRelease(laserInfo info, GameObject controller)
    {
        if (info.grabbingObject)
        {//掴んでいれば
            if (info.grabbingObject.tag == "FaceParts")
            {//FacePartsタグであれば
                var parts = info.grabbingObject.GetComponent<Parts>();//ターゲットのパーツスクリプト
                if (parts)
                {
                    info.grabbingObject.transform.parent = null;

                    parts.SetPosition();//張り付く判定をする
                    parts.canRaySee = false;
                }

                var sm = info.grabbingObject.GetComponent<SkinnedMeshRenderer>();
                if (sm != null && sm.rootBone != null)
                {
                    sm.rootBone.parent = null;
                }
                info.grabbingObject = null;
                info.LaserGrabbed = false;

            }
            else if (info.grabbingObject.tag == "InteractableObject")//interactableObjectは重力依存しているので別で処理したい
            {
                Rigidbody RBofgrabbingObject = info.grabbingObject.GetComponent<Rigidbody>();
                info.grabbingObject.parent = null;
                RBofgrabbingObject.isKinematic = false;
                info.LaserGrabbed = false;
                info.grabbingObject = null;

                //Velocityとかで投げられるようにしたい

                RBofgrabbingObject.velocity = (controller == ControllerLeft) ? curVelocityL : curVelocityR;


                //ポップアップ機能
                var popup = selectPopup(controller.GetComponent<laserInfo>().controller);
                if (popup)
                    popup.GetComponent<DisplayPopup>().popupAppear(info.grabbingObject, controller);//ポップアップを表示

            }
        }

    }

    //////////////////////////////////////////////////////////////////////////////////
    //Interactable関連のボタン操作
    //レーザーポインターがtargetに触れたときの処理
    public void PointerInside(object sender, PointerEventArgs e, GameObject controller)
    {

        //1.レーザーポインターを当てたときの処理
        //レーザーポインターを当てたときの一連の動作については、そのレーザーポインターが物を持っていないときに全て行われるものとする
        //var Grabbed = controller.GetComponent<laserInfo>();

        //インタラクションのあるオブジェクトに対して発動
        if (e.target.tag == "FaceParts" || e.target.tag == "InteractableObject" || e.target.tag == "Mokkin" || e.target.tag == "Button")
        {

            controller.GetComponent<laserInfo>().pointedObject = e.target;//ポインティングしているオブジェクトの名前をコントローラ単位で記録する

            switch (e.target.tag)
            {

                //レーザーポインターを当てたときに、当てたパーツが光るようにしてみよう

                case "FaceParts":
                    //FacePartsのときはレンダラーが子オブジェクト"Face"にあるので、それを変更する
                    //Faceのタグは一律で"FaceMesh"にしておく
                    //ただFaceのテクスチャはVRM形式特有のものでEmission操作できなかったので、テクスチャを直接張り替えて対応します

                    Transform face = GetChildWithTag(e.target, "FaceMesh");//便利だなこれ

                    face.GetComponent<Renderer>().materials = face.GetComponent<PartsTexture>().pointedMaterials;//マテリアル列の一斉変換をする
                    break;
                case "InteractableObject"://InteractableObject
                                          //ポップアップ表示
                    var popup = selectPopup(controller.GetComponent<laserInfo>().controller);
                    if (popup)
                        popup.GetComponent<DisplayPopup>().popupAppear(e.target, controller);//ポップアップを表示


                    e.target.transform.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    e.target.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.blue);

                    break;
                case "Mokkin":
                    //木琴のときは、モデルの構造上少しだけ異なる
                    //ポップアップ表示
                    var popupMokkin = selectPopup(controller.GetComponent<laserInfo>().controller);
                    if (popupMokkin)
                        popupMokkin.GetComponent<DisplayPopup>().popupAppear(e.target, controller);//ポップアップを表示

                    foreach (Transform child in e.target)
                    {
                        child.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                        child.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.blue);
                    }


                    break;

                case "Button":
                    e.target.transform.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    e.target.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.blue);//エミッションカラー変更
                    break;
            }
        }
        else
            controller.GetComponent<laserInfo>().pointedObject = null;//ポインティングしているオブジェクトの枠を開けます
    }

    //================================================================================================
    //レーザーポインターがtargetから離れたとき
    public void PointerOutside(object sender, PointerEventArgs e, GameObject controller)
    {
        switch (e.target.tag)
        {
            //レーザーポインターを離したときにもとに戻します
            case "FaceParts":
                Transform face = GetChildWithTag(e.target.transform, "FaceMesh");
                face.GetComponent<Renderer>().materials = face.GetComponent<PartsTexture>().normalMaterials;//マテリアル列の一斉変換をする
                break;

            case "InteractableObject":
                e.target.transform.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");

                //ポップアップ消滅
                var popup = selectPopup(controller.GetComponent<laserInfo>().controller);
                if (popup)
                    popup.GetComponent<DisplayPopup>().popupDisappear();//ポップアップを消す
                break;

            case "Mokkin":
                //木琴のときは、モデルの構造上少しだけ異なる
                foreach (Transform child in e.target)
                {
                    child.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                }

                //ポップアップ消滅
                var popupMokkin = selectPopup(controller.GetComponent<laserInfo>().controller);
                if (popupMokkin)
                    popupMokkin.GetComponent<DisplayPopup>().popupDisappear();//ポップアップを消す
                break;

            case "Button":

                e.target.transform.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                break;

        }
        controller.GetComponent<laserInfo>().pointedObject = null;//ポインティングしているオブジェクトの枠を開けます
    }

    //////////////////////////////////////////////////////////
    //インタラクションを操作する
    public void pointerInteraction(Transform pointed, GameObject controller)
    {
        var bootInteraction = pointed.GetComponent<Interaction>();
        if (bootInteraction)
            bootInteraction.bootInteraction(controller);
    }

    /////////////////////////////////////////////////////////
    //表示・削除するポップアップを選択する
    GameObject selectPopup(laserInfo.controllerLR controller)//判別にはコントローラIDを使う
    {
        GameObject popupObj = null;//表示するポップアップを格納
        if (controller == laserInfo.controllerLR.Left) popupObj = popupObjLeft;
        else if (controller == laserInfo.controllerLR.Right) popupObj = popupObjRight;

        return popupObj;
    }

}



