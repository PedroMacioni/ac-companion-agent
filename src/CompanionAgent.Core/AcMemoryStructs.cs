using System.Runtime.InteropServices;

namespace CompanionAgent.Core;

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct AcPhysics
{
    public int    PacketId;
    public float  Gas;
    public float  Brake;
    public float  Fuel;
    public int    Gear;
    public int    Rpms;
    public float  SteerAngle;
    public float  SpeedKmh;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] Velocity;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] AccG;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelSlip;
    public float  WheelLoad;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelsPressure;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelAngularSpeed;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreWear;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreDirtyLevel;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreCoreTemperature;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] CamberRad;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] SuspensionTravel;
    public float  Drs;
    public float  TC;
    public float  Heading;
    public float  Pitch;
    public float  Roll;
    public float  CgHeight;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public float[] CarDamage;
    public int    NumberOfTyresOut;
    public int    PitLimiterOn;
    public float  Abs;
    public float  KersCharge;
    public float  KersInput;
    public int    AutoShifterOn;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public float[] RideHeight;
    public float  TurboBoost;
    public float  Ballast;
    public float  AirDensity;
    public float  AirTemp;
    public float  RoadTemp;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] LocalAngularVelocity;
    public float  FinalFF;
    public float  PerformanceMeter;
    public int    EngineBrake;
    public int    ErsRecoveryLevel;
    public int    ErsPowerLevel;
    public int    ErsHeatCharging;
    public int    ErsIsCharging;
    public float  KersCurrentKJ;
    public int    DrsAvailable;
    public int    DrsEnabled;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] BrakeTemp;
    public float  Clutch;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreTempI;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreTempM;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreTempO;
    public int    IsAIControlled;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public float[] TyreContactPoint;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public float[] TyreContactNormal;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public float[] TyreContactHeading;
    public float  BrakeBias;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] LocalVelocity;
}

// Matches rsys-dev/accsharedmemory exactly: CharSet.Unicode + ByValTStr for strings
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct AcGraphics
{
    public int    PacketId;
    public int    Status;           // 0=OFF 1=REPLAY 2=LIVE 3=PAUSE
    public int    Session;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string CurrentTime;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string LastTime;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string BestTime;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string Split;
    public int    CompletedLaps;
    public int    Position;
    public int    ICurrentTime;
    public int    ILastTime;
    public int    IBestTime;
    public float  SessionTimeLeft;
    public float  DistanceTraveled;
    public int    IsInPit;
    public int    CurrentSectorIndex;
    public int    LastSectorTime;
    public int    NumberOfLaps;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string TyreCompound;
    public float  ReplayTimeMultiplier;
    public float  NormalizedCarPosition;
    public int    ActiveCars;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 180)]
    public float[] CarCoordinates;  // 60 cars × 3 floats [X, Y, Z]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public int[]  CarID;
    public int    PlayerCarID;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct AcStatic
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string SmVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string AcVersion;
    public int    NumberOfSessions;
    public int    NumberOfCars;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string CarModel;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string Track;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerSurname;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerNick;
    public int    SectorCount;
}

public static class AcStructHelper
{
    // Use PlayerCarID to find the player's slot in CarCoordinates
    // Fallback to index 0 in single-player if CarID lookup fails
    public static (float X, float Y, float Z) GetPlayerPosition(AcGraphics g)
    {
        if (g.CarCoordinates is null || g.CarCoordinates.Length < 3)
            return (0, 0, 0);

        var idx = 0;
        if (g.CarID != null)
        {
            for (int i = 0; i < g.CarID.Length && i * 3 + 2 < g.CarCoordinates.Length; i++)
            {
                if (g.CarID[i] == g.PlayerCarID)
                {
                    idx = i;
                    break;
                }
            }
        }

        return (g.CarCoordinates[idx * 3], g.CarCoordinates[idx * 3 + 1], g.CarCoordinates[idx * 3 + 2]);
    }
}