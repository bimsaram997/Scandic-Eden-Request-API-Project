using EdenRequest.Api.Data;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IEmployeeService
    {
        Task<Employee> GetEmployeeByEmailAndPassword(string email, string password);
    }
    public class EmployeeService: IEmployeeService  
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Employee> GetEmployeeByEmailAndPassword(string email, string password)
        {
            var employee = await _employeeRepository.GetEmployeeByEmailAndPassword(email, password);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with email {email}  does not exist.");
            }
            return employee;
        }
    }
}
