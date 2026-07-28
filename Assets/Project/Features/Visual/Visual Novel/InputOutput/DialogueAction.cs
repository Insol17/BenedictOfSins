using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueAction
{
    public enum ActionType
    {
        MoveCharacter,
        ShowCharacter,
        HideCharacter,
        ChangeBackground,
        CameraShake,
        CameraZoom,
        SetCharacterGrayScaleUI,
        SetCharacterNormalColor,
        SetFilmGrainIntensity,
        LoadScene,
        PlaySound
    }

    public ActionType actionType;  // 이 부분 꼭 필요!

    public string targetName;
    public Vector3 targetPosition;
    public float duration = 1f;
    public float intensity;
    public string sceneName;
    public AudioClip soundClip;
}

