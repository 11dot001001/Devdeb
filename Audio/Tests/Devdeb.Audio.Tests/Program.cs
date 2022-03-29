using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Management;
using Devdeb.Audio.Tests.Wav;
using System.Drawing;
using System.Drawing.Imaging;

namespace Devdeb.Audio.Tests
{
	internal class Program
	{
		private const int _sampleRate = 48000;
		private const int _bitsPerSample = 16;
		private const int _channel = 2;
		private const int _recordingSeconds = 5;

		private static readonly byte[] _buffer = new byte[_recordingSeconds * _sampleRate * (_bitsPerSample >> 3) * _channel];
		private static int _bufferOffset = 0;

		static unsafe void Main(string[] args)
		{
			ushort value = 40234;

			byte[] bytes = new byte[2];

			fixed (byte* ptr = &bytes[0])
			{
				*(ushort*)ptr = value;
			}
			short value2 = (short)(bytes[1] << 8 | bytes[0]);

			using var inputDevice = new WaveInEvent();
			inputDevice.WaveFormat = new WaveFormat(_sampleRate, _bitsPerSample, _channel);

			inputDevice.DataAvailable += InputDevice_DataAvailable;
			inputDevice.StartRecording();
			Console.WriteLine("Sound recording has started");

			for (; _bufferOffset != _buffer.Length;)
			{
				Thread.Sleep(1);
			}
			Console.WriteLine("Sound recording has stopped");

			WavFile wavFile = new WavFile
			{
				NumChannels = (NumChannels)_channel,
				SampleRate = _sampleRate,
				BitsPerSample = _bitsPerSample,
				Data = _buffer
			};

			WavFileWriter.WriteFile(wavFile, @"C:\Users\lehac\Desktop\test.wav");
			WaveJpgRenderer.Render(inputDevice.WaveFormat, _buffer, @"C:\Users\lehac\Desktop\test_view.jpg");
		}

		private static void InputDevice_DataAvailable(object sender, WaveInEventArgs e)
		{
			var writeBytesCount = Math.Min(e.BytesRecorded, _buffer.Length - _bufferOffset);
			Array.Copy(e.Buffer, 0, _buffer, _bufferOffset, writeBytesCount);
			_bufferOffset += writeBytesCount;
		}

		public static class WaveJpgRenderer
		{
			private const int _widthPixels = 4096;
			private const int _heightChannelPixels = 1024;
			private const int _channelBorderPixels = 2;
			private const int _heightPixels = 2 * _heightChannelPixels + _channelBorderPixels;
			public static unsafe void Render(WaveFormat waveFormat, byte[] pcmBuffer, string outputPath)
			{
				int bytesPerSample = waveFormat.BitsPerSample >> 3;
				int channelBytes = pcmBuffer.Length / waveFormat.Channels;
				int samplesPerChannel = channelBytes / bytesPerSample;

				Bitmap bitmap = new Bitmap(_widthPixels, _heightPixels);
				for (int width = 0; width != bitmap.Width; width++)
					for (int height = 0; height != bitmap.Height; height++)
						bitmap.SetPixel(width, height, Color.White);

				IEnumerable<short> GetLeft16BitChannel(byte[] buffer)
				{
					for (int i = 0; i != buffer.Length; i += 4)
					{
						yield return (short)(buffer[i + 1] << 8 | buffer[i]);
					}
				}
				IEnumerable<short> GetRight16BitChannel(byte[] buffer)
				{
					for (int i = 2; i != buffer.Length + 2; i += 4)
						yield return (short)(buffer[i + 1] << 8 | buffer[i]);
				}

				int heightOffset = 0;
				Render16BitChannel(GetLeft16BitChannel(pcmBuffer), samplesPerChannel, heightOffset, bitmap, new Pen(Color.Red));
				heightOffset += _heightChannelPixels;
				RenderChannelBorder(heightOffset, bitmap);
				heightOffset += _channelBorderPixels;
				Render16BitChannel(GetRight16BitChannel(pcmBuffer), samplesPerChannel, heightOffset, bitmap, new Pen(Color.Black));
				heightOffset += _heightChannelPixels;

				bitmap.Save(outputPath, ImageFormat.Jpeg);
			}

			private static IEnumerable<(int X, int Y)> EnumeratePixelsOf16BitChannel(IEnumerable<short> samples, int samplesCount, Bitmap bitmap)
			{
				int samplesPerWidthPixel = samplesCount > _widthPixels ? samplesCount / _widthPixels : 1;

				var samplesEnumerator = samples.GetEnumerator();
				for (int width = 0; width != bitmap.Width; width++)
				{
					int samplePerWidthPixelAccumulator = 0;
					int samplePerWidthPixelIndex = 0;
					for (; samplePerWidthPixelIndex != samplesPerWidthPixel; samplePerWidthPixelIndex++)
					{
						if (!samplesEnumerator.MoveNext())
							break;

						samplePerWidthPixelAccumulator += samplesEnumerator.Current;
					}
					short averageSamplesValue = (short)(samplePerWidthPixelAccumulator / samplePerWidthPixelIndex);

					int heightPixelValue = averageSamplesValue * _heightChannelPixels / short.MaxValue + _heightChannelPixels / 2;

					yield return (width, heightPixelValue);
				}
			}
			private static void Render16BitChannel(IEnumerable<short> samples, int samplesCount, int heightOffset, Bitmap bitmap, Pen pen)
			{
				IEnumerator<(int X, int Y)> pixelsEnumerator = EnumeratePixelsOf16BitChannel(samples, samplesCount, bitmap).GetEnumerator();

				Graphics graphics = Graphics.FromImage(bitmap);

				if (!pixelsEnumerator.MoveNext())
					return;

				(int X, int Y) lastPosition = pixelsEnumerator.Current;
				for (; pixelsEnumerator.MoveNext();)
				{
					(int X, int Y) currentPosition = pixelsEnumerator.Current;
					graphics.DrawLine(pen, lastPosition.X, lastPosition.Y + heightOffset, currentPosition.X, currentPosition.Y + heightOffset);
					lastPosition = currentPosition;
				}
			}
			private static void RenderChannelBorder(int heightOffset, Bitmap bitmap)
			{
				Graphics graphics = Graphics.FromImage(bitmap);
				Pen pen = new Pen(Color.Black);
				for (int channelBorderPixelOffset = 0; channelBorderPixelOffset < _channelBorderPixels; channelBorderPixelOffset++)
					graphics.DrawLine(pen, 0, channelBorderPixelOffset + heightOffset, bitmap.Width, channelBorderPixelOffset + heightOffset);
			}
		}

		static void Trash2()
		{
			using (var inputDevice = new WaveInEvent())
			using (var outputDevice = new WaveOutEvent())
			{
				inputDevice.WaveFormat = new WaveFormat(48000, 16, 2);
				var waveInProvider = new WaveInProvider(inputDevice);
				inputDevice.StartRecording();
				outputDevice.DesiredLatency = 500;
				outputDevice.NumberOfBuffers = 2;
				outputDevice.DeviceNumber = 0;

				outputDevice.Init(waveInProvider);
				outputDevice.Play();
				while (outputDevice.PlaybackState == PlaybackState.Playing)
				{
				}
			}
		}

		static void Trash()
		{
			IWavePlayer a;
			IWaveProvider waveProvider;
			//ISampleProvider sampleProvider
			IWaveProvider aa;
			WaveOut b;
			WasapiOut aaa;
			//BufferedWaveProvider a;
			//int aa = WaveOut.DeviceCount;
			List<DirectSoundDeviceInfo> c = DirectSoundOut.Devices.ToList();
			string audioFileName = @"C:\Users\lehac\Desktop\Макс Корж - Время.mp3";
			string audioFileName2 = @"C:\Users\lehac\Desktop\NF - Mansion.mp3";
			var aaaaaa = AsioOut.GetDriverNames(); // самый топ

			var capabilities = new List<WaveOutCapabilities>();
			for (int i = -1; i < WaveOut.DeviceCount; i++)
				capabilities.Add(WaveOut.GetCapabilities(i));

			var capabilities2 = new List<WaveInCapabilities>();
			for (int i = -1; i < WaveIn.DeviceCount; i++)
				capabilities2.Add(WaveInEvent.GetCapabilities(i));

			var devices = DirectSoundOut.Devices.ToList();
			using (var inputDevice = new WaveInEvent())
			using (var audioFile = new AudioFileReader(audioFileName))
			using (var audioFile2 = new AudioFileReader(audioFileName2))
			using (var outputDevice = new WaveOutEvent())
			//using (var outputDevice2 = new DirectSoundOut(c[0].Guid, 300))
			//using (var outputDevice3 = new AsioOut())
			{
				WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(inputDevice.DeviceNumber);
				inputDevice.WaveFormat = new WaveFormat(48000, 16, 2);
				var waveInProvider = new WaveInProvider(inputDevice);
				inputDevice.StartRecording();
				outputDevice.DesiredLatency = 500;
				outputDevice.NumberOfBuffers = 2;
				outputDevice.DeviceNumber = 0;
				var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(8000, 1));
				//var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
				//mixer.AddMixerInput((ISampleProvider)audioFile);
				//mixer.AddMixerInput((IWaveProvider)audioFile2);
				//mixer.AddMixerInput(waveInProvider);
				RawSourceWaveStream aaaa = new RawSourceWaveStream(audioFile, audioFile.WaveFormat);

				mixer.ReadFully = true;
				audioFile.Volume = 0.013F;

				outputDevice.Init(waveInProvider);
				outputDevice.Play();
				while (outputDevice.PlaybackState == PlaybackState.Playing)
				{
				}
			}
			var objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");

			var objCollection = objSearcher.Get();
		}
	}
}
