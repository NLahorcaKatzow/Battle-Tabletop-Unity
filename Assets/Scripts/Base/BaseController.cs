using Newtonsoft.Json;
using UnityEngine;
using Zenject;

public class BaseController : MonoBehaviour
{
    private ResourceController _resourceController;

    [Inject]
    private void Construct(ResourceController resourceController)
    {
        _resourceController = resourceController;
        _resourceController.OnCompleteResourcesLoaded += Initializate;

        if (_resourceController.IsReady)
        {
            Initializate();
        }
    }

    private void OnDestroy()
    {
        if (_resourceController != null)
        {
            _resourceController.OnCompleteResourcesLoaded -= Initializate;
        }
    }

    public virtual void Initializate()
    {
        Debug.Log("BaseController initializated");
    }

    public string SerializeObject(object obj)
    {
        string data = JsonConvert.SerializeObject(obj);
        return data;
    }

    public void Log(string tag, string message)
    {
        Debug.Log($"#{tag}#: {message}");
    }

    public void LogError(string message)
    {
        Debug.LogError($"{message}");
    }

    public void Log(string tag, object obj)
    {
        Debug.Log($"#{tag}#: {SerializeObject(obj)}");
    }
}