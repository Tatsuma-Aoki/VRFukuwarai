using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using TMPro;

/// <summary>
/// カメラを変更するやつ
/// </summary>

public class CameraChanger : MonoBehaviour
{


    ////////////////////////////////////////////////////////////////
    //カメラ関係
    [Header("Camera")]
    public int cameraMode = 0;//カメラのモード これを基準にして切り替えます
    public GameObject normalView, leftView, rightView, dummyView;//正常視点、左目、右目、ダミー
    private Camera normalCamera, leftCamera, rightCamera, dummyCamera;//↑の各カメラコンポーネント

    ///////////////////////////////////////////////////////////////
    //コントローラ関係
    [Header("Controller")]
    public SteamVR_Action_Boolean pushedB = SteamVR_Actions.default_ButtonB;//Bボタンが押されたか確認
    public SteamVR_Action_Boolean pushedY = SteamVR_Actions.default_ButtonY;//Yボタンが押されたか確認
    private bool isChanged = false;//カメラ切り替えできたかを判断する
    //↑の仕様だと、押している間ずっと切り替え続けるため、それを防ぐために一回だけ反応するようにする
    //publicなのはデバッグ用
    [Header("Canvas")]
    public GameObject canvas;
    public TextMeshProUGUI cameraModeText;
    public Camera renderCamera;

    void Start()
    {
        normalCamera = normalView.GetComponent<Camera>();
        leftCamera = leftView.GetComponent<Camera>();
        rightCamera = rightView.GetComponent<Camera>();
        dummyCamera = dummyView.GetComponent<Camera>();
        //それぞれの視点のカメラコンポーネントを取得

        cameraModeText.text = "正常視点(Normal View)";
        renderCamera = canvas.GetComponent<Canvas>().worldCamera;
    }
    void Update()
    {
        //コントローラ用

        if (pushedB.GetState(SteamVR_Input_Sources.RightHand) || pushedY.GetState(SteamVR_Input_Sources.LeftHand))
        {
            if (!isChanged)
            {
                CameraChange();//押されたらカメラチェンジ
                isChanged = true;
            }
        }
        else isChanged = false;
    }

    void CameraChange()
    {
        //Update内でキーが押されたら呼び出される関数
        //HMDに映す映像を変更させる
        switch (cameraMode)
        {
            case 0://正常モード→両目視点モード
                normalCamera.enabled = false;
                leftCamera.enabled = true;
                leftCamera.stereoTargetEye = StereoTargetEyeMask.Left;
                rightCamera.enabled = true;
                rightCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                cameraMode++;

                canvas.transform.parent = leftCamera.transform;
                cameraModeText.text = "両目視点(Both Eye View)";

                canvas.GetComponent<Canvas>().worldCamera = leftCamera;//レンダーカメラを変更し、眼のカメラにする
                break;

            case 1://両目視点モード→左目視点モード
                rightCamera.enabled = false;
                leftCamera.stereoTargetEye = StereoTargetEyeMask.Both;//左目を両目視点にする
                cameraMode++;

                cameraModeText.text = "左目視点(Left Eye View)";
                break;

            case 2://左目視点モード→右目視点モード
                leftCamera.enabled = false;
                rightCamera.enabled = true;
                rightCamera.stereoTargetEye = StereoTargetEyeMask.Both;//右目を両目視点にする
                cameraMode++;

                canvas.transform.parent = rightCamera.transform;
                cameraModeText.text = "右目視点(Right Eye View)";

                canvas.GetComponent<Canvas>().worldCamera = rightCamera;
                break;

            case 3://右目視点モード→左目のみ表示モード
                rightCamera.enabled = false;
                leftCamera.enabled = true;
                leftCamera.stereoTargetEye = StereoTargetEyeMask.Left;//左目を左目のみの視点にする
                dummyCamera.enabled = true;
                dummyCamera.stereoTargetEye = StereoTargetEyeMask.Right;//右目を何も映さない視点にする
                cameraMode++;

                canvas.transform.parent = leftCamera.transform;
                cameraModeText.text = "左目視点(Left Eye View)";

                canvas.GetComponent<Canvas>().worldCamera = leftCamera;
                break;

            case 4://左目のみ表示モード→右目のみ表示モード
                leftCamera.enabled = false;
                rightCamera.enabled = true;
                rightCamera.stereoTargetEye = StereoTargetEyeMask.Right;//右目を右目のみの視点にする
                dummyCamera.stereoTargetEye = StereoTargetEyeMask.Left;//左目を何も映さない視点にする
                cameraMode++;

                canvas.transform.parent = rightCamera.transform;
                cameraModeText.text = "右目視点(Right Eye View)";

                canvas.GetComponent<Canvas>().worldCamera = rightCamera;
                break;

            case 5://右目のみ表示モード→正常表示モード
                rightCamera.enabled = false;
                dummyCamera.enabled = false;
                normalCamera.enabled = true;
                cameraMode = 0;

                canvas.transform.parent = normalCamera.transform;
                cameraModeText.text = "正常視点(Normal View)";

                canvas.GetComponent<Canvas>().worldCamera = normalCamera;
                break;
        }


    }
}
