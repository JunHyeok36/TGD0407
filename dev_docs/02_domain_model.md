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

InventoryState (플레이어 소지품)
├── Bag        (슬롯 기반 일반 가방)
│   └── ItemSlot[] → ItemStack (itemId + quantity)
└── KeyItems   (중요 아이템 — 슬롯 제한 없음)
    └── List<ItemStack>
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
| stamina | BoundedValue<int> | 스테미나 (최솟값~최댓값)<br>단순히 주요 카드 소모값이 아닌 엔티티의 강인함을 나타냄. 모두 소모되었을 때 **넉다운** 상태효과 획득(=그로기 상태) |
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
| inventory | InventoryState | 인벤토리 (가방 + 중요 아이템) |

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
| **공격력** | redPower | 물리 공격력 |
| | bluePower | 마법 공격력 |
| | yellowPower | 부가 속성 공격력 |
| | whitePower | 고정 공격력 |
| **방어력** | redResist | 물리 방어력 — 최종 피해: d × r/(r+100) |
| | blueResist | 마법 방어력 |
| | yellowResist | 부가 속성 방어력 |
| **덱 관련** | cardSlotCount | 핸드 카드 슬롯 수 (기본 4) |
| | drawQuickness | 카드 쿨타임 추가 감소율 |
| | curseResist | 저주 카드 획득 저항값 |
| **이동** | speed | 틱당 행동 횟수 |
| **회복** | staminaRecoverAmount | 대기(휴식) 시 회복 스태미나 (방안 A: 체력은 대기로 회복되지 않음) |
| **기타** | dodgeProb | 회피 확률 |
| | criticalProb | 크리티컬 확률 (기본 5%) |
| | criticalWeight | 크리티컬 배율 (기본 2.0) |
| | deathResistProp | 죽음 저항 확률. 0보다 큰 경우 '죽음의 문턱' 메커니즘 활성화 (기본값 0.5) |
| | knockbackResist | 넉백 저항값 |
| **상태효과 저항** | bleedingResist |
| | burningResist |
| | poisoningResist |
| | stunResist |
| | freezeResist | 
| | silenceResist | 
| | fearResist | 
| | knockdownResist | 

> **저항 공식** : 감소율이나 확률로 사용되는 저항값은 v/(v+100) 으로 계산하여 100%에 도달하지 않도록 한다. (단, `deathResistProp` 등 0~1 float 확률 필드는 독립적 직접 확률 사용)

#### 상태효과 (StatusEffect)

| 유형 | 분류 | 설명 | 중첩 및 갱신 규칙 |
|------|------|------|-------------------|
| Stun | CC | 해당 틱 동안 모든 행동 불가 | 단일 인스턴스. 재부여 시 `Mathf.Max(남은틱, 새틱)`으로 갱신 |
| Knockdown | CC | 스테미나 소모 시 발동. 최초 1회 최대 체력 30% 고정 피해(사망 불가), 기본 5틱간 카드 쿨타임 100% 증가 | 단일 인스턴스. 30% 피해는 최초 1회만 적용, 틱은 `Mathf.Max` 갱신 |
| Silence | CC | 해당 틱 동안 카드 사용 불가 | 단일 인스턴스. 재부여 시 `Mathf.Max` 갱신 |
| Fear | CC | 해당 틱 동안 특정 Entity를 향해 이동·대상 지정 불가. 적의 경우 반대 방향으로 이동 | 단일 인스턴스. 재부여 시 `Mathf.Max` 갱신 |
| Freeze | CC | 해당 틱 동안 기본 이동 불가 (카드 이동은 가능). Burning과 상호 배타 | 단일 인스턴스. `Mathf.Max` 갱신. **Burning과 상호 전소멸(All-Clear)** |
| SuperArmor | 버프 | 피격 시 넉백 저항 및 강인도 유지 | 단일 인스턴스. 재부여 시 `Mathf.Max` 갱신 |
| Bleeding | DoT | 해당 틱 동안 value의 **고정** 피해 | **독립 인스턴스(병렬 유지)**. 각 인스턴스별 개별 피해 및 0.8배 FloatingText 팝업 |
| Burning | DoT | 해당 틱 동안 value의 **물리** 피해. 카드 캐스팅 시 확률적 소실. Freeze와 상호 배타 | **독립 인스턴스(병렬 유지)**. **Freeze와 상호 전소멸(All-Clear)** |
| Poisoning | DoT | 해당 틱 동안 남은틱 × value의 **마법** 피해. 치유 효과 50% 감소 | **독립 인스턴스(병렬 유지)**. 각 인스턴스별 개별 피해 처리 |
| Curse | 디버프 | 해당 틱 동안 curseResist가 value만큼 감소 | 단일 인스턴스. 지속시간은 `Mathf.Max`, value는 **누적 합산(`+=`)** |
| DeathsDoor | 특수/디버프 | 체력 0 도달 시 사망 유예 및 디버프 상태. 스택당 사망 저항(-0.08f), 공격력(-12%), 최대 체력/스태미나(-10%), 대기 스태미나 회복량(-15%) 페널티 부여 | 최대 5스택 중첩. 0 HP에서 피격 시 사망 굴림(`Random.value < deathResistProp`) 성공 시 +1스택, 실패 시 최종 사망. 기본 최대 체력의 50% 이상 회복 시 전면 해제 및 스탯 원복 |

##### 상태효과 상세 규칙
1. **군중 제어기(CC) & 버프**:
   - 무한 CC 방지를 위해 1개 인스턴스만 유지하며, 지속시간은 더 큰 값으로 갱신합니다.
2. **지속 피해(DoT)**:
   - 서로 다른 지속시간과 피해량을 온전히 유지하기 위해 독립 인스턴스로 병렬 누적됩니다.
   - 틱 진행 시 각 DoT마다 개별 데미지 틱이 발생하며, 시인성을 위해 기본 FloatingText보다 0.8배 작은 크기로 순차 팝업됩니다.
3. **Burning vs Freeze 상호 전소멸(All-Clear)**:
   - Burning이 부여될 때 대상에게 Freeze가 있으면, 기존 Freeze를 모두 제거하고 신규 Burning도 상쇄 소멸합니다. (Freeze 부여 시 기존 Burning 제거도 동일)
4. **Curse (스탯 디버프)**:
   - 지속시간은 `Mathf.Max`로 갱신되고, 저항 감소 수치(`value`)는 중첩 시 계속 누적 합산됩니다.
5. **죽음의 문턱 (Death's Door) & 사망 굴림 (Deathblow Check)**:
   - **대상**: `lifeStat.deathResistProp > 0f`인 엔티티 (플레이어 및 보스/네임드 몬스터). 일반 몬스터(`deathResistProp == 0f`)는 HP가 0 이하가 되면 즉시 사망합니다.
   - **문턱 진입**: 치명 피해를 받아 체력이 0 이하가 되면 즉시 사망하지 않고 HP가 0으로 고정되며, `DeathsDoor` 1스택이 부여됩니다.
   - **사망 굴림 (Deathblow Check)**: 0 HP 상태에서 추가 피해(직접 타격 및 출혈/화상/중독 등 모든 DoT 틱 피해)를 입을 때마다 유효 사망 저항률(`GetEffectiveDeathResistProp()`)에 기반하여 생사 판정(`Random.value < deathResistProp`)을 수행합니다.
     - **저항 성공**: 사망하지 않고 생존하며 `DeathsDoor` 스택이 +1 증가합니다 (최대 5스택).
     - **저항 실패**: 최종 사망(`Die()`) 처리되어 그리드에서 제거되고 게임오버 연출이 실행됩니다.
   - **5스택 페널티 및 하한선(Floor) 보장**:
     - 죽음 저항 확률: 스택당 `-0.08f` (-8%p) 차감 (5스택 시 **최소 0.10f / 10% 하한선** 보장).
     - 공격력 (`RedPower`, `BluePower`): 스택당 `-12%` 차감 (5스택 시 -60%, 최소 25% 비율 및 최소 1 보장).
     - 최대 체력 / 최대 스태미나: 스택당 `-10%` 차감 (5스택 시 -50%, **기본 최대치의 50% 하한선** 보장).
     - 대기 시 스태미나 회복량: 스택당 `-15%` 차감 (최소 1 회복 보장).
   - **문턱 극복 및 해제 조건**:
     - HP를 $\ge 1$ 이상 회복하면 즉시 사망 굴림 대상에서 제외됩니다.
     - 체력이 **기본 최대 체력(`BaseMaxHealth`)의 50% 이상**으로 회복되면 `DeathsDoor` 상태효과가 완전히 제거되고 깎였던 모든 기본 스탯이 원래 수치로 복구됩니다.

#### TickDuration (지속 시간)
상태효과 및 카드의 지속 시간 표현.

| 타입 | 설명 |
|------|------|
| TickBased | 틱 기반 — 틱마다 1 감소, 0이 되면 만료 |
| Forever | 영구 지속 |
| Conditional | 조건에 따라 결정 (이벤트로 직접 해제시켜야 함) |

---
## Entity 종류 및 상호작용 규칙

### Obstacle (ObstacleData)
- Deck을 가지지 않고 행동이 단순하거나 반복적인 Entity.

### Projectile
- 특정 방향과 속도로 날아가 상호작용하는 Entity.
- 레이저 투사체(LaserProjectileData)가 구현되어 있으며, 방향·속도·관통 여부·충돌 처리 등을 포함한다.

### Entity 상호작용 & 연출 공통 규칙
1. **FloatingText 발생 대상 제한**:
   - 데미지 수치 및 상태효과 텍스트 등의 FloatingText는 오직 **`Life` 엔티티**에서만 발생한다.
   - `Projectile`이나 `Obstacle`은 피격되거나 파괴되더라도 일반 FloatingText를 띄우지 않는다.
2. **동일 세력(Side) 투사체 보호**:
   - 공격 범위 내에 아군(동일 Side)이 생성한 Projectile이 포함되어도 이를 공격하거나 파괴하지 않는다. 적대적 세력의 투사체나 대상만 피격 판정 대상에 포함된다.
3. **넉백(Knockback) 시 시선 방향 유지**:
   - 넉백 효과를 받아 강제로 밀려날 때에는 스스로 이동하는 느낌을 방지하고 "밀려나는" 연출을 위해 **기존에 바라보던 시선 방향(Facing Direction)을 강제로 유지**시킨 상태에서 좌표를 이동한다.

---

## 카드 시스템

### Card 구조 개요
이동을 제외한 모든 행동 및 상호작용 단위의 매개체.  
기본 모델인 `CardData`를 추상화하고, 전투에서 쿨타임과 자원을 소모하는 **`BattleCardData`** 와 탐색 모드에서 엔티티와 상호작용하는 **`InteractionCardData`** 로 역할을 명확히 분리합니다.

```
_prototype_CardData (추상 기본 클래스)
├── id, description, sourceProvider (제공 엔티티 참조)
├── IsVisible(ConditionContext) (가시성 조건 평가)
│
├── _prototype_BattleCardData (전투 카드)
│   ├── cardType, costValue, coolTicks, currentCoolTicks
│   ├── castRange, targetRange
│   ├── targetAnchorType (FollowCaster / FixedGround)
│   └── actionList (List<_prototype_EntityAction>)
│
└── _prototype_InteractionCardData (상호작용 카드)
    ├── interactionKey (상호작용 식별자)
    ├── visibilityConditions (List<_prototype_Condition>)
    └── ExecuteInteraction(caster) (상자/상인/포탈 등 실행)
```

---

### CardData (추상 기본 클래스)
| 필드/메서드 | 타입 | 설명 |
|-------------|------|------|
| `id` | `string` | 카드 식별자 |
| `descriptionLocalizationKey` | `string` | Unity Localization 문자열 테이블(`Cards_Table`) 참조 키 |
| `description` | `string` | 카드 설명 텍스트 (로컬라이제이션 미적용 시 Fallback 또는 기본 템플릿) |
| `sourceProvider` | `_prototype_EntityData` | 카드를 제공한 주변 엔티티 (상호작용 카드 등에서 사용, 런타임 참조) |
| `IsVisible(context)` | `bool` | 현재 컨텍스트(플레이 모드, 거리, 소지 아이템 등)에서 노출 가능한지 여부 반환 |
| `Clone()` | `_prototype_CardData` | 런타임 덱 인스턴스 복제용 추상 메서드 |

#### 카드 설명 포매터 및 다국어 지원 (`_prototype_CardDescriptionFormatter`)
- **다국어(Localization) 지원**: Unity Localization의 `Cards_Table` 및 `Stats_Table`과 연동되어 현재 언어(`ko`, `en`)에 맞게 동적 텍스트 제공.
- **수치 동적 계산 모드 (Default)**: 카드 액션의 기본 수치와 시전자(Life)의 스탯(공격력, 주문력 등)을 곱연산하여 최종 적용 데미지로 포매팅 (`{damage}` 토큰 치환).
- **상세 계수 모드 (Alt/Shift 홀드)**: 계산 전 기본 피해량과 스탯 계수 분해 수식(`[10 + 공격력 100%]`)으로 변환 표기.
- **Rich Text 색상 강조**: 물리/공격력(`#FF6B4A`), 마법/주문력(`#4AA8FF`), 체력/회복(`#4ADE80`) 등으로 자동 하이라이트.

---

### BattleCardData (전투 카드)
전투 모드에서 쿨타임 및 코스트를 소모하여 행동(`EntityAction`)을 실행하는 카드.

| 필드 | 타입 | 설명 |
|------|------|------|
| `cardType` | `_prototype_CardType` | 카드 유형 (Attack, Skill, Curse, Dash 등) |
| `costValue` | `_prototype_CostValue` | 소비 자원 정보 (FixedHealth, FixedStamina 등) |
| `coolTicks` | `BoundedValue<byte>` | 기본 쿨타임 범위 (Min=0, Max=기본 쿨타임) |
| `currentCoolTicks` | `int` | 현재 남은 쿨타임 (0이 되어야 사용 가능) |
| `castRange` | `_prototype_ICastRangeSelector` | 시전 가능 범위 선택기 |
| `targetRange` | `_prototype_ITargetRangeSelector` | 목표 지정 범위 선택기 |
| `targetAnchorType` | `_prototype_TargetAnchorType` | 조준점 앵커링 방식 (`FollowCaster` / `FixedGround`) |
| `actionList` | `List<_prototype_EntityAction>` | 실행 행동 목록 (피해, 넉백, 상태이상, 투사체 등) |

#### TargetAnchorType (조준 앵커링)
- **FollowCaster (기본값)**:
  - 시전자가 밀려나거나(넉백) 이동하면 이동 변위만큼 조준점과 공격 범위가 함께 이동하여 새 위치에서 실시간 재계산됩니다.
  - *전술적 활용*: 적이 공격을 예고(Planned Intent)한 상태에서 플레이어가 넉백/밀치기 카드로 적을 밀쳐내면 공격 목표 범위가 이동하여 공격을 회피(Whiff)시킬 수 있습니다.
- **FixedGround**:
  - 시전자가 이동하더라도 최초 조준된 바닥 좌표(월드 좌표)가 고정된 채 공격 범위가 계산됩니다 (바닥 설치기, 지점 폭격, 메테오 등).

---

### InteractionCardData (상호작용 카드)
탐색 모드(Exploration Mode)에서 주변 오브젝트(상자, NPC, 상인, 포탈 등)의 `CardProviderComponentData`를 통해 플레이어에게 제공되는 카드.

| 필드/메서드 | 타입 | 설명 |
|-------------|------|------|
| `interactionKey` | `string` | 상호작용 식별 키 (예: "OpenChest", "Talk", "Warp") |
| `visibilityConditions` | `List<_prototype_Condition>` | 카드 자체에 캡슐화된 노출 조건 목록 |
| `IsVisible(context)` | `bool` | 모든 조건(`PlayModeCondition`, `DistanceCondition`, `RequiredItemCondition` 등)을 평가하여 만족할 때만 손패에 노출 |
| `ExecuteInteraction(caster)` | `UniTask` | 상호작용 로직 실행 |

#### 카드 제공자 (CardProviderComponentData) 및 가시성 평가
- 탐색 모드 진행 시 플레이어 주변(반경 1칸 이내)의 엔티티를 검색하여 `CardProviderComponentData`가 제공하는 카드 목록을 취합합니다.
- 외부 래퍼 구조 대신 카드 자체가 `visibilityConditions`를 캡슐화하여 보유하고, `IsVisible(targetContext)`를 스스로 평가하여 UI에 표기됩니다.

---

### CardDeck (덱 및 순환 관리)
전투 중 덱의 5개 영역(All, Remained, Hand, Discarded, Destroyed)을 관리하고 쿨타임 흐름을 제어합니다.

| 필드 | 설명 |
|------|------|
| `allCardDatas` | 위치에 관계없는 모든 카드 목록 |
| `remainedCardDatas` | 드로우 대기 카드 목록. 소진 시 `discardedCardDatas`를 셔플 후 복귀 |
| `handedCardDatas` | 손패 카드 목록 (슬롯 수 = `lifeStat.handCardSlotCount`) |
| `discardedCardDatas` | 사용 및 버린 카드 목록 |
| `destroyedCardDatas` | 이번 전투에서 파괴/소멸된 카드 목록 |

#### 쿨타임(CoolTicks) 및 순환 상세 규칙
1. **드로우 시 쿨타임 초기화**:
   - 덱에서 카드가 손패(`handedCardDatas`)로 드로우될 때, 해당 카드의 `currentCoolTicks`는 기본 쿨타임(`coolTicks.Current`)으로 세팅되어 즉시 사용할 수 없습니다.
2. **틱당 쿨타임 감소**:
   - 매 틱마다 손패의 모든 전투 카드는 `currentCoolTicks`가 `1 + lifeStat.drawQuickness`만큼 감소하며, `0`에 도달하면 카드가 활성화되어 사용 가능합니다.
3. **상태효과와 쿨타임의 상호작용**:
   - **기절 (Stun)**: 플레이어가 스턴으로 인해 자동 틱이 진행될 때에도 쿨타임은 정상적으로 매 틱 감소합니다.
   - **넘어짐 (Knockdown)**: 넘어짐 상태에서는 쿨타임이 감소하지 않고, 반대로 `currentCoolTicks`가 `1 + drawQuickness`만큼 **증가**하여 카드 사용이 지연되는 페널티를 받습니다.
   - **침묵 (Silence)**: 손패 카드가 잠겨 비활성화 보라색 오버레이(`SilenceOverlay`)가 표시되며 클릭 및 드래그 사용이 완전히 차단됩니다. (쿨타임 감소는 정상 진행)
4. **최장 쿨타임 카드 버리기 (`DiscardHighestCooldownCard`)**:
   - 플레이어나 적 AI가 대기(휴식) 행동을 수행할 때 호출됩니다.
   - 손패 중 잔여 쿨타임(`currentCoolTicks`)이 가장 길어 장기간 사용하기 어려운 카드를 자동으로 선별하여 버린 카드 더미(`discardedCardDatas`)로 이동시킵니다.
   - 이를 통해 다음 드로우 슬롯을 확보하고 덱 순환을 유도합니다.

---

### 범위 선택기 (RangeSelector)
- **CastRangeSelector (시전 가능 범위)**:
  - `Self`: 시전자 위치에만 시전
  - `AroundRect`: 시전자 주변 N칸 사각형 영역
  - `Cross`: 시전자를 지나는 십자 방향 직선 N칸
- **TargetRangeSelector (목표 적용 범위)**:
  - `Single`: 단일 좌표 1칸
  - `Line` / `PenetratedLine`: 직선 범위 / 관통 직선 범위
  - `Arc`: 전방 호(부채꼴) 영역 (반지름 및 각도 지정)
  - `RectSplash` / `CrossSplash` / `Ring`: 사각형 폭발 / 십자 폭발 / 도넛형 고리 범위

---

### EntityAction (행동 실행기)
카드가 발동될 때 실행되는 세부 액션 단위로, 카드의 전유물이 아닌 엔티티·환경·트랩 공통 액션 체계로 사용됩니다.
- `DamageEntityAction`: 물리/마법/고정 피해 적용 및 크리티컬 판정
- `KnockbackEntityAction`: 타겟 밀치기 (경로 및 벽 충돌 검증)
- `ApplyStatusEffectEntityAction`: 상태효과(CC, DoT, 디버프 등) 부여
- `MoveToPointEntityAction`: 좌표 이동
- `SpawnDirectionalProjectileEntityAction`: 방향성 투사체 생성
- `SpawnTargetedProjectileEntityAction`: 추적 투사체 생성
- `AreaEffectAction`: 지속 장판 및 범위 효과 생성

---

## 인벤토리 시스템

### 구조 개요

InventoryState는 LifeState.inventory에 포함되어 세이브 데이터의 일부로 저장된다.

```
InventoryState
├── Bag               <- 일반 인벤토리 (슬롯 한계 있음)
│   └── ItemSlot[]
│       └── ItemStack (itemId, quantity)
└── KeyItems          <- 중요 아이템 (한계 없음)
    └── List<ItemStack>
```

---

### ItemStack
아이템 한 묶음 (ID + 수량). 모든 아이템 단위의 최소 표현.

| 필드 | 타입 | 설명 |
|------|------|------|
| `itemId` | `string` | 아이템 ID (Archive ItemDocument.id와 매칭) |
| `quantity` | `int` | 수량 |

---

### ItemSlot
가방의 슬롯 하나. 비어 있거나 ItemStack 하나를 보관한다.

| 필드/프로퍼티 | 설명 |
|---------------|------|
| `slotIndex` | 슬롯 인덱스 (UI 슬롯과 1:1 매핑) |
| `Item` | 현재 보관 중인 ItemStack (없으면 null) |
| `IsEmpty` | 슬롯이 비어 있는지 여부 |

주요 메서드:
- `Set(item)` - 슬롯에 아이템 설정 (null 전달 시 비움)
- `Pop()` - 슬롯을 비우고 아이템 반환

---

### Bag (일반 가방)
슬롯 배열로 구성된 일반 인벤토리.

| 프로퍼티 | 설명 |
|----------|------|
| `SlotCount` | 전체 슬롯 수 |
| `Slots` | 슬롯 목록 (읽기 전용) |
| `UsedSlotCount` | 사용 중인 슬롯 수 |
| `IsFull` | 가방이 꽉 찬 상태인지 |

주요 메서드:
- `Add(stack, maxStack)` - 아이템 추가. 같은 ID 슬롯에 먼저 스택, 없으면 빈 슬롯에 배치. 실제 추가된 수량 반환
- `RemoveAt(slotIndex, quantity)` - 특정 슬롯에서 수량만큼 제거
- `Swap(slotA, slotB)` - 슬롯 위치 교환 (UI 드래그 지원)
- `CountOf(itemId)` - 특정 아이템의 가방 내 총 수량 반환

> maxStack은 ItemDocument(Archive)에서 조회하여 호출 시 전달한다. Domain 객체는 직접 Archive를 참조하지 않는다.

---

### InventoryState (인벤토리 통합 상태)

| 필드 | 타입 | 설명 |
|------|------|------|
| `bag` | `Bag` | 슬롯 기반 일반 가방 |
| `KeyItems` | `IReadOnlyList<ItemStack>` | 중요 아이템 목록 (읽기 전용 노출) |

주요 메서드 (중요 아이템):
- `AddKeyItem(stack)` - 중요 아이템 추가. 같은 ID면 수량 합산
- `RemoveKeyItem(itemId, quantity)` - 중요 아이템 제거. 실제 제거된 수량 반환
- `HasKeyItem(itemId, quantity)` - 보유 여부 확인

---

### ItemType (아이템 유형)
| 값 | 설명 |
|----|------|
| `Consumable` | 소모품 (포션 등) |
| `Material` | 조합 재료 |
| `KeyItem` | 중요 아이템 (열쇠, 퀘스트 아이템) |
| `Misc` | 기타 |

---

### ItemDocument (Archive 정적 데이터)
아이템의 정적 데이터를 ScriptableObject로 관리한다.

| 필드 | 타입 | 설명 |
|------|------|------|
| `id` | `string` | 아이템 고유 ID |
| `displayName` | `string` | 표시 이름 |
| `description` | `string` | 설명 |
| `itemType` | `ItemType` | 아이템 유형 |
| `maxStack` | `int` | 슬롯당 최대 스택 수 (KeyItem은 99 등 충분히 큰 값) |
| `icon` | `Sprite` | 아이콘 |

---

### 신규 파일 목록

#### 1) _Prototype 구현 (완료)
```
Assets/_Prototype/Scripts/Domain/Inventory/
├── _prototype_ItemType.cs                  <- 아이템 유형 열거형 (Consumable, Material, KeyItem, Misc)
├── _prototype_ItemStack.cs                 <- 아이템 단위 (ID + 수량)
├── _prototype_ItemSlot.cs                  <- 가방 슬롯
├── _prototype_Bag.cs                       <- 슬롯 기반 일반 가방
├── _prototype_IInventoryData.cs            <- 인벤토리 공통 인터페이스
├── _prototype_PlayerInventoryData.cs       <- 플레이어 풀 인벤토리 (Bag + KeyItems + UseItem)
├── _prototype_LootInventoryData.cs         <- 몬스터 경량 전리품 인벤토리 (1~2칸 전리품 전용, 메모리 최적화)
├── _prototype_InventoryDataModel.cs        <- 인벤토리 모델 추상 SO 기본 클래스
├── _prototype_PlayerInventoryDataModel.cs  <- 플레이어 인벤토리 SO 모델
├── _prototype_LootInventoryDataModel.cs    <- 몬스터 전리품 인벤토리 SO 모델 (드랍 확률 지원)
└── _prototype_EntityLootDroppedEvent.cs    <- 사망 시 전리품 드랍 도메인 이벤트

Assets/_Prototype/Scripts/Domain/Item/
└── _prototype_ItemDataModel.cs             <- 아이템 SO 모델 (EntityAction 사용 효과 지원)

Assets/_Prototype/Scripts/Domain/Interaction/
└── _prototype_HasItemCondition.cs          <- 아이템 보유/소모 상호작용 조건

Assets/_Prototype/Scripts/Domain/Entity/Actions/
└── _prototype_HealEntityAction.cs          <- 포션 등 회복용 엔티티 액션
```

#### 2) _Game 정식 도메인 (예정)
```
Assets/_Game/Scripts/Domain/Inventory/
├── ItemType.cs         <- 아이템 유형 열거형
├── ItemStack.cs        <- 아이템 단위 (ID + 수량)
├── ItemSlot.cs         <- 가방 슬롯
├── Bag.cs              <- 슬롯 기반 일반 가방
└── InventoryState.cs   <- Bag + KeyItems 통합 상태

Assets/_Game/Scripts/Domain/Archive/
└── ItemDocument.cs     <- 아이템 정적 데이터 (ScriptableObject)
```

