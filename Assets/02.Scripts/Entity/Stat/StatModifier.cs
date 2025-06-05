public class StatModifier
{
    public StatType type;
    public int value;
    public string source; // 패시브, 아이템, 버프 등?

    public StatModifier(StatType type, int value, string source)
    {
        this.type = type;
        this.value = value;
        this.source = source;
    }
}