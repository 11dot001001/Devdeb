using Devdeb.Network.TCP.Rpc.Requestor.Context;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions.Server;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Api;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Server
{
	public class ServerStudentContoller : IStudentContoller
	{
		private static readonly Dictionary<Guid, StudentVm> _students;
		private static int _freeId;
		private static readonly object _freeIdLocker;

		static ServerStudentContoller()
		{
			_freeIdLocker = new object();
			_freeId = 0;
			_students = new Dictionary<Guid, StudentVm>();
		}

		private readonly IDateTimeService _dateTimeService;
		private readonly IRequestorContext _requestorContext;
		private readonly ClientApi _clientApi;

		public ServerStudentContoller(
			IDateTimeService dateTimeService,
			IRequestorContext requestorContext,
			ClientApi clientApi
		)
		{
			_dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
			_requestorContext = requestorContext ?? throw new ArgumentNullException(nameof(requestorContext));
			_clientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
		}

		public int FreeId
		{
			get
			{
				lock (_freeIdLocker)
				{
					Console.WriteLine($"Returned id {_freeId}.");
					return _freeId++;
				}
			}
		}

		public Task<Guid> AddStudent(StudentFm studentFm, int testValue)
		{
			Guid studentId = Guid.NewGuid();

			lock (_students)
				_students.Add(
					studentId,
					new StudentVm
					{
						Id = studentId,
						Name = studentFm.Name,
						Age = studentFm.Age
					}
				);

			Console.WriteLine(
				$"####################\n" +
				$"Student {studentFm.Name} was added with id {studentId}.\n" +
				$"TestValue {testValue}.\n" +
				$"Current date: {_dateTimeService.CurrentDateTime}.\n" +
				$"####################"
			);

			_clientApi.ClientController.HandleStudentUpdate(
				Guid.NewGuid(),
				new StudentVm { Name = "maria ivanovna" }
			);

			return Task.FromResult(studentId);
		}
		public void DeleteStudent(Guid id)
		{
			Console.WriteLine($"Student {id} was deleted.");
			_students.Remove(id);
		}
		public Task<StudentVm> GetStudent(Guid id) => Task.FromResult(_students[id]);
	}
}
