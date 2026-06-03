namespace CompanionAgent.Core;

public sealed class TelemetryCollector : IDisposable
{
    private readonly AcSharedMemoryReader _reader = new();
    private readonly LapBuffer _buffer = new();
    private Timer? _timer;

    private int _lastCompletedLaps = -1;
    private int _lastSectorIndex = -1;
    private CompletedLapTelemetry? _bestLap;
    private int _bestLapTime = int.MaxValue;
    private bool _sessionActive;
    private bool _announcedConnected;

    public bool IsConnected { get; private set; }

    // Optional logging hook so the agent UI/log can show telemetry activity.
    public Action<string>? Log { get; set; }

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
                if (IsConnected && !_announcedConnected)
                {
                    _announcedConnected = true;
                    Log?.Invoke("Telemetria: conectado ao Assetto Corsa");
                }
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

            if (!_sessionActive)
            {
                _sessionActive = true;
                Log?.Invoke("Telemetria: sessão ao vivo detectada, gravando");
            }

            var (cx, cy, cz) = AcStructHelper.GetPlayerPosition(g);

            _buffer.AddFrame(new TelemetryFrame(
                X:         cx,
                Y:         cy,
                Z:         cz,
                SpeedKmh:  p.SpeedKmh,
                Throttle:  p.Gas,
                Brake:     p.Brake,
                NormPos:   g.NormalizedCarPosition,
                LapTimeMs: g.ICurrentTime
            ));

            if (g.CurrentSectorIndex != _lastSectorIndex && _lastSectorIndex >= 0)
                _buffer.RecordSectorBoundary(_lastSectorIndex, g.NormalizedCarPosition);
            _lastSectorIndex = g.CurrentSectorIndex;

            // A lap completed (the game incremented the lap counter).
            if (g.CompletedLaps > _lastCompletedLaps && _lastCompletedLaps >= 0)
            {
                var lapTime = g.ILastTime;
                var frames = _buffer.FrameCount;
                var completed = _buffer.Finish(lapTime, hasCut: false);
                _buffer.Clear();
                _lastSectorIndex = -1;

                if (completed is not null && lapTime > 0)
                {
                    Log?.Invoke($"Telemetria: volta completa ({lapTime / 1000.0:0.000}s, {frames} pontos)");
                    if (lapTime < _bestLapTime)
                    {
                        _bestLapTime = lapTime;
                        _bestLap = completed;
                        BestLapCompleted?.Invoke(this, completed);
                        Log?.Invoke("Telemetria: nova melhor volta registrada");
                    }
                }
                else
                {
                    Log?.Invoke($"Telemetria: volta ignorada (frames={frames}, tempo={lapTime}ms)");
                }
            }
            _lastCompletedLaps = g.CompletedLaps;
        }
        catch { /* never crash the timer thread */ }
    }

    private void EndSession()
    {
        _sessionActive = false;
        if (_bestLap is not null)
        {
            Log?.Invoke($"Telemetria: sessão encerrada, enviando melhor volta ({_bestLapTime / 1000.0:0.000}s)");
            SessionEnded?.Invoke(this, new SessionTelemetryResult(_bestLap, _bestLapTime));
            _bestLap = null;
            _bestLapTime = int.MaxValue;
        }
        else
        {
            Log?.Invoke("Telemetria: sessão encerrada sem volta completa para enviar");
        }
        _lastCompletedLaps = -1;
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