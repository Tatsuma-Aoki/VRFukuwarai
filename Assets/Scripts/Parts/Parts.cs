using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Parts : MonoBehaviour
{

    [SerializeField] private Transform targetBoneRoot;
    [SerializeField] private Vector3 forward = Vector3.forward, up = Vector3.up; // 前方向、上方向
    [SerializeField] private float rayLength = 0.3f, hoverLength = 0.01f;
    [SerializeField] private string targetMeshLayerName = "Player1Body", ignoreBoneTagName = "Ignore";
    [SerializeField] private Vector3 offset;
    [SerializeField] private List<Transform> bones;
    [SerializeField] private GameObject playerParts;
    [SerializeField] private Transform playerRootBone;
    private int layerMask;
    private LineRenderer lr;
    public bool canRaySee;
    //掴んでいるときにtrueにするー＞LaserGrabberで制御
    public int id;//顔のパーツ識別番号(0~5)　↑で使います

    [Header("PartsManager")]
    //パーツの位置情報とかを記録しておくやつ
    [SerializeField] private Transform oldParentBone;//顔のパーツの親オブジェクト
    [SerializeField] private Vector3 oldPosition;//顔のパーツの元位置
    [SerializeField] private Quaternion oldRotation;//顔のパーツの元回転

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
        lr = gameObject.GetComponent<LineRenderer>();
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        bones = new List<Transform>();
        GetChildBones(targetBoneRoot);
        layerMask = LayerMask.GetMask(targetMeshLayerName);


        //個々のパーツの位置情報と回転情報を記録
        oldParentBone = transform.parent;
        oldPosition = transform.position;
        oldRotation = transform.rotation;

        //初期ボーンとしても記録
        firstParentBone = oldParentBone;
        firstPosition = oldPosition;
        firstRotation = oldRotation;

    }

    public void Update()
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


    private Transform GetChildWithName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            var childRes = GetChildWithName(child, name);
            if (childRes)
            {
                return childRes;
            }
        }
        return null;
    }
    private void GetChildBones(Transform parent)
    {
        foreach (Transform child in parent.GetComponentInChildren<Transform>())
        {
            if (!child.gameObject.activeInHierarchy || child.gameObject.tag == "FaceParts")
                continue;
            bones.Add(child);
            GetChildBones(child);
        }
    }

    private void LateUpdate()
    {
        if (canRaySee)
        {
            if (!lr.enabled)
            {
                lr.enabled = true;
            }
            var dir = transform.TransformDirection(-forward);
            var worldPos = transform.TransformPoint(offset);
            lr.SetPositions(new Vector3[] { worldPos, worldPos + dir.normalized * rayLength });
        }
        else
        {
            lr.enabled = false;
        }
    }

    // オブジェクトからレイを飛ばし、対象の身体パーツ（腕など）に当たったらそこに身体パーツ（耳など）を貼り付ける
    public void SetPosition()
    {
        RaycastHit hit;
        var dir = transform.TransformDirection(-forward);

        if (Physics.Raycast(transform.position, dir, out hit, rayLength, layerMask))
        //レイキャストが当たった場合
        //Raycast(始点、向き、当たり、長さ、)
        {
            var hitPos = hit.point;//キャストの当たっている点
            var normal = hit.normal;

            // 最も近いボーンを見つける
            float maxDist = float.PositiveInfinity;
            Transform nearestBone = null;
            foreach (Transform bone in bones)
            {
                float dist = (bone.position - hitPos).magnitude;
                if (dist < maxDist)
                {
                    maxDist = dist;
                    nearestBone = bone;
                }
            }
            // 指先など一部のボーンは無視する
            //if (nearestBone.tag == ignoreBoneTagName)
            //{
            //    return;
            //}

            // if (!nearestBone) Debug.Log(nearestBone);

            transform.rotation = Quaternion.LookRotation(normal, transform.TransformDirection(up)); // 回転
                                                                                                    // var movedWorldPos = transform.TransformPoint(offset);    
                                                                                                    //            transform.position += hitPos - movedWorldPos; // 位置
            transform.position = hitPos + (hit.normal * hoverLength); // 位置


            // // 最も近いボーンorその親のどちらかに追従させる
            // var nearestBonePos = nearestBone.position;
            // var parentBonePos = nearestBone.parent.position;

            // gameObject.transform.parent = nearestBone.transform.parent;
            // if(gameObject.transform.parent)Debug.Log("parent!"); 

            // if ((nearestBonePos - parentBonePos).sqrMagnitude > (hitPos - parentBonePos).sqrMagnitude)
            // {
            //     Debug.Log("Hit1");
            // }
            // else Debug.Log("Noaaaaaaa");
            // 最も近いボーンorその親のどちらかに追従させる
            /* Debug.Log("parentBone");
             Debug.Log(nearestBone);
             Debug.Log(nearestBone.parent);
             Debug.Log(hitPos);*/


            var nearestBonePos = nearestBone.position;
            var parentBonePos = nearestBone.parent.position;
            if ((nearestBonePos - parentBonePos).sqrMagnitude > (hitPos - parentBonePos).sqrMagnitude)
            {
                transform.parent = nearestBone.transform.parent;
            }
            else
            {
                transform.parent = nearestBone.transform;
            }

            var playerPartsParent = GetChildWithName(playerRootBone, transform.parent.name);
            playerParts.transform.parent = playerPartsParent;
            playerParts.transform.localPosition = transform.localPosition;
            playerParts.transform.localRotation = transform.localRotation;

            //ParentManagerに、配置された位置・回転・親ボーンの情報を記録する
            oldParentBone = transform.parent;
            oldPosition = transform.position;
            oldRotation = transform.rotation;
        }
        else
        {//レイキャストが当たらなかった場合、最後にヒットした親情報を基にして、パーツをその位置に戻す
            Debug.Log("a");
            transform.parent = oldParentBone;//親の情報
            transform.position = oldPosition;//レイキャストがあたらなかったらその位置に戻す
            transform.rotation = oldRotation;//回転ももとにもどす
        }
    }

    private void facePartsInitialize()
    {
        //位置を初期化する
        transform.parent = firstParentBone;//初期の親ボーンを親にする
        transform.position = firstPosition;//相対座標に移動する
        transform.rotation = firstRotation;//回転させる
    }
}