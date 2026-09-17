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
- [x] **LifeView**
  - 체력바(HUD), 사망 처리
  - DoT 인스턴스별 개별 데미지 처리 및 0.8배 축소 FloatingText 연속 팝업
  - FloatingText 발생 대상을 오직 `Life` 엔티티로 제한 (Projectile/Obstacle 제외)
  - 넉백(Knockback) 시 바라보는 방향 유지(밀려나는 연출)
- [x] **ObstacleData / ObstacleView**
- [x] **ProjectileData / ProjectileView** — 방향성 투사체, 추적 투사체, 동일 Side 투사체 아군 공격 보호
- [x] **LaserProjectileData / LaserProjectileView** — 레이저 투사체

### 카드 시스템
- [x] **CardData** — 카드 데이터 모델 (비용·쿨타임·범위·행동 목록)
- [x] **followCaster (시전자 추적 옵션)** — 넉백 등으로 시전자가 밀려났을 때 공격 범위 실시간 재계산
- [x] **CardDeck** — 드로우/버리기/소멸 로직 (_prototype_CardDeck)
- [x] **CardAction 구현 목록**
  - DamageCardAction — 물리/마법/고정 피해
  - KnockbackCardAction — 넉백 (경로 검증 포함)
  - ApplyStatusEffectCardAction — 상태효과 적용
  - MoveToPointCardAction — 즉시 이동
  - SpawnDirectionalProjectileCardAction — 방향성 투사체 생성
  - SpawnTargetedProjectileCardAction — 추적 투사체 생성
  - ShootLaserCardAction — 레이저 발사
- [x] **CastRangeSelector** : Self, AroundRect, Cross
- [x] **TargetRangeSelector** : Single, Line, PenetratedLine, CrossSplash, RectSplash, Ring

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
