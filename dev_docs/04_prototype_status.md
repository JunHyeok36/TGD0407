# 04. 프로토타입 현황

> **목적** : 코어 시스템(틱·전투·카드·AI)을 단일 씬에서 빠르게 검증한다.  
> 프로토타입 코드는 Assets/_Prototype/ 에 위치하며, 모든 파일·클래스에 _prototype_ 접두사를 사용한다.

---

## 구현 완료 항목

### 코어 시스템
- [x] **틱 매니저** (_prototype_TickManagerService)
  - AdvanceTick(playerAction) 으로 틱 전진
  - 플레이어 타겟 Intent / 기타 Intent 분리 처리
  - OnPreTick / OnPostTick 이벤트 훅
  - 플레이어 스턴(Stun) 시 0.8초 주기 자동 틱 진행 (Auto-Tick Progression)
- [x] **이벤트 버스** (_prototype_EventBus)
  - Subscribe / Unsubscribe / Fire 패턴
  - EntityDiedEvent, EntityDamagedEvent, EntityStatusChangedEvent, TickAdvancedEvent, PlayModeChangedEvent 구현

### 플레이 모드
- [x] **PlayModeManager** (_prototype_PlayModeManager)
  - Battle ↔ Exploration 모드 전환
  - 생존 적 수 기반 자동 전환
  - 탐색 모드 중 적 공격 시 즉시 전투 모드로 복귀

### 그리드 & 맵
- [x] **GridManager** (_prototype_GridManager)
  - A\* 경로 탐색 (FindPath) — 4방향/8방향 지원
  - 위험 타일 비용 가중치 적용 (+100)
  - IsWithinBounds, GetPointView, GetNeighbors
- [x] **PointView** — 타일 상태(Normal/Wall/Abyss) 표시
- [x] **GridVisualManager** — 범위·이동 인디케이터 시각화
- [x] **MoveIndicator** — 이동 가능 경로 시각화 (1틱 이동 마커, 잔여 경로 회색/저투명도 표시, 사각형 마커 및 이중 사각 링 연출)

### Entity / Life & 상호작용
- [x] **EntityData** / **LifeData** — 체력, 스테미나, 쉴드, 스탯
- [x] **상태 효과(StatusEffect) 중첩 메커니즘 전면 개편**
  - CC (Stun, Knockdown, Silence, Fear, Freeze) 및 SuperArmor 단일 인스턴스 + `Mathf.Max` 갱신
  - Knockdown 최대 체력 30% 즉발 피해 최초 1회 제한
  - DoT (Bleeding, Burning, Poisoning) 독립 인스턴스 병렬 유지
  - Burning vs Freeze 상호 전소멸(All-Clear)
  - Curse 단일 인스턴스 + 지속시간 갱신 및 `value` 누적 합산
- [x] **죽음의 문턱 (Death's Door) & 죽음 저항 (deathResistProp) 시스템**
  - 다키스트 던전 스타일 사망 유예 및 사망 굴림(Deathblow Check)
  - `deathResistProp` (0.0f ~ 1.0f) 기반 생사 판정 (`Random.value < deathResistProp`) 및 0 HP 유지
  - 직접 타격 및 DoT(출혈, 화상, 중독) 틱 피해 피격 시에도 사망 굴림 적용
  - 최대 5스택 중첩 디버프 및 하한선(최소 저항 10%, 최대 체력/SP 50%, 공격력 40% 등) 보장
  - 기본 최대 체력의 50% 이상 회복 시 문턱 극복 및 스탯 전면 원복
  - 전용 이벤트(`EntityDeathsDoorEnteredEvent`, `EntityDeathResistedEvent`, `EntityDeathsDoorClearedEvent`) 및 상태별 연출/플로팅 텍스트
- [x] **대기(휴식) 시스템 개선 및 일원화 (방안 A)**
  - 대기 시 스태미나만 회복(`Rest()`), 체력 자동 회복 분리(`Heal()`)
  - AI 공통 휴식 루틴(`ExecuteEnemyRest`) 및 최장 쿨타임 카드 버리기(`DiscardHighestCooldownCard`) 캡슐화
- [x] **LifeView**
  - 체력바(HUD), 사망 처리 (사망 애니메이션 후 타일 분리 및 Destroy)
  - DoT 인스턴스별 개별 데미지 처리 및 0.8배 축소 FloatingText 연속 팝업
  - FloatingText 발생 대상을 오직 `Life` 엔티티로 제한 (Projectile/Obstacle 제외)
  - 넉백(Knockback) 시 바라보는 방향 유지(밀려나는 연출)
- [x] **ObstacleData / ObstacleView**
- [x] **ProjectileData / ProjectileView** — 방향성 투사체, 추적 투사체, 동일 Side 투사체 아군 공격 보호
- [x] **LaserProjectileData / LaserProjectileView** — 레이저 투사체

### 카드 시스템
- [x] **CardData 구조 개편 및 다형성 분리**
  - 추상 기본 클래스 `_prototype_CardData`
  - 전투 전용 `_prototype_BattleCardData` (쿨타임, 코스트, 범위, 앵커링, 액션 목록)
  - 탐색 상호작용 전용 `_prototype_InteractionCardData` (상호작용 키, 가시성 조건, 실행 로직)
- [x] **TargetAnchorType (조준 앵커링)**
  - `FollowCaster`: 시전자 피격/넉백 이동 시 공격 범위 실시간 재계산 (공격 회피/헛치기 전술 가능)
  - `FixedGround`: 시전자 이동과 무관하게 바닥 좌표 고정
- [x] **카드 가시성(Visibility) 및 제공자(CardProvider) 시스템**
  - 카드 내부에 `visibilityConditions` 직접 캡슐화 및 `IsVisible(context)` 자체 평가
  - 탐색 모드 시 인접 엔티티의 `_prototype_CardProviderComponentData`를 통한 상호작용 카드 동적 수급
- [x] **CardDeck 순환 & 쿨타임 메커니즘 고도화**
  - 드로우 시 쿨타임 초기화 및 틱당 쿨타임 감소 (`1 + drawQuickness`)
  - 기절(`Stun`) 시 자동 틱 중에도 정상 쿨타임 감소
  - 넘어짐(`Knockdown`) 시 쿨타임 감소 대신 반대로 쿨타임 증가(지연 페널티)
  - 침묵(`Silence`) 시 손패 잠금 오버레이 및 사용 차단
  - 대기(휴식) 시 잔여 쿨타임이 가장 긴 카드를 버리는 `DiscardHighestCooldownCard` 로직
- [x] **EntityAction 체계 (구 CardAction)**
  - DamageEntityAction — 물리/마법/고정 피해 및 크리티컬
  - KnockbackEntityAction — 넉백 (경로 및 벽 충돌 검증)
  - ApplyStatusEffectEntityAction — 상태효과 부여
  - MoveToPointEntityAction — 즉시 이동
  - SpawnDirectionalProjectileEntityAction — 방향성 투사체 생성
  - SpawnTargetedProjectileEntityAction — 추적 투사체 생성
  - AreaEffectAction — 지속 장판/지역 효과 생성
- [x] **CastRangeSelector** : Self, AroundRect, Cross
- [x] **TargetRangeSelector** : Single, Line, PenetratedLine, Arc(호/부채꼴), CrossSplash, RectSplash, Ring

### 샘플 카드 (ScriptableObject)
| 카드 | 설명 |
|------|------|
| BasicAttack | 기본 근접 공격 |
| BloodStrike | 체력 소모 강공격 |
| DashAway | 이동 |
| Earthquake | 범위 물리 피해 |
| HolyNova | 범위 마법 피해 |
| PiercingThrust | 관통 공격 |
| ShootArrow | 화살 발사 |
| ShootLaserCard | 레이저 발사 |
| Snipe | 원거리 단일 공격 |
| SplashAttack | 광역 공격 |
| StatusTestCard | 상태효과 테스트용 |
| ThrowStone | 투석 투사체 |

### AI
- [x] **MeleeChaseAI** — 근접 추격 AI (A\* 이동 + 근접 공격)
- [x] **ArcherAI** — 원거리 AI (사거리 유지 + 화살·돌 발사)
- [x] **StandStillAI** — 정지 AI (더미·테스트용)
- [x] **EnemyAIController** / **EnemyAILogic** — AI 공통 실행 프레임워크
  - 스턴 및 침묵 상태 시 공격 의도(Planned Intent) 및 공격 범위 하이라이터 자동 억제

### UI
- [x] **PlayerUIView** — 핸드 카드, 체력/스테미나, 카드 사용 인터페이스
  - 침묵 상태 시 손패 카드 보라색 오버레이(`SilenceOverlay`) 표시 및 클릭/드래그 차단
  - 경고 메시지(`ShowWarning`) 팝업 연출
  - **카드 설명 동적 수치 및 상세 계수 모드 (Alt/Shift 홀드)**
    - 기본 모드: 시전자(플레이어)의 스탯(공격력, 주문력 등)을 반영한 최종 계산 수치 표기 (예: `물리 피해를 35만큼 줍니다.`)
    - 상세 모드(Alt/Shift): 기본 피해량 및 스탯 반영 비율을 분해한 수식 형태 표기 (예: `물리 피해를 [10 + 공격력 100%]만큼 줍니다.`)
    - 데미지 및 스탯 타입별 Rich Text 색상 강조 (물리/공격력: `#FF6B4A`, 마법/주문력: `#4AA8FF`, 체력: `#4ADE80`)
    - Alt / Shift 키 실시간 감지하여 손패 카드 설명 동적 리프레시
  - **다국어 현지화(Localization) 지원 (Unity Localization)**
    - Unity Localization 패키지 연동 (`Cards_Table`, `Stats_Table`)
    - 전체 21종 카드(전투 14종, 상호작용 7종)에 대한 한국어(`ko`) / 영어(`en`) 템플릿 및 스탯 용어 등록
    - `_prototype_CardDescriptionFormatter` 기반 런타임 다국어 자동 전환 및 내장 카탈로그 사전 Fallback
  - **획득 알림 모달 & 좌측 획득 토스트 UI**
    - 카드/아이템 획득 시 팝업 모달 (`_prototype_RewardNotification.uxml`) 및 좌측 누적 토스트 UI (TYPE A)
- [x] **LifeHUD** — 체력/스테미나/쉴드 바 (월드 스페이스)
  - 상태효과 그룹화 표시 (DoT 중첩 수 `[Bleeding x2 (5t)]`, 저주 누적치 `[Curse -55 (4t)]`, CC 잔여 틱 `[Stun 2]`)
- [x] **DamageText / FloatingText** — 피해량 및 상태이상 팝업 텍스트 (Life 엔티티 전용)

### 기타 & 리팩토링
- [x] **PlayerController** — 입력 처리, 이동, 카드 선택·사용, 스턴 자동 진행 코루틴, 타겟팅 가드
- [x] **CameraController** — 회전, 줌, 플레이어 추적
- [x] **InteractionManager** — 상호작용 및 피해/상태효과 처리
- [x] **PixelPerfectCameraConfig** — 픽셀 퍼펙트 카메라 설정
- [x] **Floating UI 프리팹 책임 분리** — BootStrapper에서 PlayerController/PlayerUIView로 이관

---

## 미구현 / 진행 중 항목

> 이 항목들은 _Game/ 레이어에서 본격 구현 예정이다.

### 맵 시스템
- [ ] Level 간 이동 (워프 포인트 실제 전환)
- [ ] 맵 절차적 생성 (MapGenerator는 _Game에 구현됨, 프로토타입 미적용)
- [ ] Room 유형별 이벤트 처리 (Shop, Puzzle, Boss 등)

### 카드 시스템
- [ ] 유물(Relic) 시스템
- [ ] 카드 획득·강화·삭제 시스템
- [ ] 저주 카드 메커니즘
- [ ] Interaction 카드 (탐색 모드 상호작용)

### 게임 루프
- [ ] 로그라이크 런 흐름 (시작 → 레벨 → 보스 → 종료)
- [ ] 캐릭터 선택 화면
- [ ] 게임 결과 처리 (경험치 획득, 영구 능력치 강화)
- [ ] 게임 저장·불러오기 (WorldData 직렬화 — _Game에 일부 구현)

### UI / 연출
- [ ] 타이틀 화면
- [ ] 인게임 Pause 화면
- [ ] 카드 보상 선택 UI
- [ ] 레벨 전환 연출

### 적·보스
- [ ] 중간 보스 / 최종 보스 설계
- [ ] 다양한 적 AI 패턴

---

## _Game/ 레이어 현황 (실제 빌드 타겟)

_Game/에는 프로토타입과 독립적으로 도메인 모델과 시스템이 구현되어 있다.

| 영역 | 현황 |
|------|------|
| 도메인 모델 (Data 객체) | ✅ 정의 완료 |
| MapGenerator | ✅ 구현 완료 |
| ArchiveManager / Archive 문서 | ✅ 구현 완료 |
| Bootstrapper / GameManager / UserDataManager | ✅ 기본 구현 |
| TickManager, WorldManager, SignalHub | ✅ 기본 구현 |
| View Layer (PointView, RoomView, LevelView) | ✅ 기본 구현 |
| 카드 사용·전투 처리 | ❌ 미구현 |
| 플레이 모드 전환 | ❌ 미구현 |
| 실제 게임 루프 통합 | ❌ 미구현 |
