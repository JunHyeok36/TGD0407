using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 게임 내 모든 핵심 매니저 및 시스템의 초기화 순서를 총괄하는 부트스트래퍼입니다.
    /// 개별 매니저들의 Awake/Start 의존을 최소화하고 결정론적 순서로 시스템을 초기화합니다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class _prototype_BootStrapper : MonoBehaviour
    {
        private void Awake()
        {
            InitializeGame();
        }

        public void InitializeGame()
        {
            Debug.Log("[BootStrapper] 게임 시스템 초기화 시작...");

            // 1. 이벤트 버스 초기화 및 글로벌 리스너 등록
            _prototype_EventBus.Reset();
            _prototype_EventBus.Listen<EntityDamagedEvent>(_prototype_FloatingText.OnEntityDamaged);
            _prototype_EventBus.Listen<EntityStatusChangedEvent>(_prototype_FloatingText.OnEntityStatusChanged);

            // 2. 턴 및 틱 매니저 초기화
            _prototype_TickManager.Initialize();

            // 3. 그리드 매니저 초기화 (포인트 맵 구성 및 배치된 엔티티 뷰/컨트롤러 초기화)
            if (_prototype_GridManager.Instance != null)
            {
                _prototype_GridManager.Instance.Initialize();
            }

            // 4. 그리드 시각화 매니저 초기화 (인디케이터 및 이동 경로 렌더러 구성)
            if (_prototype_GridVisualManager.Instance != null)
            {
                _prototype_GridVisualManager.Instance.Initialize();
            }

            // 5. 카메라 컨트롤러 초기화 (초기 위치 및 회전각 설정)
            if (_prototype_CameraController.Instance != null)
            {
                _prototype_CameraController.Instance.Initialize();
            }

            // 6. 플레이 모드 매니저 초기화 (전투/탐색 모드 판단 및 이벤트 리스너 등록)
            if (_prototype_PlayModeManager.Instance != null)
            {
                _prototype_PlayModeManager.Instance.Initialize();
            }

            // 7. 적 AI 초기 의도(Intent) 계산 및 위험 타일 시각화
            if (_prototype_GridManager.Instance != null)
            {
                foreach (var life in _prototype_GridManager.Instance.GetAllLifeViews())
                {
                    if (life.TryGetComponent<_prototype_EnemyAIController>(out var enemyAI))
                    {
                        enemyAI.EvaluateInitialIntent();
                    }
                }
            }

            // 8. 플레이어 컨트롤러 초기화 (틱 리스너 등록 및 초기 덱 드로우)
            if (_prototype_PlayerController.Instance != null)
            {
                _prototype_PlayerController.Instance.Initialize();
            }

            // 9. 플레이어 UI 뷰 초기화 (UI 바인딩, 모드 리스너 등록, UI 갱신)
            if (_prototype_PlayerUIView.Instance != null)
            {
                _prototype_PlayerUIView.Instance.Initialize();
            }

            Debug.Log("[BootStrapper] 모든 시스템 초기화 완료.");
        }

        private void OnDestroy()
        {
            _prototype_EventBus.Reset();
        }
    }
}