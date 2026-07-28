using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VisualNovel/DialogueScript")]
public class DialogueScript : ScriptableObject
{
    public string scriptName;
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Optional Settings")]
    public string nextSceneName; //  대사 종료 후 이동할 씬 이름 (없으면 무시)
}
