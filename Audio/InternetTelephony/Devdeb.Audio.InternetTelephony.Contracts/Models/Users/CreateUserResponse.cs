using System;

namespace Devdeb.Audio.InternetTelephony.Contracts.Models.Users
{
	public class CreateUserResponse
	{ 
		public bool IsSuccessed { get; set; }
		public Guid UserId { get; set; }
	}
}
