using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceRegistry : MonoBehaviour
{
    private static ServiceRegistry _instance = null;
    public static ServiceRegistry Instance => _instance;

    private Dictionary<Type, object> _services = new Dictionary<Type, object>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public void Unregister<T>(T instance) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var existing) && ReferenceEquals(existing, instance))
        {
            _services.Remove(typeof(T));
        }
    }

    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out object service))
        {
            return service as T;
        }

        return null;
    }

    public bool Has<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }
}