namespace CompanionAgent.Core;

public sealed class TelemetryCollector : IDisposable
{
    private readonly AcSharedMemoryReader _reader = new();
    private readonly LapBuffer _buffer = new();
    private Timer? _timer;

    private CompletedLapTelemetry? _bestLap;
    private int _bestLapTime = int.MaxValue;
    private bool _sessionActive;

    // Lap delimitation via normalizedCarPosition: a lap is captured strictly
    // between two start/finish line crossings, which guarantees a closed loop
    // and discards the out-lap (pit exit) that precedes the first crossing.
    private float _lastNormPos = -1f;
    private bool  _hasStartedLap;
    private int   _lastSectorIndex = -1;

    public bool IsConnected { get; private set; }

    public event EventHandler<CompletedLapTelemetry>? BestLapCompleted;
    public event EventHandler<SessionTelemetryResult>? SessionEnded;

    public void Start()
    {
        _timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
    }

    private void OnTick(object? _)
    {
        try
        {
            if (!IsConnected)
            {
                IsConnected = _reader.TryConnect();
                return;
            }

            var physics  = _reader.ReadPhysics();
            var graphics = _reader.ReadGraphics();
            if (physics is null || graphics is null) return;

            var g = graphics.Value;
            var p = physics.Value;

            if (g.CarCoordinates is null || g.CarCoordinates.Length < 3) return;

            if (g.Status != 2) // not LIVE
            {
                if (_sessionActive) EndSession();
                return;
            }

            _sessionActive = true;

            var normPos = g.NormalizedCarPosition;

            // Detect start/finish line crossing: normPos wraps from ~1.0 back to ~0.0
            bool crossedLine = _lastNormPos > 0.9f && normPos < 0.1f;
            _lastNormPos = normPos;

            if (crossedLine)
            {
                // The buffer now holds a full closed lap (crossing -> crossing).
                if (_hasStartedLap)
                {
                    var lapTime = g.ILastTime;
                    var completed = _buffer.Finish(lapTime, hasCut: false);

                    if (completed is not null && lapTime > 0 && lapTime < _bestLapTime)
                    {
                        _bestLapTime = lapTime;
                        _bestLap = completed;
                        BestLapCompleted?.Invoke(this, completed);
                    }
                }

                // Start recording the next lap from a clean slate.
                _buffer.Clear();
                _lastSectorIndex = -1;
                _hasStartedLap = true;
            }

            // Only record once we are inside a proper lap (after first crossing),
            // so the pit-exit out-lap is never included.
            if (_hasStartedLap)
            {
                var (cx, cy, cz) = AcStructHelper.GetPlayerPosition(g);

                _buffer.AddFrame(new TelemetryFrame(
                    X:         cx,
                    Y:         cy,
                    Z:         cz,
                    SpeedKmh:  p.SpeedKmh,
                    Throttle:  p.Gas,
                    Brake:     p.Brake,
                    NormPos:   normPos,
                    LapTimeMs: g.ICurrentTime
                ));

                if (g.CurrentSectorIndex != _lastSectorIndex && _lastSectorIndex >= 0)
                    _buffer.RecordSectorBoundary(_lastSectorIndex, normPos);

                _lastSectorIndex = g.CurrentSectorIndex;
            }
        }
        catch { }
    }

    private void EndSession()
    {
        _sessionActive = false;
        if (_bestLap is not null)
        {
            SessionEnded?.Invoke(this, new SessionTelemetryResult(_bestLap, _bestLapTime));
            _bestLap = null;
            _bestLapTime = int.MaxValue;
        }
        _hasStartedLap = false;
        _lastNormPos = -1f;
        _lastSectorIndex = -1;
        _buffer.Clear();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _reader.Dispose();
    }
}

public record SessionTelemetryResult(
    CompletedLapTelemetry BestLap,
    int BestLapTimeMs
);