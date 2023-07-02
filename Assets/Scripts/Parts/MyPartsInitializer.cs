using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// �����̃p�[�c������������̂Ɏg��
/// </summary>

public class MyPartsInitializer : MonoBehaviour
{
    [Header("Initializer")]
    //�����ݒ肷����W�n
    [SerializeField] private Vector3 firstPosition;//�����̐e�{�[���Ƃ̑��΍��W
    [SerializeField] private Quaternion firstRotation;//�����̉�]
    [SerializeField] private Transform firstParentBone;//�����̐e�{�[���@Start�Ŏ擾���܂�

    [Header("SteamVR")]
    //SteamVR�̓��͌n�@
    public SteamVR_Action_Boolean pushedA = SteamVR_Actions.default_ButtonA;//A�{�^���������ꂽ���m�F
    public SteamVR_Action_Boolean pushedX = SteamVR_Actions.default_ButtonX;//X�{�^���������ꂽ���m�F
    private Boolean isPushedA = false, isPushedX = false;
    public Boolean initialized;
    //�����A��X���������ŋN�����邽�߁AGetState(SteamVR_Input_Sources.LeftHand or RightHand)�ł�Bool�l��and�Ŕ��f����

    void Start()
    {
        //�X�̃p�[�c�̈ʒu���Ɖ�]�����L�^
        firstParentBone = transform.parent;
        firstPosition = transform.position;
        firstRotation = transform.rotation;
    }

    private void Update()
    {
        isPushedA = pushedA.GetState(SteamVR_Input_Sources.RightHand);//�E���A�������ꂽ���H
        isPushedX = pushedX.GetState(SteamVR_Input_Sources.LeftHand);//�����X�������ꂽ���H

        if (isPushedA && isPushedX && !initialized)//�ǂ����������ꂽ��&�܂����������ĂȂ�������
        {
            facePartsInitialize();
            initialized = true;//������������I
        }
        else initialized = false;

    }
    private void facePartsInitialize()
    {
        //�ʒu������������
        transform.parent = firstParentBone;//�����̐e�{�[����e�ɂ���
        transform.position = firstPosition;//���΍��W�Ɉړ�����
        transform.rotation = firstRotation;//��]������
    }
}
