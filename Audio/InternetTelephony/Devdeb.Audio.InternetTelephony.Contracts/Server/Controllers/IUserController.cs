using Devdeb.Audio.InternetTelephony.Contracts.Models.Users;
using System.Threading.Tasks;

namespace Devdeb.Audio.InternetTelephony.Contracts.Server.Controllers
{
	public interface IUserController
	{
		Task<CreateUserResponse> CreateUser(CreateUserRequest user);

		Task<ListItemUserVm[]> GetActiveUsers();

		Task<bool> LoginVm(LoginVm loginInfo);
	}
}
