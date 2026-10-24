namespace KobnexaRoyal
{
    /// <summary>
    /// Which weapon the player is actively using. Y/Triangle toggles between
    /// the Royal Blaster and melee. Pure state, covered by EditMode tests.
    /// </summary>
    public static class WeaponMode
    {
        public enum Kind
        {
            Blaster,
            Melee
        }

        public static Kind Active { get; private set; }

        static WeaponMode()
        {
            Active = Kind.Blaster;
        }

        public static void Toggle()
        {
            Active = Active == Kind.Blaster ? Kind.Melee : Kind.Blaster;
        }

        public static void SetBlaster() { Active = Kind.Blaster; }
        public static void SetMelee() { Active = Kind.Melee; }
        public static bool IsBlaster { get { return Active == Kind.Blaster; } }
        public static bool IsMelee { get { return Active == Kind.Melee; } }
    }
}
