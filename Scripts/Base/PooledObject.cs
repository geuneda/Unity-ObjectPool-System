using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 풀링 가능한 오브젝트의 기본 클래스
/// </summary>
/// <typeparam name="T">풀링될 컴포넌트의 타입</typeparam>
public abstract class PooledObject<T> : MonoBehaviour, IPoolable where T : Component
{
    // 이 오브젝트가 속한 오브젝트 풀 참조
    protected IObjectPool<T> pool;

    // 풀 초기화 상태를 추적하여 중복 초기화 방지
    private bool isPoolInitialized = false;

    // 컴포넌트 캐싱으로 GetComponent 호출 최소화
    private T cachedComponent;

    // Transform 컴포넌트 캐싱 (자주 사용되는 컴포넌트)
    protected Transform cachedTransform;

    /// <summary>
    /// 컴포넌트 참조 초기화 및 캐싱
    /// 파생 클래스에서 재정의할 때는 base.Awake()를 호출해야 함
    /// </summary>
    protected virtual void Awake()
    {
        // this가 T 타입이어야 함을 확실히 하기 위한 체크
        if (this is T component)
        {
            cachedComponent = component;
            cachedTransform = transform;
        }
        else
        {
            Logger.ErrorLog($"[PooledObject] {gameObject.name}에서 {typeof(T).Name}로의 캐스팅 실패. " +
                         "제네릭 타입 매개변수가 현재 컴포넌트 타입과 일치하는지 확인하세요.");
        }
    }

    /// <summary>
    /// 오브젝트의 풀을 초기화하는 메서드
    /// ObjectPoolManager에 의해 호출됨
    /// </summary>
    /// <param name="pool">이 오브젝트가 속할 오브젝트 풀</param>
    public void InitializePool(IObjectPool<T> pool)
    {
        // 중복 초기화 방지 및 null 체크
        if (!isPoolInitialized && pool != null)
        {
            this.pool = pool;
            isPoolInitialized = true;
        }
        else if (pool == null)
        {
            Logger.ErrorLog($"[PooledObject] {gameObject.name}의 풀 초기화 실패: 풀이 null입니다");
        }
    }

    /// <summary>
    /// 오브젝트를 풀로 반환하는 메서드
    /// 파생 클래스에서 오브젝트를 풀로 반환할 때 사용
    /// </summary>
    protected void ReturnToPool()
    {
        // 풀과 캐시된 컴포넌트 유효성 검사
        if (pool != null && cachedComponent != null)
        {
            pool.Release(cachedComponent);
        }
        else
        {
            string errorReason = pool == null ? "풀이 null" : "컴포넌트 캐싱 실패";
            Logger.WarningLog($"[PooledObject] {gameObject.name}를 풀에 반환 실패: {errorReason}\n" +
                           $"Pool: {(pool != null ? "Valid" : "Null")}, " +
                           $"CachedComponent: {(cachedComponent != null ? "Valid" : "Null")}");
        }
    }

    public void ReleasePool()
    {
        ReturnToPool();
    }

    // IPoolable 인터페이스 구현
    public abstract void OnSpawn();
    public abstract void OnDespawn();
}