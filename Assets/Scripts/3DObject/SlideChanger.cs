using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

public class SlideChanger : MonoBehaviour
{
    /////////////////////////////////////////
    //チュートリアルのスライド変化を行う
    //スライドに使用するオブジェクト関係
    /// <summary>
    /// 
    /// </summary>
    public GameObject[] slides, viveSlides, MetaSlides;//表示するスライド群
    public int number, slideMAX;//スライドナンバーとスライド番号の最大値
    public HMDChanger hmdChanger;

    void Start()
    {
        number = 0;
        if (hmdChanger.hmd == HMDChanger.HMD.MetaQ2) slides = MetaSlides;
        else if (hmdChanger.hmd == HMDChanger.HMD.Vive) slides = viveSlides;//HMDの種類に応じてスライドを変える

        foreach (GameObject slide in slides) slide.SetActive(false);
        slides[number].SetActive(true);//1枚目のスライドだけアクティブにします

    }
    public void slideChange(string kindOfButton)
    {
        if (kindOfButton == "Front")//すすむボタン
        {
            if (number < slideMAX - 1)//最後のスライドを表示していなければ
            {
                number++;
                foreach (GameObject slide in slides) slide.SetActive(false);
                slides[number].SetActive(true);//number枚目のスライドだけアクティブにします
            }
        }

        else if (kindOfButton == "Back")
        {
            if (number > 0)//最初のイラストを表示していなければ
            {
                number--;
                foreach (GameObject slide in slides) slide.SetActive(false);
                slides[number].SetActive(true);//number枚目のスライドだけアクティブにします
            }
        }
    }
}
