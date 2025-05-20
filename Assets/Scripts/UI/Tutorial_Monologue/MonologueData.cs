// Assets/Scripts/Narration/MonologueData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Narration/Monologue Data")]
public class MonologueData : ScriptableObject
{
    public List<VisualTextStep> steps;
}
