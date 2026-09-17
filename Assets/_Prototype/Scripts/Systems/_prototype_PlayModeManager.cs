using System;
using System.Collections;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 게임 플레이 모드(전투/탐색) 전환을 관리하는 싱글톤 매니저입니다.
    /// - 전투 모드: 씬에 살아있는 적이 존재하는 상태. 모든 카드 사용 가능.
    /// - 탐색 모드: 씬의 모든 적이 사망한 상태. Utility/Communication/None 카드만 사용 가능.
    /// - 탐색 모드 중 적이 공격/피해를 입히는 이벤트가 발생하면 즉시 전투 모드로 복귀합니다.
    /// </summary>
    public class _prototype_PlayModeManager : MonoBehaviour
    {
        private static _prototype_PlayModeManager _instance;
        public static _prototype_PlayModeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<_prototype_PlayModeManager>(FindObjectsInactive.Include);
                }
                return _instance;
            }
        }

        private _prototype_PlayMode _currentMode = _prototype_PlayMode.Battle;
        public _prototype_PlayMode CurrentMode => _currentMode;

        public bool IsExploration => _currentMode == _prototype_PlayMode.Exploration;
        public bool IsBattle => _currentMode == _prototype_PlayMode.Battle;

        private IDisposable _entityDiedSub;
        private IDisposable _entityDamagedSub;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) { Destroy(gameObject); return; }
        }

        /// <summary>
        /// BootStrapper에서 호출하여 초기화합니다.
        /// 씬에 살아있는 적의 수를 확인하여 초기 모드를 결정합니다.
        /// </summary>
        public void Initialize()
        {
            // 기존 구독 해제 후 재구독
            _entityDiedSub?.Dispose();
            _entityDamagedSub?.Dispose();

            _entityDiedSub = _prototype_EventBus.Listen<EntityDiedEvent>(OnEntityDied);
            _entityDamagedSub = _prototype_EventBus.Listen<EntityDamagedEvent>(OnEntityDamaged);

            // 초기 모드 결정: 살아있는 적이 있으면 전투, 없으면 탐색
            int livingEnemyCount = CountLivingEnemies();
            _currentMode = livingEnemyCount > 0 ? _prototype_PlayMode.Battle : _prototype_PlayMode.Exploration;

            Debug.Log($"[PlayModeManager] 초기화 완료. 살아있는 적: {livingEnemyCount}명. 초기 모드: {_currentMode}");

            // UI 표시 초기 갱신
            if (_prototype_PlayerUIView.Instance != null)
            {
                _prototype_PlayerUIView.Instance.UpdateModeIndicatorDirect(_currentMode);
            }
        }

        private void OnDestroy()
        {
            _entityDiedSub?.Dispose();
            _entityDamagedSub?.Dispose();
            if (_instance == this) _instance = null;
        }

        // ─── 이벤트 핸들러 ─────────────────────────────────────────────────────

        /// <summary>
        /// 엔티티가 사망했을 때 호출됩니다.
        /// 남은 적의 수를 확인하여 탐색 모드로 전환할지 결정합니다.
        /// </summary>
        private void OnEntityDied(EntityDiedEvent evt)
        {
            if (evt.Victim == null) return;

            // 플레이어가 사망한 경우 무시 (Game Over 시스템이 처리)
            if (evt.Victim is _prototype_LifeData lifeVictim && lifeVictim.side == _prototype_Side.A)
                return;

            string victimName = evt.Victim is _prototype_LifeData lvData ? lvData.ename : "Enemy";
            Debug.Log($"[PlayModeManager] 적 사망 감지 ({victimName}). 남은 적 체크 시작.");
            // 적이 사망 → 잔여 적 카운트 확인
            StartCoroutine(CheckTransitionToExploration());
        }

        /// <summary>
        /// 엔티티가 피해를 입었을 때 호출됩니다.
        /// 탐색 모드 중 피해 이벤트가 발생하면, 실제로 살아있는 적 Entity가 존재할 때만 전투 모드로 전환합니다.
        /// </summary>
        private void OnEntityDamaged(EntityDamagedEvent evt)
        {
            // 탐색 모드에서만 체크
            if (_currentMode != _prototype_PlayMode.Exploration) return;

            // 씬에 살아있는 적 Entity가 없으면 전투 모드로 전환하지 않음
            if (CountLivingEnemies() <= 0) return;

            // Source가 적(Side.B)이고 아직 살아있는 경우 → 위협 발생으로 판단하여 전투 모드 복귀
            if (evt.Source is _prototype_LifeData lifeSource && lifeSource.side == _prototype_Side.B)
            {
                if (lifeSource.health.Current > 0)
                {
                    Debug.Log($"[PlayModeManager] 탐색 모드 중 적의 공격 감지! 전투 모드로 전환합니다.");
                    TransitionTo(_prototype_PlayMode.Battle);
                }
            }
        }

        // ─── 코루틴 ────────────────────────────────────────────────────────────

        private IEnumerator CheckTransitionToExploration()
        {
            // 1프레임 대기 후 체크 (사망 처리 완료 보장)
            yield return null;

            if (_currentMode != _prototype_PlayMode.Battle) yield break;

            int livingEnemies = CountLivingEnemies();
            Debug.Log($"[PlayModeManager] 잔여 적 카운트: {livingEnemies}");
            if (livingEnemies == 0)
            {
                Debug.Log("[PlayModeManager] 모든 적 제거됨. 탐색 모드로 전환합니다.");
                TransitionTo(_prototype_PlayMode.Exploration);
            }
        }

        // ─── 모드 전환 ──────────────────────────────────────────────────────────

        /// <summary>
        /// 지정된 모드로 전환합니다. 이미 해당 모드면 무시합니다.
        /// 적 Entity가 없는 경우 전투 모드로 전환되지 않습니다.
        /// </summary>
        public void TransitionTo(_prototype_PlayMode newMode)
        {
            if (_currentMode == newMode) return;

            // 적 Entity가 존재할 때만 전투(Battle) 모드로 전환 허용
            if (newMode == _prototype_PlayMode.Battle && CountLivingEnemies() <= 0)
            {
                Debug.Log("[PlayModeManager] 씬에 살아있는 적 Entity가 없으므로 전투 모드로 전환하지 않고 탐색 모드를 유지합니다.");
                return;
            }

            var previous = _currentMode;
            _currentMode = newMode;

            Debug.Log($"[PlayModeManager] 모드 전환: {previous} → {newMode}");

            if (newMode == _prototype_PlayMode.Battle)
                OnEnterBattleMode(previous);
            else if (newMode == _prototype_PlayMode.Exploration)
                OnEnterExplorationMode(previous);

            // 이벤트 발행
            _prototype_EventBus.Fire(new PlayModeChangedEvent(previous, newMode));

            // UI 직접 갱신 보장 (이벤트 버스 리셋 등으로 인한 리스너 유실 방어)
            if (_prototype_PlayerUIView.Instance != null)
            {
                _prototype_PlayerUIView.Instance.UpdateModeIndicatorDirect(newMode);
            }
        }

        private void OnEnterBattleMode(_prototype_PlayMode previous)
        {
            if (previous == _prototype_PlayMode.Exploration)
            {
                _prototype_PlayerUIView.Instance?.ShowWarning("⚔ 전투 모드 진입!", 2.5f);
            }
        }

        private void OnEnterExplorationMode(_prototype_PlayMode previous)
        {
            _prototype_PlayerUIView.Instance?.ShowWarning("✦ 탐색 모드 진입. 적이 없습니다.", 2.5f);
            // 위험 타일은 정리하지 않음 (유저 코멘트 반영)
        }

        // ─── 유틸리티 ───────────────────────────────────────────────────────────

        /// <summary>
        /// 씬에서 살아있는 적 엔티티의 수를 반환합니다.
        /// </summary>
        public int CountLivingEnemies()
        {
            int count = 0;
            var playerView = _prototype_PlayerController.Instance != null ? _prototype_PlayerController.Instance.ControlledEntityView : null;

            var lifeViews = _prototype_GridManager.Instance.GetAllLifeViews();
            foreach (var lv in lifeViews)
            {
                if (lv == null || lv == playerView) continue;
                if (!lv.gameObject.activeInHierarchy) continue;

                if (lv.EntityData is _prototype_LifeData lifeData)
                {
                    // 적 사이드(Side.B)이고 체력이 남아있는 경우만 적 엔티티로 카운트
                    if (lifeData.side == _prototype_Side.B && lifeData.health.Current > 0)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 현재 모드에서 해당 카드를 사용할 수 있는지 여부를 반환합니다.
        /// 배틀 모드에서는 일반 덱 카드(sourceProvider == null)만, 탐색 모드에서는 상호작용 카드(sourceProvider != null)만 사용 가능합니다.
        /// </summary>
        public bool CanUseCard(_prototype_CardData cardData)
        {
            if (cardData == null) return false;
            if (_currentMode == _prototype_PlayMode.Battle)
                return cardData is _prototype_BattleCardData;
            else
                return cardData is _prototype_InteractionCardData;
        }
    }
}
