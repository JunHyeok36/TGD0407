using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    public class _prototype_ChannelingController : MonoBehaviour
    {
        private _prototype_EntityView _owner;
        private _prototype_IChanneledOperation _currentOperation;
        private int _elapsedTicks;

        public bool IsChanneling => _currentOperation != null && !_currentOperation.IsComplete;
        public _prototype_IChanneledOperation CurrentOperation => _currentOperation;
        public int ElapsedTicks => _elapsedTicks;

        public event Action<_prototype_IChanneledOperation> OnChannelStarted;
        public event Action<_prototype_IChanneledOperation> OnChannelCompleted;
        public event Action<_prototype_IChanneledOperation, string> OnChannelCancelledEvent;

        public void Initialize(_prototype_EntityView owner)
        {
            _owner = owner;
            _currentOperation = null;
            _elapsedTicks = 0;
        }

        public async UniTask StartChanneling(_prototype_IChanneledOperation operation)
        {
            if (operation == null) return;
            if (IsChanneling)
            {
                CancelChanneling("NewOperationStarted");
            }

            _currentOperation = operation;
            _elapsedTicks = 0;
            OnChannelStarted?.Invoke(operation);
            await operation.OnChannelStart(_owner);
        }

        public async UniTask ProcessTick()
        {
            if (!IsChanneling) return;

            _elapsedTicks++;
            await _currentOperation.OnChannelTick(_owner);

            if (_currentOperation != null && (_currentOperation.IsComplete || _elapsedTicks >= _currentOperation.TotalTicks))
            {
                var op = _currentOperation;
                _currentOperation = null;
                await op.OnChannelComplete(_owner);
                OnChannelCompleted?.Invoke(op);
            }
        }

        public void CancelChanneling(string reason = "CC")
        {
            if (!IsChanneling) return;

            var op = _currentOperation;
            _currentOperation = null;
            op.OnChannelCancelled(_owner, reason);
            OnChannelCancelledEvent?.Invoke(op, reason);
        }
    }
}
