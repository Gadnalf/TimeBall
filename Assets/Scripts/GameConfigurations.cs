public static class GameConfigurations
{
    public static float baseMovementSpeed = 25f;
    public static float withBallMovementSpeed = 17.5f;
    public static float ballChargingMovementSpeed = 5f;
    public static float rotationSpeed = 400f;

    public static float roundDuration = 10f;
    public static float roundLengthIncrease = 10f;
    public static float maxRoundLength = 30f;
    public static int numberOfRounds = 5;

    public static float ballDistance = 2f;
    public static float ballHeight = 0.5f;
    public static float ballChargeTime = 1f;
    public static int maxBallCharge = 3;
    public static int cloneBaseCharge = 1;
    public static float cloneBaseChargeCDInSeconds = 5;

    public static float horizontalThrowingForce = 1000f;
    public static float verticalThrowingForce = 200f;

    public static float speedBoostFactor = 1.1f;

    public static float dashSpeed = 50f;
    public static int dashingFrame = 10;
    public static int dashCDinFrames = 50;
    public static float dashCDinSeconds = dashCDinFrames / 50;

    public static float goalExplosionSpeed = 50f;
    public static int goalExplosionFrame = 50;

    public static float stunningSpeed = 1f;
    public static int stunningFrame = 50;
}