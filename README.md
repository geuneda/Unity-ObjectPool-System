# Unity Object Pool System

효율적이고 재사용 가능한 유니티 오브젝트 풀링 시스템입니다. 이 시스템은 Unity의 내장 `ObjectPool<T>`를 기반으로 하며, 게임 오브젝트의 생성/파괴 비용을 최소화하고 메모리 관리를 최적화합니다.

## ✨ 주요 기능

- 💡 Unity의 내장 `IObjectPool<T>` 인터페이스 활용
- 🔄 자동 확장/축소되는 동적 풀 크기 관리
- 🎯 제네릭 기반의 타입 안전성
- 🔒 스레드 세이프한 풀 관리
- 📝 상세한 로깅 시스템
- 🚀 컴포넌트 캐싱을 통한 성능 최적화

## 🛠 시스템 구성

시스템은 세 개의 핵심 클래스로 구성됩니다:

1. **ObjectPoolManager**: 전역 풀 관리자
2. **PooledObject<T>**: 풀링 가능한 오브젝트의 기본 클래스
3. **IPoolable**: 풀링 인터페이스

## 📥 설치 방법

1. 이 저장소를 클론하거나 다운로드합니다.
2. 세 개의 스크립트 파일을 Unity 프로젝트의 적절한 위치에 복사합니다:
   - `ObjectPoolManager.cs`
   - `PooledObject.cs`
   - `IPoolable.cs`

## 💻 사용 방법

### 1. 풀링할 오브젝트 생성

```csharp
// Bullet.cs
public class Bullet : PooledObject<Bullet>
{
    public override void OnSpawn()
    {
        // 총알이 풀에서 꺼내질 때의 초기화 로직
    }

    public override void OnDespawn()
    {
        // 총알이 풀로 돌아갈 때의 정리 로직
    }
}
```

### 2. 풀 매니저 초기화

```csharp
void Start()
{
    // 풀 매니저 초기화
    Managers.Pool.Init();
}
```

### 3. 오브젝트 풀 사용

```csharp
public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    private IObjectPool<Bullet> bulletPool;

    void Start()
    {
        // 풀 가져오기 (초기 용량: 20, 최대: 100)
        bulletPool = Managers.Pool.GetPool(bulletPrefab, 20, 100);
    }

    void SpawnBullet()
    {
        // 풀에서 총알 가져오기
        Bullet bullet = bulletPool.Get();
        // 총알 사용...
    }
}
```

## ⚙️ 설정 옵션

### ObjectPoolManager

- `defaultCapacity`: 풀의 초기 크기 (기본값: 10)
- `maxSize`: 풀의 최대 크기 (기본값: 100)

## 🔍 주요 메서드

### ObjectPoolManager
- `Init()`: 풀 매니저 초기화
- `GetPool<T>()`: 지정된 프리팹의 오브젝트 풀 반환

### PooledObject<T>
- `ReturnToPool()`: 오브젝트를 풀로 반환
- `OnSpawn()`: 풀에서 꺼낼 때 호출
- `OnDespawn()`: 풀로 반환할 때 호출

## 📋 요구사항

- Unity 2021.1 이상
- C# 7.0 이상

## ⚠️ 주의사항

1. 풀링할 오브젝트는 반드시 `PooledObject<T>`를 상속받아야 합니다.
2. `OnSpawn()`과 `OnDespawn()` 메서드를 반드시 구현해야 합니다.
3. 파생 클래스에서 `Awake()`를 오버라이드할 때는 `base.Awake()`를 호출해야 합니다.

## 🔄 성능 최적화

- 컴포넌트 참조 캐싱으로 `GetComponent` 호출 최소화
- 풀 크기의 동적 관리로 메모리 사용 최적화
- 스레드 세이프한 작업 처리

## 📝 라이선스

MIT License

## 🤝 기여하기

1. 이 저장소를 포크합니다
2. 새 브랜치를 생성합니다
3. 변경사항을 커밋합니다
4. 브랜치에 푸시합니다
5. Pull Request를 생성합니다

## 📞 문의

문제가 발생하거나 제안사항이 있으시다면 이슈를 생성해 주세요.
