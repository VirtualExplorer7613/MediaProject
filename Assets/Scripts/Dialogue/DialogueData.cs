using System;
using System.Collections.Generic;

[Serializable]
public class DialogueData
{
    public string character;
    public bool icon_on_detected;
    public Expressions expressions;
    public List<DialogueEntry> dialogue_sequence;
}
