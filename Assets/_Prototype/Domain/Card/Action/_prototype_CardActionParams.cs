using System;

namespace TDG0407._prototype
{

    public class _prototype_CardActionParams
    {
        public _prototype_CardData card;
        public _prototype_Point castPoint;
        public _prototype_Point sourcePoint;
        public int recordedTick;

        public _prototype_CardActionParams(
            _prototype_CardData card,
            _prototype_Point sourcePoint,
            _prototype_Point castPoint)
        {
            this.card = card ?? throw new ArgumentNullException(nameof(card));
            this.sourcePoint = sourcePoint;
            this.castPoint = castPoint;
            this.recordedTick = _prototype_TickManager.CurrentTick;
        }

    }

}
