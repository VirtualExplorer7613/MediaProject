public static class StrategyFactory
{
    public static IInteractionStrategy Create(ItemDefinition def)
    {
        return def.itemType switch
        {
            ItemType.Ultrasound => new UltrasoundStrategy(def),
            ItemType.Magnet => new MagnetStrategy(def),
            //ItemType.Brush => new BrushStrategy(def),
            _ => new UltrasoundStrategy(def)
        };
    }
}
