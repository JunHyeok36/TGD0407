namespace TDG0407._prototype
{
    public class _prototype_CardActionParams : _prototype_IActionParams
    {
        public _prototype_CardData CardData { get; }
        public _prototype_EntityData SourceProvider => CardData?.sourceProvider;
        public _prototype_Point TargetedPoint { get; }
        public _prototype_Point Direction { get; }
        public System.Collections.Generic.IReadOnlyList<_prototype_Point> TargetPoints { get; }

        public _prototype_CardActionParams(
            _prototype_CardData cardData, 
            _prototype_Point targetedPoint = default, 
            _prototype_Point direction = default,
            System.Collections.Generic.IReadOnlyList<_prototype_Point> targetPoints = null)
        {
            CardData = cardData;
            TargetedPoint = targetedPoint;
            Direction = direction;
            TargetPoints = targetPoints;
        }
    }
}
