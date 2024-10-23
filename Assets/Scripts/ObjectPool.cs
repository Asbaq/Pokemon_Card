using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 20;
    public Transform scrollContent;  // Parent object in the ScrollView for buttons
    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    public swipe swipe;

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab,scrollContent);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
           swipe.enabled = true;
    }

    public GameObject GetObject()
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, scrollContent);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}

