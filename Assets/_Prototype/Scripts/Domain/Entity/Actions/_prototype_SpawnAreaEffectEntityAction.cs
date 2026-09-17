using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_SpawnAreaEffectEntityAction : _prototype_EntityAction
    {
        public _prototype_AreaEffectDataModel areaEffectModel;
        public GameObject areaEffectPrefab; // 뷰 역할을 할 빈 프리팹 (AreaEffectView 컴포넌트 필수)
        [SerializeReference, SubclassSelector] public _prototype_ITargetRangeSelector areaSelector; // 반경, 십자 등 범위 선택기 (선택 사항: 미지정 시 카드의 타겟 범위 직접 사용)

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (areaEffectModel == null || areaEffectPrefab == null)
            {
                Debug.LogError("AreaEffectModel or Prefab is missing.");
                return;
            }

            var sourceLife = source as _prototype_LifeData;
            _prototype_Side mySide = sourceLife != null ? sourceLife.side : _prototype_Side.None;

            List<_prototype_Point> anchorPoints = new();
            if (@params is _prototype_CardActionParams cardParams)
            {
                anchorPoints.Add(cardParams.TargetedPoint);
            }
            else if (targets != null && targets.Any())
            {
                foreach (var t in targets)
                {
                    if (t != null) anchorPoints.Add(t.point);
                }
            }

            if (anchorPoints.Count == 0)
            {
                anchorPoints.Add(source.point);
            }

            foreach (var anchorPoint in anchorPoints)
            {
                HashSet<_prototype_Point> areaPointsSet = null;

                if (areaSelector != null)
                {
                    var pointsList = areaSelector.GetValidTargetPoints(source.point, anchorPoint);
                    if (pointsList != null && pointsList.Count > 0)
                    {
                        areaPointsSet = new HashSet<_prototype_Point>(pointsList);
                    }
                }
                else if (@params is _prototype_CardActionParams cp && cp.TargetPoints != null && cp.TargetPoints.Count > 0)
                {
                    areaPointsSet = new HashSet<_prototype_Point>(cp.TargetPoints);
                }

                if (areaPointsSet == null || areaPointsSet.Count == 0) continue;

                // 데이터 생성
                var areaData = areaEffectModel.CreateData() as _prototype_AreaEffectData;
                if (areaData == null) continue;

                areaData.caster = source;
                areaData.side = mySide;
                areaData.areaPoints = areaPointsSet;
                areaData.point = anchorPoint;
                
                var centerPointView = _prototype_GridManager.Instance.GetPointView(anchorPoint) 
                                      ?? _prototype_GridManager.Instance.GetPointView(source.point);
                if (centerPointView == null) continue;

                var spawnPos = centerPointView.transform.position;
                var go = GameObject.Instantiate(areaEffectPrefab, spawnPos, Quaternion.identity, centerPointView.transform);
                
                var view = go.GetComponent<_prototype_AreaEffectView>();
                if (view == null) view = go.AddComponent<_prototype_AreaEffectView>();

                view.areaModel = areaEffectModel; // 시각적 조각 생성을 위해 모델 전달
                view.Initialize(areaData, centerPointView); // 여기서 모든 좌표에 등록됨
            }

            await UniTask.Yield();
        }
    }
}
