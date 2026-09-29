using Android.Media;

namespace MotionAlarm;

public sealed class AlarmPlayer : IAlarmPlayer
{
    private MediaPlayer? _player;
    private readonly object _sync = new();

    public Task PlayLoopAsync(CancellationToken ct = default)
    {
        lock (_sync)
        {
            if (_player?.IsPlaying == true)
            {
                return Task.CompletedTask;
            }

            _player?.Release();

            _player = new MediaPlayer();

            using var afd =
                Android.App.Application.Context.Assets.OpenFd("alarm.mp3");

            _player.SetAudioAttributes(
                new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Alarm)
                    .Build());

            _player.SetDataSource(
                afd.FileDescriptor,
                afd.StartOffset,
                afd.Length);

            _player.Looping = true;
            _player.Prepare();
            _player.Start();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        lock (_sync)
        {
            if (_player is null)
            {
                return Task.CompletedTask;
            }
                
            if (_player.IsPlaying)
            {
                _player.Stop();
            }
                
            _player.Release();
            _player.Dispose();
            _player = null;
        }

        return Task.CompletedTask;
    }
}