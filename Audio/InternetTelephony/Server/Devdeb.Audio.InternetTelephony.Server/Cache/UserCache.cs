using Devdeb.Audio.InternetTelephony.Contracts.Models.Users;
using Devdeb.Audio.InternetTelephony.Server.Models;
using Devdeb.Network.TCP.Communication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Server.Cache
{
	internal class UserCache
	{
		private readonly ConcurrentDictionary<Guid, User> _users;

		public UserCache() => _users = new ConcurrentDictionary<Guid, User>();

		public Task<CreateUserResponse> CreateUser(CreateUserRequest user, TcpCommunication tcpCommunication)
		{
			if (_users.Values.Any(x => x.Name == user.Name))
				return Task.FromResult(new CreateUserResponse() { IsSuccessed = false });

			Guid userId = Guid.NewGuid();
			_ = _users.TryAdd(userId, new User() 
			{ 
				Name = user.Name, 
				Id = userId,
				TcpCommunication = tcpCommunication
			});

			return Task.FromResult(new CreateUserResponse() { IsSuccessed = true, UserId = userId });
		}

		public ICollection<User> Users => _users.Values;

		public bool TryGetUser(Guid userId, out User user) => _users.TryGetValue(userId, out user);
	}
}
