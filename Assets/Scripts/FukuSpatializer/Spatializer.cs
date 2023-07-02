using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace FukuSpatializer
{
    [RequireComponent(typeof(AudioSource))]
    public class Spatializer : MonoBehaviour
    {
        private Complex[] signalBufferL, signalBufferR;
        private Complex[] filterBufferL, filterBufferR;
        private Complex[] olsSignalBufferL, olsSignalBufferR;
        private Complex[] prvL, prvR; // フィルタの要素数をMとして前のラストN個分のデータを取っておく
        private int M;
        private static readonly int N = 1024; // Windows環境下で1024固定？現状1024以外になったことがない
        private bool isStarted = false;
        [SerializeField] private float lenStd = 1;
        [SerializeField] private float scale = 1;
        private float lenL = 1, lenR = 1;
        private Utility utility;
        private void Start()
        {
            utility = Utility.Instance;
            M = utility.filterN;
            signalBufferL = new Complex[N + M];
            signalBufferR = new Complex[N + M];
            olsSignalBufferL = new Complex[M * 2];
            olsSignalBufferR = new Complex[M * 2];
            filterBufferL = new Complex[M * 2];
            filterBufferR = new Complex[M * 2];
            prvL = new Complex[M];
            prvR = new Complex[M];
            isStarted = true;
        }
        private UnityEngine.Vector3 sourcePosL, sourcePosR;
        private void Update()
        {
            sourcePosL = utility.GetPositionFromEarL(transform.position);
            sourcePosR = utility.GetPositionFromEarR(transform.position);
            lenL = sourcePosL.magnitude;
            lenR = sourcePosR.magnitude;
        }
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isStarted) return; // Startより早く呼ばれうるのでその場合は処理を行わない

            Debug.Assert(channels == 2);
            int n = data.Length / channels;
            Debug.Assert(n <= N);

            // 音声データを左右分け、複素数型にする。
            for (int i = 0; i < n; i++)
            {
                signalBufferL[M + i] = new Complex(data[2 * i], 0);
                signalBufferR[M + i] = new Complex(data[2 * i + 1], 0);
            }
            // 音声データの最初の部分に過去Mフレームを連結
            for (int i = 0; i < M; i++)
            {
                signalBufferL[i] = prvL[i];
                signalBufferR[i] = prvR[i];
            }

            // ラストM個分保存
            if (n < M)
            {
                for (int i = 0; i < M - n; i++)
                {
                    prvL[i] = prvL[i + n];
                    prvR[i] = prvR[i + n];
                }
                for (int i = M - n; i < M; i++)
                {
                    prvL[i] = data[2 * (n - M + i)];
                    prvR[i] = data[2 * (n - M + i) + 1];
                }
            }
            else
            {
                for (int i = 0; i < M; i++)
                {
                    prvL[i] = new Complex(data[2 * (n - M + i)], 0);
                    prvR[i] = new Complex(data[2 * (n - M + i) + 1], 0);
                }
            }

            utility.GetFilter(sourcePosL, sourcePosR, filterBufferL, filterBufferR, out float delayLeft, out float delayRight);
            // フィルタの後半をゼロフィルする
            for (int i = 0; i < M; i++)
            {
                filterBufferL[M + i] = new Complex(0, 0);
                filterBufferR[M + i] = new Complex(0, 0);
            }
            Fourier.Forward(filterBufferL);
            Fourier.Forward(filterBufferR);

            float ratioL = Mathf.Min(Mathf.Pow(lenL / lenStd, -2), 1);
            float ratioR = Mathf.Min(Mathf.Pow(lenR / lenStd, -2), 1);

            // 畳み込み(Overlap-save method)
            for (int s = 0; s + 2 * M <= n + M; s += M)
            {
                for (int i = 0; i < 2 * M; i++)
                {
                    olsSignalBufferL[i] = signalBufferL[s + i];
                    olsSignalBufferR[i] = signalBufferR[s + i];
                }
                Fourier.Forward(olsSignalBufferL);
                Fourier.Forward(olsSignalBufferR);
                for (int i = 0; i < 2 * M; i++)
                {
                    olsSignalBufferL[i] *= filterBufferL[i];
                    olsSignalBufferR[i] *= filterBufferR[i];
                }
                Fourier.Inverse(olsSignalBufferL);
                Fourier.Inverse(olsSignalBufferR);
                for (int i = 0; i < M; i++)
                {
                    data[(s * 2) + (i * 2)] = (float)olsSignalBufferL[M + i].Real * ratioL * scale;
                    data[(s * 2) + (i * 2) + 1] = (float)olsSignalBufferR[M + i].Real * ratioR * scale;

                }
            }



        }
    }
}