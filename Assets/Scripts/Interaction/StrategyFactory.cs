public static class StrategyFactory
{
    public static IInteractionStrategy Create(ItemDefinition def)
    {
        return def.itemType switch
        {
            ItemType.Ultrasound => new UltrasoundStrategy(def),
            ItemType.Magnet => new MagnetStrategy(def),
            ItemType.Key => new KeyStrategy(def),
            ItemType.Hammer => new HammerStrategy(def),
            ItemType.Scissor => new ScissorStrategy(def),
            //ItemType.Brush => new BrushStrategy(def),
            _ => new UltrasoundStrategy(def)
        };
    }
}
