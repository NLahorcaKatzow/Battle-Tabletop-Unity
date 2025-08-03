using Newtonsoft.Json;
using UnityEngine;
using Zenject;
public class BaseController : MonoBehaviour
{
    
[Inject] private ResourceController _resourceController;

    private void Start()
    {
        _resourceController.OnCompleteResourcesLoaded += Initializate;
    }

    private void OnDestroy()
    {
        _resourceController.OnCompleteResourcesLoaded -= Initializate;
    }

    public virtual void Initializate()
    {
    }
    
    public string SerializeObject(object obj)
    {
        string data = JsonConvert.SerializeObject(obj);
        return data;
    }
    
    public void Log(string tag,string message)
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