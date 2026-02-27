using System;
using UnityEngine;

public class ItemUseBroadcaster : MonoBehaviour
{
    public event Action<string> ItemUsed;

    public void RaiseItemUsed(string itemId)
    {
        ItemUsed?.Invoke(itemId);
    }
}
