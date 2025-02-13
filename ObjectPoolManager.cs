using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 오브젝트 풀링을 전역적으로 관리하는 싱글톤 매니저 클래스입니다.
/// 프리팹 인스턴스를 재사용하여 메모리 할당과 가비지 컬렉션을 최소화합니다.
/// </summary>
public class ObjectPoolManager : MonoBehaviour, IManager
{
    // 프리팹의 ID를 키로 사용하고, 해당 프리팹의 오브젝트 풀을 값으로 저장하는 딕셔너리
    // object 타입으로 저장하여 다양한 컴포넌트 타입의 풀을 지원합니다.
    private Dictionary<int, object> poolCache = new Dictionary<int, object>();
    private readonly object poolLock = new object();

    // 풀링된 모든 오브젝트들의 부모 트랜스폼
    // 하이어라키 창을 깔끔하게 유지하기 위해 사용됩니다.
    private Transform poolContainer;

    /// <summary>
    /// 오브젝트 풀 매니저를 초기화합니다.
    /// 풀링된 오브젝트들을 담을 컨테이너를 생성합니다.
    /// </summary>
    public void Init()
    {
        lock (poolLock)
        {
            poolContainer = new GameObject("PoolContainer").transform;
            poolContainer.SetParent(transform);
        }
    }

    /// <summary>
    /// 지정된 프리팹의 오브젝트 풀을 가져오거나 새로 생성합니다.
    /// </summary>
    /// <typeparam name="T">풀링할 컴포넌트의 타입</typeparam>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="defaultCapacity">풀의 초기 용량 (기본값: 10)</param>
    /// <param name="maxSize">풀의 최대 크기 (기본값: 100)</param>
    /// <returns>해당 프리팹의 오브젝트 풀</returns>
    public IObjectPool<T> GetPool<T>(T prefab, int defaultCapacity = 10, int maxSize = 100) where T : Component
    {
        int prefabId = prefab.GetInstanceID();

        lock (poolLock)
        {
            // 이미 해당 프리팹의 풀이 존재하는 경우 캐시된 풀을 반환
            if (poolCache.TryGetValue(prefabId, out var existingPool))
            {
                // 캐시된 풀을 요청된 타입으로 캐스팅 시도
                if (existingPool is ObjectPool<T> typedPool)
                {
                    return typedPool;
                }
                else
                {
                    Logger.ErrorLog($"[오브젝트 풀 매니저] {prefab.name} 프리팹의 풀이 잘못된 타입으로 캐시되어 있습니다. 예상 타입: {typeof(T).Name}");
                    return null;
                }
            }

            // 새로운 풀 생성
            // 순환 참조를 피하기 위해 변수를 미리 선언
            ObjectPool<T> newPool = null;
            newPool = new ObjectPool<T>(
                createFunc: () =>
                {
                    var instance = CreatePooledItem(prefab);
                    // PooledObject 컴포넌트가 있는 경우 풀 참조 초기화
                    if (instance is PooledObject<T> pooledObject)
                    {
                        pooledObject.InitializePool(newPool);
                    }
                    return instance;
                },
                actionOnGet: OnTakeFromPool,      // 풀에서 객체를 가져올 때 실행할 동작
                actionOnRelease: OnReturnToPool,   // 풀에 객체를 반환할 때 실행할 동작
                actionOnDestroy: OnDestroyPoolObject, // 풀에서 객체가 제거될 때 실행할 동작
                collectionCheck: true,             // 동일한 객체가 여러 번 반환되는 것을 방지
                defaultCapacity: defaultCapacity,   // 풀의 초기 용량
                maxSize: maxSize                   // 풀의 최대 크기
            );

            // 생성된 풀을 캐시에 저장
            poolCache.Add(prefabId, newPool);
            return newPool;
        }
    }

    /// <summary>
    /// 풀링된 아이템의 새 인스턴스를 생성합니다.
    /// </summary>
    /// <typeparam name="T">생성할 컴포넌트 타입</typeparam>
    /// <param name="prefab">생성할 프리팹</param>
    /// <returns>생성된 인스턴스</returns>
    private T CreatePooledItem<T>(T prefab) where T : Component
    {
        var instance = Instantiate(prefab);
        instance.name = prefab.name; // 프리팹 이름 유지
        instance.transform.SetParent(poolContainer); // 풀 컨테이너의 자식으로 설정
        return instance;
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져올 때 호출되는 메서드입니다.
    /// 오브젝트를 활성화하고 IPoolable 인터페이스가 구현된 경우 OnSpawn을 호출합니다.
    /// </summary>
    private void OnTakeFromPool<T>(T obj) where T : Component
    {
        obj.gameObject.SetActive(true);
        if (obj is IPoolable poolable)
        {
            poolable.OnSpawn();
        }
    }

    /// <summary>
    /// 오브젝트가 풀로 반환될 때 호출되는 메서드입니다.
    /// 오브젝트를 비활성화하고 IPoolable 인터페이스가 구현된 경우 OnDespawn을 호출합니다.
    /// </summary>
    private void OnReturnToPool<T>(T obj) where T : Component
    {
        obj.gameObject.SetActive(false);
        if (obj is IPoolable poolable)
        {
            poolable.OnDespawn();
        }
    }

    /// <summary>
    /// 풀에서 오브젝트가 제거될 때 호출되는 메서드입니다.
    /// 풀의 최대 크기를 초과하거나 풀이 Clear될 때 호출됩니다.
    /// </summary>
    private void OnDestroyPoolObject<T>(T obj) where T : Component
    {
        Destroy(obj.gameObject);
    }

    public void Clear()
    {
        lock (poolLock)
        {
            // 모든 풀의 오브젝트 제거
            foreach (var pool in poolCache.Values)
            {
                if (pool is IObjectPool<Component> componentPool)
                {
                    componentPool.Clear();
                }
            }
            poolCache.Clear();

            // 풀 컨테이너의 모든 자식 오브젝트 제거
            if (poolContainer != null)
            {
                for (int i = poolContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(poolContainer.GetChild(i).gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 현재 사용 중인 풀의 프리팹 ID 목록을 반환합니다.
    /// </summary>
    public HashSet<int> GetActivePoolPrefabIds()
    {
        lock (poolLock)
        {
            return new HashSet<int>(poolCache.Keys);
        }
    }

    /// <summary>
    /// 특정 프리팹 ID의 풀을 제거합니다.
    /// </summary>
    public void RemovePool(int prefabId)
    {
        lock (poolLock)
        {
            if (poolCache.TryGetValue(prefabId, out var pool))
            {
                if (pool is IObjectPool<Component> componentPool)
                {
                    componentPool.Clear();
                }
                poolCache.Remove(prefabId);
            }
        }
    }

    /// <summary>
    /// 사용하지 않는 풀들을 정리합니다.
    /// </summary>
    /// <param name="activePoolIds">현재 활성화된 풀의 ID 목록</param>
    public void CleanupUnusedPools(HashSet<int> activePoolIds)
    {
        lock (poolLock)
        {
            var unusedPoolIds = poolCache.Keys.Where(id => !activePoolIds.Contains(id)).ToList();
            foreach (var id in unusedPoolIds)
            {
                RemovePool(id);
            }
        }
    }
}