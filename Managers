using UnityEngine;

/// <summary>
/// 게임의 핵심 매니저들을 관리하는 싱글톤 클래스입니다.
/// </summary>
public class Managers : MonoBehaviour
{
    private static Managers instance;
    public static Managers Instance => instance;

    private ObjectPoolManager objectPoolManager;
    public static ObjectPoolManager Pool { get { return instance.objectPoolManager; } }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);

        // ObjectPoolManager 생성
        CreateManagers();
    }

    /// <summary>
    /// ObjectPoolManager를 생성합니다.
    /// </summary>
    private void CreateManagers()
    {
        objectPoolManager = CreateManager<ObjectPoolManager>(transform);
    }

    /// <summary>
    /// 매니저 컴포넌트를 생성하고 초기화합니다.
    /// </summary>
    private static T CreateManager<T>(Transform parent) where T : Component, IManager
    {
        GameObject go = new GameObject(typeof(T).Name);
        T manager = go.AddComponent<T>();
        go.transform.SetParent(parent);
        return manager;
    }

    private void Start()
    {
        InitializeManagers();
    }

    /// <summary>
    /// ObjectPoolManager를 초기화합니다.
    /// </summary>
    private void InitializeManagers()
    {
        try
        {
            objectPoolManager.Init();
        }
        catch (System.Exception ex)
        {
            Logger.ErrorLog($"ObjectPoolManager 초기화 실패: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
