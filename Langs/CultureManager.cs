namespace WeaponFusion.Langs
{
    public static class CultureManager
    {
        public const string Item_LevelMax = "'{0}' is at max level [{1}]";

        public static string Format(this string value, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                value = value.Replace($"{{{i}}}", args[i].ToString());
            }
            return value;
        }
    }
}
