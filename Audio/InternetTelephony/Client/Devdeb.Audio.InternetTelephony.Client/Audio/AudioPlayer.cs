using NAudio.Wave;

namespace Devdeb.Audio.InternetTelephony.Client.Audio
{
	internal class AudioPlayer
	{
		private readonly WaveOutEvent _waveOutEvent;
		private readonly NetworkWaveProvider _networkWaveProvider;

		public AudioPlayer()
		{
			_waveOutEvent = new WaveOutEvent();
			_networkWaveProvider = new NetworkWaveProvider();
			_waveOutEvent.Init(_networkWaveProvider);
			_waveOutEvent.Play();
		}

		public void AddBufferToPlay(byte[] buffer) => _networkWaveProvider.AddBuffer(buffer);
	}
}
