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
        public static _prototype_PlayModeManager Instance { get; private set; }

        private _prototype_PlayMode _currentMode = _prototype_PlayMode.Battle;
        public _prototype_PlayMode CurrentMode => _currentMode;

        public bool IsExploration => _currentMode == _prototype_PlayMode.Exploration;
        public bool IsBattle => _currentMode == _prototype_PlayMode.Battle;

        private IDisposable _entityDiedSub;
        private IDisposable _entityDamagedSub;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        /// <summary>
        /// BootStrapper에서 호출하여 초기화합니다.
        /// 씬에 살아있는 적의 수를 확인하여 초기 모드를 결정합니다.
        /// </summary>
        public void Initialize()
        {
            // 이벤트 구독
            _entityDiedSub = _prototype_EventBus.Listen<EntityDiedEvent>(OnEntityDied);
            _entityDamagedSub = _prototype_EventBus.Listen<EntityDamagedEvent>(OnEntityDamaged);

            // 초기 모드 결정: 살아있는 적이 있으면 전투, 없으면 탐색
            int livingEnemyCount = CountLivingEnemies();
            _currentMode = livingEnemyCount > 0 ? _prototype_PlayMode.Battle : _prototype_PlayMode.Exploration;

            Debug.Log($"[PlayModeManager] 초기화 완료. 살아있는 적: {livingEnemyCount}명. 초기 모드: {_currentMode}");
        }

        private void OnDestroy()
        {
            _entityDiedSub?.Dispose();
            _entityDamagedSub?.Dispose();
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

            // 적이 사망 → 잔여 적 카운트 확인 (약간의 지연 후 체크, 사망 처리가 완료된 후)
            StartCoroutine(CheckTransitionToExploration());
        }

        /// <summary>
        /// 엔티티가 피해를 입었을 때 호출됩니다.
        /// 탐색 모드 중 피해 이벤트가 발생하면(적의 공격) 즉시 전투 모드로 전환합니다.
        /// </summary>
        private void OnEntityDamaged(EntityDamagedEvent evt)
        {
            // 탐색 모드에서만 체크
            if (_currentMode != _prototype_PlayMode.Exploration) return;

            // Source가 적(Side.B)인 경우 → 위협 발생으로 판단
            if (evt.Source is _prototype_LifeData lifeSource && lifeSource.side == _prototype_Side.B)
            {
                Debug.Log($"[PlayModeManager] 탐색 모드 중 적의 공격 감지! 전투 모드로 전환합니다.");
                TransitionTo(_prototype_PlayMode.Battle);
            }
        }

        // ─── 코루틴 ────────────────────────────────────────────────────────────

        private IEnumerator CheckTransitionToExploration()
        {
            // 1프레임 대기 후 체크 (사망 처리 완료 보장)
            yield return null;

            if (_currentMode != _prototype_PlayMode.Battle) yield break;

            int livingEnemies = CountLivingEnemies();
            if (livingEnemies == 0)
            {
                Debug.Log("[PlayModeManager] 모든 적 제거됨. 탐색 모드로 전환합니다.");
                TransitionTo(_prototype_PlayMode.Exploration);
            }
        }

        // ─── 모드 전환 ──────────────────────────────────────────────────────────

        /// <summary>
        /// 지정된 모드로 전환합니다. 이미 해당 모드면 무시합니다.
        /// </summary>
        public void TransitionTo(_prototype_PlayMode newMode)
        {
            if (_currentMode == newMode) return;

            var previous = _currentMode;
            _currentMode = newMode;

            Debug.Log($"[PlayModeManager] 모드 전환: {previous} → {newMode}");

            if (newMode == _prototype_PlayMode.Battle)
                OnEnterBattleMode(previous);
            else if (newMode == _prototype_PlayMode.Exploration)
                OnEnterExplorationMode(previous);

            _prototype_EventBus.Fire(new PlayModeChangedEvent(previous, newMode));
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
            var enemyControllers = FindObjectsByType<_prototype_EnemyAIController>(FindObjectsInactive.Exclude);
            int count = 0;
            foreach (var ctrl in enemyControllers)
            {
                var ev = ctrl.EntityView;
                if (ev == null || ev.EntityData == null) continue;
                if (ev.EntityData.health.Current > 0)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 현재 모드에서 해당 카드를 사용할 수 있는지 여부를 반환합니다.
        /// </summary>
        public bool CanUseCard(_prototype_CardData cardData)
        {
            if (cardData == null) return false;
            if (_currentMode == _prototype_PlayMode.Battle) return true;
            return cardData.IsUsableInExploration;
        }
    }
}
