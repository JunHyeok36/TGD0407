# 05. 코딩 컨벤션

> 현재 코드베이스에서 유추하여 정리한 규칙이다.  
> 일관성을 위해 새로운 코드 작성 시 아래 규칙을 따른다.

---

## 네임스페이스

| 위치 | 네임스페이스 |
|------|-------------|
| _Game/Scripts/Core/ | TDG0407.Core.* |
| _Game/Scripts/Domain/ | TDG0407.Domain.* |
| _Game/Scripts/Systems/ | TDG0407.Systems.* |
| _Game/Scripts/View/ | TDG0407.View.* |
| _Prototype/ | TDG0407._prototype |

---

## 파일 / 클래스 네이밍

| 구분 | 규칙 | 예시 |
|------|------|------|
| **프로토타입 파일·클래스** | _prototype_ 접두사 | _prototype_LifeData.cs, class _prototype_GridManager |
| **일반 클래스** | PascalCase | RoomData, CardDeck, MapGenerator |
| **인터페이스** | I 접두사 + PascalCase | ICardAction, ITickManager, IDataValidatable |
| **열거형** | PascalCase | RoomType, CardType, PointType |
| **추상 클래스** | PascalCase (접미사 없음) | EntityData, CardAction |
| **ScriptableObject 모델** | ~DataModel 접미사 | CardDataModel, EntityDataModel |
| **Data 클래스** | ~Data 접미사 | RoomData, LifeData, WorldData |
| **문서(Archive)** | ~Document 접미사 | RoomDocument, LifeDocument |
| **컬렉션(Archive)** | ~Collection 접미사 | RoomCollection, EntityCollection |

---

## 멤버 네이밍

| 구분 | 규칙 | 예시 |
|------|------|------|
| **public 필드** | camelCase | entityInstanceId, worldSeed |
| **private 필드** | _ 접두사 + camelCase | _currentMode, _tickDurationType |
| **프로퍼티** | PascalCase | CurrentTick, IsExpired, PlacedEntities |
| **메서드** | PascalCase | AdvanceTick(), FindPath(), CanPlaceEntity() |
| **이벤트** | PascalCase (On~ 또는 이름 직접) | OnPreTick, OnTickPlan |
| **static 프로퍼티** | PascalCase | Instance, CurrentMode |
| **로컬 변수** | camelCase | livingEnemyCount, 
ewMovementCost |
| **파라미터** | camelCase | entityData, startPoint |
| **const** | PascalCase (또는 UPPER_SNAKE) | 프로젝트 내 사례 드물어 자유 |

---

## 코드 구조 (Region 패턴)

클래스 내부는 #region 으로 구분하는 것을 권장한다.

```csharp
#region Fields
// 필드
#endregion
#region Properties
// 프로퍼티
#endregion
#region Constructors
// 생성자
#endregion
#region Methods
// 메서드
#endregion
#region EventHandlers
// 이벤트 핸들러
#endregion
#region Operators
// 연산자 오버로드
#endregion
```

---

## 비동기 패턴

- UniTask 를 사용한다 (sync UniTask / sync UniTaskVoid).
- 반환값이 필요 없는 비동기 메서드는 UniTaskVoid 를 사용하고 .Forget() 으로 호출한다.

```csharp
// 예시
protected override async UniTaskVoid Awake()
{
    base.Awake().Forget();
    InitializeManagers().Forget();
}

private async UniTaskVoid InitializeManagers()
{
    await ArchiveManager.Initialize();
}
```

---

## 싱글턴 패턴

- MonoBehaviour 싱글턴은 Singleton<T> 기반 클래스를 상속한다.
- 정적 시스템 클래스는 static class 로 정의한다.

```csharp
// MonoBehaviour 싱글턴
public sealed class Bootstrapper : Singleton<Bootstrapper> { ... }

// Static 시스템
public static class GameManager { ... }
```

---

## 데이터 객체 불변성 & 복사

- Domain Layer의 Data 객체는 Clone() 메서드를 통해 깊은 복사를 제공한다.
- BoundedValue<T> 도 Clone() 을 통해 복사한다.

```csharp
// 예시
public override EntityData Clone()
{
    return new LifeData(this); // 복사 생성자 사용
}
```

---

## 에디터 전용 코드

Unity 에디터 전용 코드는 #if UNITY_EDITOR 로 감싼다.  
[ContextMenu("...")] 어트리뷰트를 통해 에디터 유틸리티를 제공한다.

```csharp
#if UNITY_EDITOR
[ContextMenu("Get Point Views")]
private void GetPointViews() { ... }
#endif
```

---

## 파일 위치 규칙

| 파일 종류 | 위치 |
|-----------|------|
| 순수 C# 도메인 모델 | _Game/Scripts/Domain/ |
| 시스템·매니저 | _Game/Scripts/Systems/ |
| Unity View (MonoBehaviour) | _Game/Scripts/View/ 또는 _Prototype/View/ |
| ScriptableObject 데이터 모델 | _Game/Data/ 또는 _Prototype/DataModel/ |
| JSON 정적 데이터 | _Game/Data/Stat/ |
| 에디터 스크립트 | _Game/Editor/ 또는 _Prototype/Editor/ |
| 프리팹 | _Game/Prefabs/ 또는 _Prototype/Prefabs/ |
| 리소스 (런타임 로드) | _Game/Resources/ 또는 _Prototype/Resources/ |
