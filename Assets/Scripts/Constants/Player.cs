namespace OneInside.Constants
{
    public static class Player
    {
        public const int MAX_PLAYERS = 12;
        public const int MIN_PLAYERS = 2;

        public const int MAX_IMPOSTERS = 4;
        public const int MIN_IMPOSTERS = 1;
    }

    public static class DefaultPlayerStatsConfig
    {
        public static class Player
        {
            public const float MAX_HEALTH = 3.0f;
            public const float STUN_DURATION = 2.0f;
        }

        public static class Imposter
        {
            public const float ATTACK_COOLDOWN = 1.0f;
            public const float DAMAGE = 1.0f;
        }

    }
}
