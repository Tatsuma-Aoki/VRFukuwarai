using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// �C���^���N�V���������I�u�W�F�N�g�ɓ���āC�󋵂ɉ������C���^���N�V�������s��
/// </summary>
public class Interaction : MonoBehaviour
{
    public bool isChanged = false;
    [Header("Slide")]
    public GameObject slide;//�X���C�h�`�F���W���[���Ăяo�����߂̃X���C�h
    public void bootInteraction(GameObject controller)
    {
        //�|�C���e�B���O���Ă���I�u�W�F�N�g�̃C���^���N�V�����𑀍�
        if (tag == "InteractableObject")
        {

            //���W�J�Z�́A�{�^���������ꂽ�特�y�Đ����邩�ۂ���؂�ւ���
            if (gameObject.name == "Razikase")
            {


                if (SteamVR_Actions.default_ButtonA.GetState(SteamVR_Input_Sources.RightHand) || SteamVR_Actions.default_ButtonX.GetState(SteamVR_Input_Sources.LeftHand))
                {

                    if (!isChanged)
                    {
                        if (GetComponent<AudioSource>().mute)
                        {
                            GetComponent<AudioSource>().mute = false;
                            GetComponent<AudioClipPlayer>().randomPlayer();//�K���ɉ��y���Đ�����֐��ł�
                        }
                        else GetComponent<AudioSource>().mute = true;
                        isChanged = true;
                    }
                }
                else isChanged = false;
            }
        }



        //�؋Ղ̏ꍇ
        else if (tag == "Mokkin")
        {
            if (SteamVR_Actions.default_ButtonA.GetState(SteamVR_Input_Sources.RightHand) || SteamVR_Actions.default_ButtonX.GetState(SteamVR_Input_Sources.LeftHand))
            {


                if (!isChanged)
                {
                    GetComponent<AudioClipPlayer>().randomPlayer();//�K����SE���Đ�����֐��ł�
                    isChanged = true;
                }
            }
            else isChanged = false;
        }

        //�{�^���̏ꍇ
        else if (tag == "Button")
        {
            if (controller.GetComponent<laserInfo>().LaserGrabbed)
            {//�{�^������������
                if (!isChanged)
                {
                    controller.GetComponent<laserInfo>().pointedObject.GetComponent<Renderer>().materials = controller.GetComponent<laserInfo>().pointedObject.GetComponent<PartsTexture>().pointedMaterials;//�}�e���A���ϊ�

                    isChanged = true;

                    if (controller.GetComponent<laserInfo>().pointedObject.GetComponent<ButtonID>().button == ButtonID.buttonKind.Front) slide.GetComponent<SlideChanger>().slideChange("Front");
                    else if (controller.GetComponent<laserInfo>().pointedObject.GetComponent<ButtonID>().button == ButtonID.buttonKind.Back) slide.GetComponent<SlideChanger>().slideChange("Back");
                }
            }
            else
            {
                isChanged = false;
                GetComponent<Renderer>().materials = GetComponent<PartsTexture>().normalMaterials;//�}�e���A���߂�
            }

        }

    }
}
