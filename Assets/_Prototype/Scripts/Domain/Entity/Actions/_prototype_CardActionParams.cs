namespace TDG0407._prototype
{
    public class _prototype_CardActionParams : _prototype_IActionParams
    {
        public _prototype_CardData CardData { get; }
        public _prototype_EntityData SourceProvider => CardData?.sourceProvider;

        public _prototype_CardActionParams(_prototype_CardData cardData)
        {
            CardData = cardData;
        }
    }
}
