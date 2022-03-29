using Devdeb.Audio.InternetTelephony.Contracts.Models.Users;
using Devdeb.Audio.InternetTelephony.Contracts.Server.Controllers;
using Devdeb.Audio.InternetTelephony.Server.Cache;
using Devdeb.Network.TCP.Rpc.Requestor.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Server.Controllers
{
	internal class UserController : IUserController
	{
		private readonly UserCache _userCache;
		private readonly IRequestorContext _requestorContext;

		public UserController(UserCache userCache, IRequestorContext requestorContext)
		{
			_userCache = userCache ?? throw new ArgumentNullException(nameof(userCache));
			_requestorContext = requestorContext ?? throw new ArgumentNullException(nameof(requestorContext));
		}

		public Task<CreateUserResponse> CreateUser(CreateUserRequest user)
		{
			return _userCache.CreateUser(user, _requestorContext.TcpCommunication);
		}

		public Task<ListItemUserVm[]> GetActiveUsers()
		{
			var users = _userCache.Users
				.Where(x => x.TcpCommunication != _requestorContext.TcpCommunication)
				.Select(x => new ListItemUserVm() { Id = x.Id, Name = x.Name }).ToArray();
			return Task.FromResult(users);
		}

		public Task<bool> LoginVm(LoginVm loginInfo)
		{
			throw new NotImplementedException();
		}
	}
}
