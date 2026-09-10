# 03. 시스템 아키텍처

## 디렉터리 구조

```
Assets/
├── _Prototype/          # 코어 시스템 검증용 프로토타입
│   ├── Core/            # 이벤트 버스, 틱 매니저 인터페이스
│   ├── DataModel/       # ScriptableObject 기반 모델 데이터
│   ├── Domain/          # 도메인 로직 (Card, Entity, Map)
│   ├── Editor/          # 에디터 툴
│   ├── Interaction/     # 데미지 컨텍스트, 수정자 인터페이스
│   ├── Prefabs/         # 투사체 프리팹 등
│   ├── Resources/       # 런타임 로드 에셋
│   ├── Systems/         # Bootstrapper, PlayModeManager
│   ├── UI/              # UXML/USS 기반 UI
│   └── View/            # HUD, 인디케이터, 플레이어 UI View
│
└── _Game/               # 실제 게임 빌드 타겟
    ├── Data/            # ScriptableObject·JSON 정적 데이터
    ├── Scripts/
    │   ├── Core/        # 유틸리티 (Grid, Value, Utils)
    │   ├── Domain/      # 도메인 데이터 모델 (순수 C#)
    │   ├── Extensions/  # IEnumerable 등 확장 메서드
    │   ├── Features/    # 기능별 모듈 (미사용)
    │   ├── Systems/     # 게임 시스템 (Data, Generators, Managers, Signals)
    │   └── View/        # Unity MonoBehaviour 뷰 레이어
    └── Scenes/          # 씬 파일
```

---

## 아키텍처 레이어

```
┌─────────────────────────────────────────────────────┐
│                     View Layer                       │
│   MonoBehaviour / UI / Camera / Visual Feedback      │
├─────────────────────────────────────────────────────┤
│                    Systems Layer                     │
│   Bootstrapper · GameManager · WorldManager          │
│   TickManager · MapGenerator · SignalHub             │
├─────────────────────────────────────────────────────┤
│                    Domain Layer                      │
│   Data 객체 (EntityData, RoomData, ...)               │
│   CardDeck · Stat · StatusEffect · TickDuration      │
├─────────────────────────────────────────────────────┤
│                     Core Layer                       │
│   Point · BoundedValue · Singleton · ObjectPooler   │
└─────────────────────────────────────────────────────┘
```

### Core Layer
도메인과 시스템에서 공통으로 사용하는 기반 유틸리티.

| 클래스 | 역할 |
|--------|------|
| Point | 정수 2D 좌표 struct. 사칙연산·비교 연산자 오버로드 |
| LevelPoint | Level 내 좌표 (levelId + Point) |
| RoomPoint | Room 내 좌표 (roomInstanceId + Point) |
| PointArea | Point 집합 영역 표현 |
| BoundedValue<T> | 최솟값·최댓값이 있는 값 타입 (체력·스테미나·쿨타임 등에 사용) |
| RefValue<T> | 참조형 래퍼 |
| Singleton<T> | MonoBehaviour 싱글턴 기반 클래스 |
| ObjectPooler | 오브젝트 풀 관리 |
| SeedParser | 시드 파싱 유틸리티 |

### Domain Layer
순수 C# 데이터(Data) 객체 모음. Unity API에 의존하지 않는다.

| 네임스페이스 | 주요 클래스 |
|-------------|-------------|
| Domain.Map | WorldData, MapData, LevelData, RoomData, PointData, WarpPointData |
| Domain.Entities | EntityData, LifeData, ObstacleData, Shield, Stat |
| Domain.Cards | CardDeck, CardSlot, CardData, CardType, CostType, CardPosition |
| Domain.Effects | StatusEffect |
| Domain | TickDuration, TickDurationType, Coefficients, RangeType |

### Systems Layer
게임 시스템을 조율하는 매니저 클래스들.

#### Orchestrators (총괄 관리자)
| 클래스 | 역할 |
|--------|------|
| Bootstrapper | 앱 시작 시 실행. ArchiveManager 초기화 → 데이터 로드 |
| GameManager | 게임 전반 시스템 관리 (static). UserDataManager.Initialize() 호출 |
| UserDataManager | 유저 데이터(WorldData 등) 관리, 저장·불러오기 |
| SettingsManager | 게임 설정 관리 |

#### Processors (처리자)
| 클래스 | 역할 |
|--------|------|
| WorldManager | 현재 World 상태 관리 및 진행 처리 |
| TickManager | 틱 진행 관리 |
| LevelViewPresenter | 레벨 뷰와 상태를 연결하는 Presenter |

#### Data / Loaders
| 클래스 | 역할 |
|--------|------|
| JsonInitialDataLoader | JSON 파일에서 Entity·Card 초기 데이터 로드 |
| LifeEntityInitialDataLoader | Life Entity 초기 데이터 로드 |
| GameData | 런타임 게임 데이터 컨테이너 |
| SettingData | 설정 데이터 컨테이너 |

#### Generators
| 클래스 | 역할 |
|--------|------|
| MapGenerator | 맵(World/Map/Level/Room/Point) 절차적 생성 |
| RandomLevelGenerateParameter | 레벨 생성 파라미터 |

#### Signals
SignalHub를 통해 시스템 간 이벤트를 전달한다.

| 시그널 | 설명 |
|--------|------|
| TickSignal | 틱 진행 신호 |

#### Archive (정적 데이터 저장소)
Assets/_Game/Data/ 의 ScriptableObject·JSON 데이터를 런타임에 조회하는 저장소.

| 클래스 | 역할 |
|--------|------|
| ArchiveManager | Archive 초기화 총괄 |
| EntityCollection | Entity 데이터 컬렉션 |
| LevelCollection | Level 데이터 컬렉션 |
| RoomCollection | Room 데이터 컬렉션 |
| PointCollection | Point 데이터 컬렉션 |

### View Layer
MonoBehaviour 기반의 Unity 뷰 컴포넌트.

| 클래스 | 역할 |
|--------|------|
| EntityView | Entity의 씬 표현 |
| PointView | Point의 씬 표현 |
| RoomView | Room의 씬 표현 및 포인트 관리 |
| LevelView | Level의 씬 표현 |
| MapPrefabLoader | Map 프리팹 로드 |

---

## 틱 시스템

플레이어의 행동 하나가 틱을 1 전진시키며, 등록된 모든 TickActionPlanner가 순서에 따라 실행된다.

```
플레이어 입력
    │
    ▼
TickManager.AdvanceTick(playerAction)
    │
    ├─ OnPreTick (사전 처리)
    ├─ playerAction() (플레이어 행동)
    ├─ 각 Entity의 TickActionPlanner 수집
    │      ├─ 플레이어 타겟 Intent → 우선 순차 실행
    │      └─ 기타 Intent → 병렬 실행
    └─ OnPostTick (사후 처리)
```

**우선순위 규칙**: 플레이어를 직접 타겟으로 하는 행동이 기타 행동보다 먼저 실행된다.

---

## 이벤트 시스템 (Prototype)

_prototype_EventBus 를 통해 이벤트 기반 통신을 수행한다.

```csharp
// 구독
_prototype_EventBus.Listen<EntityDiedEvent>(OnEntityDied);

// 발행
_prototype_EventBus.Fire(new TickAdvancedEvent(currentTick));
```

주요 이벤트:
| 이벤트 | 설명 |
|--------|------|
| TickAdvancedEvent | 틱 진행 완료 시 발행 |
| EntityDiedEvent | Entity 사망 시 발행 |
| EntityDamagedEvent | Entity 피해 시 발행 |
| PlayModeChangedEvent | 플레이 모드 전환 시 발행 |

---

## 데이터 흐름

```
JSON / ScriptableObject (정적 데이터)
    │ 로드 (ArchiveManager / JsonInitialDataLoader)
    ▼
Domain Data 객체 (WorldData / RoomData / LifeData ...)
    │ 조회·변경 (Systems Layer)
    ▼
View Layer (MonoBehaviour, UI)
    │ 사용자 입력
    ▼
Systems Layer (TickManager, WorldManager ...)
    │ 데이터 갱신
    ▼
Domain Data 객체 (업데이트)
```

---

## 의존성 패키지

| 패키지 | 용도 |
|--------|------|
| **UniTask** (Cysharp) | 비동기 처리 (sync/await 패턴) |
| **DOTween** | 트위닝 애니메이션 |
| **Addressables** | 에셋 비동기 로드 |
| **Unity Input System** | 입력 처리 |
