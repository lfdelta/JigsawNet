using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueObjectManager : MonoBehaviour
{
    public delegate void RequestResponseDelegate(GameObject RequestedObject);

    private Dictionary<string, GameObject> StoredObjects;

    private Dictionary<string, List<RequestResponseDelegate>> UnservicedRequests;


    private void Awake()
    {
        StaticJigsawData.ObjectManager = this;
        StoredObjects = new Dictionary<string, GameObject>();
        UnservicedRequests = new Dictionary<string, List<RequestResponseDelegate>>();
    }


    public void RegisterObject(GameObject Object, string UniqueTag)
    {
        if (StoredObjects.ContainsKey(UniqueTag))
        {
            Debug.LogErrorFormat("UniqueObjectManager.RegisterObject received tag {0} which is already registered", UniqueTag);
            return;
        }
        StoredObjects.Add(UniqueTag, Object);
        List<RequestResponseDelegate> objectRequests;
        if (UnservicedRequests.TryGetValue(UniqueTag, out objectRequests))
        {
            foreach (RequestResponseDelegate request in objectRequests)
            {
                request(Object);
            }
            UnservicedRequests.Remove(UniqueTag);
        }
    }


    public void RequestObject(string UniqueTag, RequestResponseDelegate Callback)
    {
        GameObject outObj;
        if (StoredObjects.TryGetValue(UniqueTag, out outObj))
        {
            Callback(outObj);
            return;
        }
        List<RequestResponseDelegate> requestList;
        if (UnservicedRequests.TryGetValue(UniqueTag, out requestList))
        {
            requestList.Add(Callback);
        }
        else
        {
            requestList = new List<RequestResponseDelegate>();
            requestList.Add(Callback);
            UnservicedRequests.Add(UniqueTag, requestList);
        }
    }
}
