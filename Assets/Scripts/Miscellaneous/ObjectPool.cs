using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    private static readonly Dictionary<string, Queue<GameObject>> objectPool = new();

    public static GameObject GetObject(GameObject gameObject)
    {
        if (objectPool.TryGetValue(gameObject.name, out Queue<GameObject> objectList))
        {
            if (objectList.Count == 0)
                return CreateNewObject(gameObject);

            else
            {
                GameObject pooledObject = objectList.Dequeue();

                if (pooledObject == null)
                    return CreateNewObject(gameObject);

                pooledObject.SetActive(true);
                return pooledObject;
            }
        }

        else
            return CreateNewObject(gameObject);
    }

    private static GameObject CreateNewObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogWarning("Attempted to instantiate a null GameObject.");
            return null;
        }

        GameObject newGO = Object.Instantiate(gameObject);
        newGO.name = gameObject.name;
        return newGO;
    }

    public static void ReturnGameObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogWarning("Attempted to return a null GameObject to the pool.");
            return;
        }

        gameObject.SetActive(false);

        if (objectPool.TryGetValue(gameObject.name, out Queue<GameObject> objectList))
            objectList.Enqueue(gameObject);

        else
        {
            Queue<GameObject> newObjectQueue = new();
            newObjectQueue.Enqueue(gameObject);
            objectPool.Add(gameObject.name, newObjectQueue);
        }
    }
}