using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killObjectsAfterTime : MonoBehaviour
{
    public float lifetime;
    public GameObject[] objectsToKill;
    public bool killThisObject;

    public bool unscaledTime;

    void Start()
    {
        StartCoroutine(despawn());
    }

    IEnumerator despawn()
    {
        if (!unscaledTime)
        {
            yield return new WaitForSeconds(lifetime);
        } else
        {
            yield return new WaitForSecondsRealtime(lifetime);
        }
        foreach (GameObject g in objectsToKill)
        {
            Destroy(g);
        }
        if (killThisObject) { Destroy(gameObject); }
    }
}
