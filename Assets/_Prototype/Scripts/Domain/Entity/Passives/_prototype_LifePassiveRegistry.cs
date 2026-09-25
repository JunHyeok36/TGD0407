using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    /// <summary>
    /// 패시브 ID → 인스턴스 팩토리 레지스트리.
    /// 각 패시브 클래스의 static 생성자에서 Register()를 호출해 자신을 등록합니다.
    /// </summary>
    public static class _prototype_LifePassiveRegistry
    {
        private static readonly Dictionary<string, Func<_prototype_LifePassiveData>> _factories
            = new Dictionary<string, Func<_prototype_LifePassiveData>>();

        /// <summary>패시브 팩토리를 등록합니다.</summary>
        public static void Register(string id, Func<_prototype_LifePassiveData> factory)
        {
            if (string.IsNullOrEmpty(id)) return;
            _factories[id] = factory;
        }

        /// <summary>ID로 새 패시브 인스턴스를 생성합니다. 등록되지 않은 ID는 null 반환.</summary>
        public static _prototype_LifePassiveData Create(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _factories.TryGetValue(id, out var factory) ? factory() : null;
        }

        /// <summary>등록 여부를 확인합니다.</summary>
        public static bool IsRegistered(string id)
            => !string.IsNullOrEmpty(id) && _factories.ContainsKey(id);

        /// <summary>등록된 모든 패시브 ID 목록을 반환합니다.</summary>
        public static IEnumerable<string> AllIds => _factories.Keys;
    }
}
