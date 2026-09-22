# 엔티티 명칭 국문 (영문)
- **ID :** `Char_` 또는 `Mob_` 접두사 사용
- **분류 :** [플레이어 / 일반 몬스터 / 보스]
- **외형**
  - [엔티티의 외형이나 외모 특징 서술]
- **콘셉트** 
  - [엔티티의 핵심 기믹이나 특징 서술]

## 1. 기본 스탯 정보
| 카테고리 | 스탯 ID | 항목 | 수치 |
| :--- | :--- | :--- | :--- | 
| **기본** | `health` | 체력 | 0 |
| | `shield` | 보호막 | 0 |
| | `stamina` | 스테미나 | 0 |
| | `staminaRecoverAmount` | 스테미나 회복량 | 0 |
| **힘** | `redPower` | 공격력 | 0 |
| | `bluePower` | 주문력 | 0 |
| | `yellowPower` | ??? | 0 |
| | `ciriticalProb` | 크리티컬 확률 | 0.0% | 
| | `ciriticalWeight` | 크리티컬 배율 | 50.0% |
| **방어** | `redResist` | 물리 방어력 | 0 |
| | `blueResist` | 마법 저항력 | 0 |
| | `yellowResist` | ??? | 0 |
| **저항** | `bleedingResistProb` | 출혈 저항률 | 0.0% | 
| | `burningResistProb` | 화상 저항률 | 0.0% |
| | `poisoningResistProb` | 중독 저항률 | 0.0% |
| | `stunResistProb` | 기절 저항률 | 0.0% |
| | `freezeResistProb` | 동결 저항률 | 0.0% |
| | `silenceResistProb` | 침묵 저항률 | 0.0% |
| | `fearResistProb` | 공포 저항률 | 0.0% |
| | `provocationResistProb` | 도발 저항률 | 0.0% |
| | `airborneResistProb` | 에어본 저항률 | 0.0% |
| | `knockdownResistProb` | 넉다운 저항률 | 0.0% |
| | `deathResistProb` | 죽음 저항률 | 0.0% |
| **덱/행동** | `cardSlotCount` | 핸드 카드 슬롯 | 0 |
| | `drawQuickness` | 카드 쿨타임 감소 | 0 |
| | `speed` | 속도 | 1 |
| | `curseResistProb` | 저주 저항률 | 0.0% |
| | `dodgeProb` | 회피율 | 0.0% | 

## 2. 메커니즘 및 패시브
### 고유 패시브
- **메커니즘 명칭** 
  - [발동 조건 서술]
  - [특정 상태효과(Burning, Freeze 등) 부여/해제, 스탯 증감 등]
### 선택 패시브
- **메커니즘 명칭** 
  - [발동 조건 서술]
  - [특정 상태효과(Burning, Freeze 등) 부여/해제, 스탯 증감 등]
  ### 전용 상태 효과
- **메커니즘 명칭**
  - [발동 조건 서술]
  - [특정 상태효과(Burning, Freeze 등) 부여/해제, 스탯 증감 등]

## 3. 기본 덱
| 등급 | 카드 ID | 카드명 | 종류 | 소모값 | 쿨타임 | Anchor | CastRange | TargetRange | 설명 | 개수 | 비고 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| cardRarity | `cardModelId1` | cardName1 | cardType | 1 SP | coolTick | anchor | cast | target | - action1 | ×1 | |
|  | `cardModelId2` | cardName2 | cardType | 2 SP | coolTick | anchor | cast | target | - action1 | ×2 | |

- 카드 연계 및 활용 방법 등

## 4. 추가 덱 
| 등급 | 카드 ID | 카드명 | 종류 | 소모값 | 쿨타임 | Anchor | CastRange | TargetRange | 설명 | 비고 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| cardRarity | `cardModelId1` | cardName1 | cardType | 1 SP | coolTick | anchor | cast | target | - action1 | |
|  | `cardModelId2` | cardName2 | cardType | 2 SP | coolTick | anchor | cast | target | - action1 | |

- 카드 연계 및 활용 방법 등