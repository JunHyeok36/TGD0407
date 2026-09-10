using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    public class _prototype_TickIntent
    {
        public bool TargetsPlayer;
        public Func<UniTask> Execute;
    }
}
