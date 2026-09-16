using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    /// <summary>
    /// 아이템의 정적 데이터 모델 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDataModel", menuName = "_Prototype/Item/ItemDataModel", order = 0)]
    public class _prototype_ItemDataModel : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea(2, 5)]
        public string description;
        public _prototype_ItemType itemType = _prototype_ItemType.Misc;
        public int maxStack = 99;
        public Sprite icon;

        [Tooltip("인벤토리에서 직접 사용(Use) 가능한 아이템인지 여부")]
        public bool isUsable = false;

        [Tooltip("아이템 사용 시 발동할 엔티티 액션 목록 (체력 회복, 버프 부여 등)")]
        [SerializeReference, SubclassSelector]
        public List<_prototype_EntityAction> actions = new();

        /// <summary>
        /// 아이템의 사용 액션들을 사용자(LifeData)를 대상으로 실행합니다.
        /// </summary>
        public async UniTask<bool> ExecuteUseActions(_prototype_LifeData user)
        {
            if (user == null || actions == null || actions.Count == 0)
                return false;

            var targets = new List<_prototype_EntityData> { user };
            foreach (var action in actions)
            {
                if (action != null)
                {
                    await action.ExecuteAction(user, targets, null);
                }
            }
            return true;
        }
    }
}
