using Devdeb.Serialization;
using Devdeb.Serialization.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test.Devdeb.Serialization
{
	[TestClass]
	public class SerializerTest
	{
		[TestMethod]
		public void Run()
		{
			Player player = new Player(new PlayerData(3, 4), 5);
			PlayerSerializer2 playerSerializer = new PlayerSerializer2();


		}
	}

	public class PlayerData
	{
		public PlayerData(int login, int password)
		{
			Login = login;
			Password = password;
		}

		public int Login { get; set; }
		public int Password { get; set; }
	}
	public class Player
	{
		public int Field;

		public Player(PlayerData playerData, int field)
		{
			PlayerData = playerData ?? throw new ArgumentNullException(nameof(playerData));
			Field = field;
		}

		public PlayerData PlayerData { get; set; }
	}

	public class PlayerSerializer : CustomSerializer<Player>
	{
		protected override void Configure(SerializerConfigurations<Player> serializerBuilderSettings)
		{
			serializerBuilderSettings.AddMember(x => x.PlayerData);
			serializerBuilderSettings.AddMember(x => x.Field);
		}
	}
	public class PlayerDataSerializer : CustomSerializer<PlayerData>
	{
		protected override void Configure(SerializerConfigurations<PlayerData> serializerSettings) => serializerSettings.AddMember(x => x.Login);
	}
	public class PlayerSerializer2 : CustomSerializer<Player>
	{
		protected override void Configure(SerializerConfigurations<Player> serializerBuilderSettings)
		{
			serializerBuilderSettings.AddMember(x => x.PlayerData, new PlayerDataSerializer());
			serializerBuilderSettings.AddMember(x => x.Field);
		}
	}

	public sealed class PlayerSerializer2Inner : Serializer<Player>
	{
		private readonly Serializer<PlayerData> _serializer1;
		private readonly Serializer<int> _serializer2;

		public PlayerSerializer2Inner(Serializer<PlayerData> serializer1, Serializer<int> serializer2)
		{
			_serializer1 = serializer1 ?? throw new ArgumentNullException(nameof(serializer1));
			_serializer2 = serializer2 ?? throw new ArgumentNullException(nameof(serializer2));
		}

		public sealed override int Count(Player instance) => _serializer1.Count(instance.PlayerData) + _serializer2.Count(instance.Field);

		public sealed override Player Deserialize(byte[] buffer, ref int index) => new Player(_serializer1.Deserialize(buffer, ref index), _serializer2.Deserialize(buffer, ref index));

		public sealed override void Serialize(Player instance, byte[] buffer, ref int index)
		{
			_serializer1.Serialize(instance.PlayerData, buffer, ref index);
			_serializer2.Serialize(instance.Field, buffer, ref index);
		}
	}
}