using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Numerics;
using UnityEngine.SceneManagement;

namespace FukuSpatializer
{
    public class Utility : SingletonMonoBehaviour<Utility>
    {
        [SerializeField] string filename = "Sofa/RIEC_hrir_subject_001.sofa"; // Assets/StreamingAssets/以下を指定
        [SerializeField] float samplerate = 48000;
        // 今後の拡張のために一応SOFAファイルは最大５つまで扱えるようにしてあり、操作対象を0~4の数字で指定できる。このラッパークラスでは1つのみ使うことを想定している。
        [SerializeField] int sofaNum = 0;

        public int filterN { get; private set; }
        private bool isLeftFirst;

        // Open, Close処理
        [DllImport("libmysofa_wrapper.dll", CharSet = CharSet.Ansi)]
        private static extern int mysofa_open_wr(int sofa_num, string filename, float samplerate, out int filterlength);
        [DllImport("libmysofa_wrapper.dll", CharSet = CharSet.Ansi)]
        private static extern int mysofa_open_advanced_wr(int sofa_num, string filename, float samplerate, out int filterlength, int norm, float neighbor_angle_step, float neighbor_radius_step);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern void mysofa_close_wr(int sofa_num);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern void mysofa_close_cached_wr(int sofa_num);

        // 移動後の耳位置におけるIRを取得する
        [DllImport("libmysofa_wrapper.dll")]
        private static extern void getfilter_wr(int sofa_num, float lx, float ly, float lz, float rx, float ry, float rz, float[] IRleft, float[] IRright, out float delayLeft, out float delayRight);

        // Sofaファイルのデータにアクセスするメソッド
        [DllImport("libmysofa_wrapper.dll")]
        private static extern int getListenerPosition(int sofanum, float[] dst);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern int getReceiverPosition(int sofanum, float[] dst);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern int getSourcePosition(int sofanum, float[] dst);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern int getEmitterPosition(int sofanum, float[] dst);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern int getListenerUp(int sofanum, float[] dst);
        [DllImport("libmysofa_wrapper.dll")]
        private static extern int getListenerView(int sofanum, float[] dst);

        public Transform earL { get; private set; }
        public Transform earR { get; private set; }

        private UnityEngine.Vector3 listenerPosition, receiverPositionL, receiverPositionR;

        public bool IsAvailable
        {
            get; private set;
        }
        protected override void Awake()
        {
            base.Awake();
            IsAvailable = false;
            int err = mysofa_open_wr(sofaNum, Application.dataPath + "/StreamingAssets/" + filename, samplerate, out int filterLength);
            if (err == 0)
            {
                Debug.Log($"'{Application.dataPath + "/StreamingAssets/" + filename}'をロードしました。");
            }
            else
            {
                Debug.Log($"'{Application.dataPath + "/StreamingAssets/" + filename}'のロードに失敗しました。");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
            }
            filterN = filterLength;
            listenerPosition = GetListenerPosition();
            var up = GetListenerUp();
            var (r1, r2) = GetReceiverPosition();
            var view = GetListenerView();
            r1 -= listenerPosition;
            r2 -= listenerPosition;
            var cross = UnityEngine.Vector3.Cross(up, view);
            var d1 = UnityEngine.Vector3.Dot(cross, r1);
            var d2 = UnityEngine.Vector3.Dot(cross, r2);
            Debug.Assert(d1 * d2 < 0, "左右に耳が存在する形式ではないSOFAファイルです。想定外の挙動を起こす可能性があります。"); // 右側と左側に耳が存在する
            isLeftFirst = d1 < 0;

            if (isLeftFirst)
            {
                receiverPositionL = r1;
                receiverPositionR = r2;
            }
            else
            {
                receiverPositionR = r1;
                receiverPositionL = r2;
            }

            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene nextScene, LoadSceneMode mode)
        {
            Initialize();
        }

        public void Initialize()
        {
            var earLs = GameObject.FindGameObjectsWithTag("EarL");
            var earRs = GameObject.FindGameObjectsWithTag("EarR");

            Debug.Assert((earLs.Length == 1 && earRs.Length == 1) || (earLs.Length == 0 && earRs.Length == 0),
            "耳オブジェクト(TagがEarLかEarRのオブジェクト)はシーンに左右1つずつ配置するか、1つも配置しないかのどちらかにしてください。");

            if (earLs.Length == 1 && earRs.Length == 1)
            {
                earL = earLs[0].transform;
                earR = earRs[0].transform;
                IsAvailable = true;
                Debug.Log("audio start");
            }
            else
            {
                earL = null;
                earR = null;
                IsAvailable = false;
            }
        }
        private UnityEngine.Vector3 c2s(UnityEngine.Vector3 cPos)
        {
            float r = cPos.magnitude;
            float theta = Mathf.Atan2(cPos.z, Mathf.Sqrt(cPos.x * cPos.x + cPos.y * cPos.y));
            float phi = Mathf.Atan2(cPos.y, cPos.x);
            return new UnityEngine.Vector3(phi * (180 / Mathf.PI) + 360 % 360, theta * (180 / Mathf.PI), r);
        }
        public UnityEngine.Vector3 GetListenerPosition()
        {
            var pos = new float[3];
            int n = getListenerPosition(sofaNum, pos);
            Debug.Assert(n == pos.Length, "想定外の形式のSOFAファイルです。");
            return new UnityEngine.Vector3(-pos[1], pos[2], pos[0]);
        }
        public (UnityEngine.Vector3, UnityEngine.Vector3) GetReceiverPosition()
        {
            var pos = new float[3 * 2];
            int n = getReceiverPosition(sofaNum, pos);
            Debug.Assert(n == pos.Length, "想定外の形式のSOFAファイルです。");
            return (new UnityEngine.Vector3(-pos[1], pos[2], pos[0]), new UnityEngine.Vector3(-pos[4], pos[5], pos[3]));
        }
        public UnityEngine.Vector3 GetListenerUp()
        {
            var up = new float[3];
            int n = getListenerUp(sofaNum, up);
            Debug.Assert(n == up.Length, "想定外の形式のSOFAファイルです。");
            return new UnityEngine.Vector3(-up[1], up[2], up[0]);
        }
        public UnityEngine.Vector3 GetListenerView()
        {
            var view = new float[3];
            int n = getListenerView(sofaNum, view);
            Debug.Assert(n == view.Length, "想定外の形式のSOFAファイルです。");
            return new UnityEngine.Vector3(-view[1], view[2], view[0]);
        }

        // 移動先の耳が頭についているものと考え、その状態における聞き手の位置を原点とした座標
        public void GetFilter(UnityEngine.Vector3 lPos, UnityEngine.Vector3 rPos, Complex[] IRleft, Complex[] IRright, out float delayLeft, out float delayRight)
        {
            float[] irl = new float[filterN];
            float[] irr = new float[filterN];
            // lPos = c2s(lPos);
            // rPos = c2s(rPos);
            getfilter_wr(sofaNum, lPos.z, -lPos.x, lPos.y, rPos.z, -rPos.x, rPos.y, irl, irr, out delayLeft, out delayRight);
            for (int i = 0; i < filterN; i++)
            {
                IRleft[i] = new Complex(irl[i], 0);
                IRright[i] = new Complex(irr[i], 0);
            }
        }
        public UnityEngine.Vector3 GetPositionFromEarL(UnityEngine.Vector3 sourcePosition)
        {
            Debug.Assert(earL != null, "耳オブジェクトが配置されているシーンでのみこの関数は呼ぶことができます。");
            return earL.transform.InverseTransformPoint(sourcePosition) - receiverPositionL;
        }
        public UnityEngine.Vector3 GetPositionFromEarR(UnityEngine.Vector3 sourcePosition)
        {
            Debug.Assert(earR != null, "耳オブジェクトが配置されているシーンでのみこの関数は呼ぶことができます。");
            return earR.transform.InverseTransformPoint(sourcePosition) - receiverPositionR;
        }
    }
}