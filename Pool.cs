#define AdditionalValidation//checks was instance given when it tries to return
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PoolEmptyBehaviour
{
    AllocateMore,
    ReturnNull,
    ReturnCyclicallyAlreadyGiven
}

[RequireComponent(typeof(PoolManager))]
public class Pool : MonoBehaviour
{
    public GameObject Prefab;
    public int CountToAllocateInStart = 10;
    public Queue<GameObject> InsideInstances;
    public LinkedList<GameObject> OutsideInstances = new LinkedList<GameObject>();

    public bool ChangeActiveState = true;//enable and disable objects when it comes in or out pool.
    public bool ChangeParent = false;//enable and disable objects when it comes in or out pool.

    public PoolEmptyBehaviour EmptyBehaviour = PoolEmptyBehaviour.AllocateMore;
    public int CountToAllocateIfReachedLimit = 1;

    public GameObjectEvent InstanceTaking = new GameObjectEvent();
    public GameObjectEvent InstanceTaken = new GameObjectEvent();
    public GameObjectEvent InstanceReturning = new GameObjectEvent();
    public GameObjectEvent InstanceReturned = new GameObjectEvent();

    public string InternInstanceName { get; private set; }
    private bool _isInitialized = false;

    private void OnValidate()
    {
        if (CountToAllocateInStart < 0)
            CountToAllocateInStart = 0;

        if (CountToAllocateIfReachedLimit < 0)
            CountToAllocateIfReachedLimit = 1;
    }

    private void Awake()
    {
        Initialize();
    }

    public string Initialize()
    {
        if (!_isInitialized)
        {
            OnValidate();
            InternInstanceName = string.Intern(Prefab.name);
            InsideInstances = new Queue<GameObject>(CountToAllocateInStart);
            Allocate(CountToAllocateInStart);
            _isInitialized = true;
        }

        return InternInstanceName;
    }

    public void Allocate(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var instance = Instantiate(Prefab, transform);
            instance.name = InternInstanceName;

#if AdditionalValidation
            OutsideInstances.AddLast(instance);
#endif

            ReturnToPool(instance);
        }
    }

    public void ReturnToPool(GameObject instance)
    {
#if AdditionalValidation
        if (!OutsideInstances.Contains(instance))
        {
            if (!InsideInstances.Contains(instance))
                Debug.LogException(new ArgumentException($"This instance '{instance.name}' was not given, but tries to return."), instance);

            return;
        }
        OutsideInstances.Remove(instance);
#endif
        InstanceReturning.Invoke(instance);

        if (ChangeActiveState)
            instance.SetActive(false);

        if (ChangeParent)
            instance.transform.SetParent(transform);

        InsideInstances.Enqueue(instance);
        InstanceReturned.Invoke(instance);
    }

    public GameObject TakeFromPool()
    {
        GameObject res = null;

        if (InsideInstances.Count == 0)
        {
            switch (EmptyBehaviour)
            {
                case PoolEmptyBehaviour.AllocateMore:
                    Allocate(CountToAllocateIfReachedLimit);
                    res = InsideInstances.Dequeue();
                    break;

                case PoolEmptyBehaviour.ReturnCyclicallyAlreadyGiven:
                    var outsideOne = OutsideInstances.First.Value;
#if !AdditionalValidation
                    OutsideInstances.RemoveFirst();
#endif
                    ReturnToPool(outsideOne);
                    res = InsideInstances.Dequeue();
                    break;

                case PoolEmptyBehaviour.ReturnNull:
                    return res;

                default:
                    throw new ArgumentException(nameof(EmptyBehaviour));
            }
        }
        else
        {
            res = InsideInstances.Dequeue();
        }

        InstanceTaking.Invoke(res);

        if (ChangeParent)
            res.transform.SetParent(null);

        if (ChangeActiveState)
            res.SetActive(true);

        OutsideInstances.AddLast(res);
        InstanceTaken.Invoke(res);

        return res;
    }
}
