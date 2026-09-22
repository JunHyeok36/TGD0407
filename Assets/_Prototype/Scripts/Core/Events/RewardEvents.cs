namespace TDG0407._prototype
{
    public struct EntityCardAcquiredEvent
    {
        public _prototype_EntityData Entity { get; }
        public _prototype_CardData Card { get; }
        public string SourceName { get; }

        public EntityCardAcquiredEvent(_prototype_EntityData entity, _prototype_CardData card, string sourceName = "")
        {
            Entity = entity;
            Card = card;
            SourceName = sourceName;
        }
    }

    public struct EntityItemAcquiredEvent
    {
        public _prototype_EntityData Entity { get; }
        public _prototype_ItemDataModel Item { get; }
        public int Quantity { get; }
        public string SourceName { get; }

        public EntityItemAcquiredEvent(_prototype_EntityData entity, _prototype_ItemDataModel item, int quantity = 1, string sourceName = "")
        {
            Entity = entity;
            Item = item;
            Quantity = quantity;
            SourceName = sourceName;
        }
    }
}
