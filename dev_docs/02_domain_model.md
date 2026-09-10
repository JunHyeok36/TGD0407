# 02. 도메인 모델

## 계층 구조 개요

```
World (세이브 데이터 최상위)
└── Map (여러 Level의 집합)
    └── Level (여러 Room의 집합)
        └── Room (게임 진행의 최소 단위, 여러 Point의 집합)
            └── Point/PointData (Room을 구성하는 최소 단위, Entity 배치)
                └── Entity (Point 위에 존재하는 단위 오브젝트)
                    ├── Life    (Deck을 가지고 카드로 상호작용)
                    ├── Obstacle (단순/반복 행동)
                    └── Projectile (방향·속도로 이동하는 Entity)

Card (행동 단위 매개체)
└── CardDeck / Deck (여러 Card의 위치를 정의한 집합)
```

---

## 맵 계층

### World (WorldData)
세이브 데이터의 최상위 단위. 하나의 런(Run)에 해당한다.

| 필드 | 타입 | 설명 |
|------|------|------|
| worldSeed | int | 월드 생성 시드 (읽기 전용) |
| proceduralSeed | int | 인게임 랜덤 요소에 사용하는 시드 (읽기 전용) |
| mapDatas | List<MapData> | 맵 데이터 목록 |
| lifeData | LifeData | 플레이어 Life 데이터 |
| playerPosition | RoomPoint | 플레이어의 상세 위치 (방 ID + 로컬 좌표) |

### Map (MapData)
여러 Level의 집합. 던전 전체를 구성한다.

| 필드 | 타입 | 설명 |
|------|------|------|
| instanceId | int? | 고유 번호 (WorldData.mapDatas 인덱스와 동일) |
| levelDatas | List<LevelData> | 생성된 레벨 목록 |

### Level (LevelData)
여러 Room의 집합. 하나의 구역(테마)을 나타낸다.

| 필드 | 타입 | 설명 |
|------|------|------|
| instanceId | int? | 고유 번호 |
| themeId | string | 레벨 테마 ID |
| roomDatas | Dictionary<Point, RoomData> | Point를 키로, RoomData를 값으로 |
| size | Point | 레벨의 크기 |
| startPoint | Point | 시작 Room의 위치 |

주요 메서드:
- TeleportEntity(entityData) — startPoint Room의 (0,0)에 Entity 배치 (가장 가까운 배치 가능 위치 폴백)

### Room (RoomData)
게임 진행의 최소 단위. 여러 PointData의 집합.

| 필드 | 타입 | 설명 |
|------|------|------|
| roomInstanceId | int? | 고유 번호 |
| roomId | string | 모델 데이터 ID |
| levelId | string | 소속 레벨 ID |
| position | Point[] | 레벨 내 위치 목록 (Single=1, Double=2, Quad=4) |
| scale | RoomScale | Single / Double / Quad |
| size | Point | Room 크기 (중앙 기준 ±size/2 영역) |
| roomType | RoomType | Room 유형 |
| pointDatas | Dictionary<Point, PointData> | Point를 키로, PointData를 값 |
| warpPointDatas | Dictionary<Point, WarpPointData> | Room 간 이동 워프 포인트 |

#### Room 유형 (RoomType)
```csharp
// 0xB00 ~ 일반
Empty     // 빈 방 (레벨 시작 방)
Battle    // 일반 전투 방
SoftBattle // 하 난이도 전투 방
HardBattle // 상 난이도 전투 방
Puzzle    // 수수께끼 방

// 0xE00 ~ 이벤트
Shop      // 상점 방

// 0xC00 ~ 시크릿
JustChest // 보물 방

// 0x0FF
Final     // 최종 보스 방
```

주요 메서드:
- IsLinkedWith(other) — 워프 포인트 연결 여부 확인
- AddWarpPoint(position, warpPointData) — 워프 포인트 추가

### Point / PointData
Room을 구성하는 최소 단위. Entity가 배치되는 공간.

| 필드 | 타입 | 설명 |
|------|------|------|
| instanceId | int? | 고유 번호 (생성 순서 0부터 할당) |
| position | Point | 좌표 |
| pointType | PointType | 유형 |
| placedEntities | List<EntityData> | 배치된 Entity 목록 |

#### PointType
| 값 | 설명 |
|----|------|
| Ground (Normal) | 일반 바닥 — Ground, Flying, Ghost 배치 가능 |
| Wall | Room 내부 벽 — Ghost만 이동 가능 |
| Abyss | 낭떠러지 — Flying, Ghost만 이동 가능 |
| Border (OuterWall) | 경계 — 절대 이동 불가 |

주요 메서드:
- IsPlaceable(movementType) — 해당 MovementType으로 배치 가능 여부 반환
- CanPlaceEntity(entityData) — MovementType + PointType + 기존 배치 수를 종합 판단
- PlaceEntity(entityData) — Entity 배치 (이전 Point에서 Remove 후 추가)
- RemoveEntity(entityData) — Entity 제거

### Point (Domain)
정수 2D 좌표 값. struct 타입.

```csharp
struct Point { int x; int y; }
// 사칙연산, 비교 연산자 오버로드 지원
// static Point.zero, Point.one 제공
```

---

## Entity 계층

### Entity (EntityData)
PointData 위에 배치되는 모든 오브젝트의 공통 추상 데이터.

| 필드 | 타입 | 설명 |
|------|------|------|
| entityInstanceId | int? | 고유 번호 |
| entityId | string | 모델 데이터 ID |
| position | Point | 현재 위치 좌표 |
| health | BoundedValue<int> | 체력 (최솟값~최댓값) |
| stamina | BoundedValue<int> | 스테미나 (최솟값~최댓값) |
| shields | Queue<Shield> | 보호막 목록 (FIFO 순으로 소모) |

#### MovementType (배치 유형)
| 값 | 이동 가능 지형 |
|----|----------------|
| Ground | Ground 타입만 |
| Flying | Ground, Abyss |
| Ghost | Ground, Abyss, Wall |

---

### Life (LifeData)
Deck을 가지고 카드를 사용하여 상호작용하는 Entity.

EntityData 의 모든 필드를 포함하며 추가로:

| 필드 | 타입 | 설명 |
|------|------|------|
| stat | Stat | 능력치 |
| deck | CardDeck | 덱 |
| statusEffects | List<StatusEffect> | 지속효과 목록 |

#### Side (세력)
| 값 | 설명 |
|----|------|
| None | 중립 (먼저 공격하지 않음) |
| A | 플레이어 및 플레이어 세력 |
| B | 제 2세력 (적) |
| C | 제 3세력 (적) |

#### Stat 능력치

| 카테고리 | 필드 | 설명 |
|----------|------|------|
| **공격력** | 
edPower | 물리 공격력 |
| | bluePower | 마법 공격력 |
| | yellowPower | 부가 속성 공격력 |
| | whitePower | 고정 공격력 |
| **방어력** | 
edResist | 물리 방어력 — 최종 피해: d × r/(r+100) |
| | blueResist | 마법 방어력 |
| | yellowResist | 부가 속성 방어력 |
| **덱 관련** | cardSlotCount | 핸드 카드 슬롯 수 (기본 4) |
| | drawQuickness | 카드 쿨타임 추가 감소율 |
| | curseResist | 저주 카드 획득 저항값 |
| **이동** | speed | 틱당 행동 횟수 |
| **회복** | healthRecoveryAmount | 대기 시 회복 체력 |
| | staminaRecoveryAmount | 대기 시 회복 스테미나 |
| **기타** | dodgeProb | 회피 확률 |
| | criticalProb | 크리티컬 확률 (기본 5%) |
| | criticalWeight | 크리티컬 배율 (기본 2.0) |
| | deathResist | 사망 저항값 |
| | knockbackResist | 넉백 저항값 |
| **상태효과 저항** | bleedingResist, burningResist, poisoningResist, stunResist, freezeResist, silenceResist, fearResist, knockdownResist | 각 상태효과 저항값 |

> **저항 공식** : 감소율이나 확률로 사용되는 저항값은 v/(v+100) 으로 계산하여 100%에 도달하지 않도록 한다.

#### 상태효과 (StatusEffect)

| 유형 | 설명 |
|------|------|
| Stun | 해당 틱 동안 모든 행동 불가 |
| Knockdown | 스테미나 완전 소모 시 발생. 최초 발동 시 최대 체력 30% 고정 피해 (이 피해로 사망 불가), 기본 5틱간 카드 쿨타임 100% 증가 |
| Silence | 해당 틱 동안 카드 사용 불가 |
| Fear | 해당 틱 동안 특정 Entity를 향해 이동·대상 지정 불가. 적의 경우 반대 방향으로 이동 |
| Curse | 해당 틱 동안 curseResist가 value만큼 감소 |
| Bleeding | 해당 틱 동안 value의 **고정** 피해 |
| Burning | 해당 틱 동안 value의 **물리** 피해. 카드 캐스팅 시 value/(value+200)% 확률로 카드 소실. Freeze와 공존 불가 |
| Freeze | 해당 틱 동안 기본 이동 불가 (카드 이동은 가능). Burning과 공존 불가 |
| Poisoning | 해당 틱 동안 남은틱 × value의 **마법** 피해. 치유 효과 50% 감소 |

#### TickDuration (지속 시간)
상태효과 및 카드의 지속 시간 표현.

| 타입 | 설명 |
|------|------|
| TickBased | 틱 기반 — 틱마다 1 감소, 0이 되면 만료 |
| Forever | 영구 지속 |
| Conditional | 조건에 따라 결정 (이벤트로 직접 해제시켜야 함) |

---
## Entity 종류
### Obstacle (ObstacleData)
- Deck을 가지지 않고 행동이 단순하거나 반복적인 Entity.
### Projectile
- 특정 방향과 속도로 날아가 상호작용하는 Entity.
- 레이저 투사체(LaserProjectileData)가 구현되어 있으며, 방향·속도·관통 여부·충돌 처리 등을 포함한다.

---

## 카드 시스템

### Card
이동을 제외한 모든 행동 단위의 매개체.

| 필드 | 타입 | 설명 |
|------|------|------|
| instanceId | int | 카드 고유 번호 |
| modelId | string | 기반 ModelData ID |
| cardType | CardType | 카드 유형 |
| cardCost | CostValue | 소비 자원 정보 |
| coolTicks | int | 쿨타임 (틱 단위) |
| castRangeSelector | ICastRangeSelector | 시전 범위 선택기 |
| targetRangeSelector | ITargetRangeSelector | 목표 범위 선택기 |
| cardActionList | List<ICardAction> | 실행 행동 목록 |

#### CardType
| 값 | 설명 |
|----|------|
| Normal | 기본 카드 |
| Attack | 공격 카드 |
| Skill | 기술 카드 |
| Curse | 저주 카드 |
| Interaction | 상호작용 카드 (Exploration Mode 전용) |

#### CostType (소비 자원)
| 값 | 설명 |
|----|------|
| None | 무료 |
| FixedHealth | 고정 체력 소모 |
| CurrentHealthRatio | 현재 체력 비율 소모 |
| MaxHealthRatio | 최대 체력 비율 소모 |
| FixedStamina | 고정 스테미나 소모 |
| CurrentStaminaRatio | 현재 스테미나 비율 소모 |
| MaxStaminaRatio | 최대 스테미나 비율 소모 |
| Other | 기타 자원 |

#### 시전/목표 범위 선택기 (Prototype 구현)
- **CastRangeSelector** : 시전 가능 범위
    - **Self** : 시전자의 Point에만 시전 가능
    - **AroundRect** : 시전자의 주변의 N칸만큼의 사각형 영역에 시전 가능
    - **AroundCircle** : 시전자의 주변의 N칸만큼의 원 영역에 시전 가능
    - **CrossLine** : 시전자를 지나는 직선 N칸만큼 시전 가능  
    - **FrontArc** : 시전자 앞 호만큼 시전 가능 (반지름, 각도 설정 가능)
- **TargetRangeSelector** : 목표 지정 범위
    - **Single** : Point 하나 지정
    - **Line** : 길이가 N인 라인 Point 집합 지정
    - **PenetratedLine** : 길이 제한이 없는 라인 Point 집합 지정
    - **RectSplash** : Point 주변 사각형 영역 집합 지정
    - **CircleSplash** : Point 주변 원 영역 집합 지정

#### CardAction 종류 (Prototype 구현)
- DamageCardAction — 피해 적용
- KnockbackCardAction — 넉백
- ApplyStatusEffectCardAction — 상태효과 적용
- MoveToPointCardAction — 위치 이동
- SpawnDirectionalProjectileCardAction — 방향성 투사체 생성
- SpawnTargetedProjectileCardAction — 추적 투사체 생성
- ShootLaserCardAction — 레이저 발사

### CardDeck / Deck
여러 카드의 위치를 정의한 집합.

| 필드 | 설명 |
|------|------|
| allCards | 위치에 관계없는 모든 카드 목록 |
| remainedCards (CardPosition.Remained) | 드로우 대기 카드 목록. 소진 시 discardedCards에서 셔플 후 복귀 |
| handCards (CardPosition.Hand) | 드로우된 카드 목록 (슬롯 수 = cardSlotCount) |
| discardedCards (CardPosition.Discarded) | 사용·버린 카드 목록 |
| destroyedCards (CardPosition.Destoryed) | 이번 Battle에서 파괴되어 사용 불가한 카드 목록 |

> 처음 드로우된 카드는 해당 카드의 coolTicks 동안 사용 불가.  
> 사용하지 않을 카드는 즉시 버려 다음 카드의 드로우 슬롯을 확보할 수 있다.
