using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 4)]
    public string dialogueText;

    public List<DialogueAction> actions = new List<DialogueAction>();
}
