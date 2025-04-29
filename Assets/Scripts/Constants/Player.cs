namespace OneInside.Constants
{
    public static class Player
    {
        public const int MAX_PLAYERS = 12;
        public const int MIN_PLAYERS = 2;

        public const int MAX_IMPOSTERS = 4;
        public const int MIN_IMPOSTERS = 1;
    }

    public static class MovementBehaviour
    {
        public const string DEFAULT = "Default";
        public const string STUNNING = "Stunning";
        public const string FIXED = "Fixed";
    }

    public static class DefaultPlayerConfig
    {
        public static class Player
        {
            public const float MAX_HEALTH = 1.0f;

            public const float POKE_COOLDOWN = 2.0f;
            public const float POKE_STUN_DURATION = 1.0f;

            public const float STUN_DURATION = 2.0f;
        }

        // public static class Crewmate
        // {

        // }

        public static class Imposter
        {
            public const float ATTACK_COOLDOWN = 2.0f;
            public const float ATTACK_STUN_DURATION = 1.5f;

            public const float DAMAGE = 1.0f;

            public const float TRANSFORMATION_ACTIVE_TIME = 10.0f;
            public const float TRANSFORMATION_TIME = 0.5f;
            public const float TRANSFORMATION_COOLDOWN_TIME = 20.0f;
        }


        public static class Movement
        {
            public const float STUN_WALK_SPEED = 3.0f;

            public const float WALK_SPEED = 6f;
            public const float RUN_SPEED = 12f;
            public const float ROTATION_SPEED = 15f;
            public const float TURN_SMOOTH_TIME = .1f;
            public const float JUMP_HEIGHT = 6f;

            public const float GRAVITY = 1f;
            public const float GROUNDED_GRAVITY = -0.5f;
            public const float GROUND_CHECK_DISTANCE = 0.1f;

            public const float MAX_DOWN_SPEED = -53f;
        }

    }
}
