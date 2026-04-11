using UnityEngine;

public static class TrashVfxUtility
{
    public static void SpawnOneShot(GameObject prefab, Vector3 position, Vector3 offset, float destroyDelay = 2f)
    {
        if (prefab == null) return;

        GameObject vfx = Object.Instantiate(prefab, position + offset, Quaternion.identity);
        Object.Destroy(vfx, destroyDelay);
    }
}