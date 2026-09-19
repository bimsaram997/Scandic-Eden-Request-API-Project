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
        Task<bool> SavePushTokenAsync(int employeeId, PushSubscriptionDto dto);
        Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(string role);
        Task<EmployeeDto> GetEmployeeGenericDataById(int id);
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

        public async Task<bool> SavePushTokenAsync(int employeeId, PushSubscriptionDto dto)
        {
            var employee = await _employeeRepository.GetEmployeeById(employeeId);
            if (employee == null) return false;

            if (!string.IsNullOrEmpty(dto.PushEndpoint))
            {
                var previousDeviceOwner = await _employeeRepository.GetEmployeeByPushEndpointAsync(dto.PushEndpoint);

                if (previousDeviceOwner != null && previousDeviceOwner.Id != employeeId)
                {
                    previousDeviceOwner.PushEndpoint = null;
                    previousDeviceOwner.PushP256DH = null;
                    previousDeviceOwner.PushAuth = null;

                    await _employeeRepository.UpdateAsync(previousDeviceOwner);
                }
            }

            employee.PushEndpoint = dto.PushEndpoint;
            employee.PushP256DH = dto.PushP256DH;
            employee.PushAuth = dto.PushAuth;
            await _employeeRepository.UpdateAsync(employee);
            return true;
        }

        public Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(string role)
        {
            return _employeeRepository.GetEmployeesByRoleAsync(role);
        }

        public async Task<EmployeeDto> GetEmployeeGenericDataById(int id)
        {
            var request = await _employeeRepository.GetEmployeeById(id);
            if (request == null) return null;
            return new EmployeeDto
            {
                Id = request.Id,
                Name = request.Name,
                Role = request.Role,
                Email = request.Email


            };
        }
    }
}
