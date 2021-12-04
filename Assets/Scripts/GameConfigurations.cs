using UnityEngine;
public static class GameConfigurations
{
    public static float baseMovementSpeed = 20f;
    public static float withBallMovementSpeed = 12.5f;
    public static float guardMovementSpeed = 5f;
    public static float rotationSpeed = 400f;
    public static float passAngle = 90f;

    public static float roundDuration = 10f;
    public static float roundLengthIncrease = 0f;
    public static float maxRoundLength = 25f;
    public static int numberOfRounds = 5;

    public static float ballDistance = 2f;
    public static float ballHeight = 1.3f;
    public static float ballChargeTime = 1.5f;
    public static int goalShieldBreakableCharge = 1;
    //public static int maxCloneAutoCharge = 3;
    public static int maxBallCharge = 5;
    public static int cloneBaseCharge = 1;
    public static float cloneBaseChargeCDInSeconds = 10;

    public static float horizontalThrowingForce = 2000f;
    public static float verticalThrowingForce = 250f;

    public static float speedBoostFactor = 1.25f;

    public static float dashSpeed = 75f;
    public static int dashingFrame = 10;
    public static int dashCDinFrames = 50;
    public static float dashCDinSeconds = dashCDinFrames / 50;

    public static float goalExplosionSpeed = 50f;
    public static int goalExplosionFrame = 35;

    public static float stunningSpeed = 1f;
    public static int stunningFrame = 10;

    public static float guardExpandRate = 0.2f;
    public static float guardMaxSize = 5f;
    public static float haltRate = 0.8f;

    public static float cloneMaxPauseSeconds = 2;

    public static float nearEndingTime = 3f;
    public static float normalTimeScale = 0.8f;
    public static float slowTimeScale = 0.5f;

    public static Color FromChargeToColor(int chargeLevel)
    {
        switch (chargeLevel)
        {
            case 0:
                return Color.white;
            case 1:
                return Color.red;
            case 2:
                return new Color(1f, 0.75f, 0f); // orange
            case 3:
                return Color.yellow;
            case 4:
                return new Color(0f, 1f, 0.6f); // light green
            case 5:
                return Color.green;
            default:
                return Color.black;
        }
    }
}