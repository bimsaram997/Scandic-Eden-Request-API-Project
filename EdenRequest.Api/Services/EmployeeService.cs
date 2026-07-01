using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IEmployeeService
    {
        Task<Employee> GetEmployeeByEmailAndPassword(string email, string password);
        Task<Employee> GetEmployeeById(int id);
        Task<IEnumerable<EmployeeDto>> GetAllEmployeeAsync();
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


        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeeAsync()
        {
            return await _employeeRepository.GetAllEmployeeAsync();
        }

        public Task<Employee> GetEmployeeById(int id)
        {
            return _employeeRepository.GetEmployeeById(id);
        }
    }
}
