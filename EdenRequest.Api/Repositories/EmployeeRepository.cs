using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetEmployeeByEmailAndPassword(string email, string password);
        Task<IEnumerable<EmployeeDto>> GetAllEmployeeAsync();
        Task<Employee> GetEmployeeById(int id);
        Task UpdateAsync(Employee employee);
        Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(string role);
        Task<Employee?> GetEmployeeByPushEndpointAsync(string pushEndpoint);

    }
    public class EmployeeRepository: IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetEmployeeByEmailAndPassword(string email, string password)
        {
            return await _context.Employees
             .FirstOrDefaultAsync(e =>
                 e.Email != null && email != null &&
                 e.Email.ToLower() == email.ToLower() &&
                 e.Password == password);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeeAsync()
        {
            return await _context.Employees
        .Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name, // Adjust property name if it's FirstName/LastName in your DB
            Email = e.Email,
            Role = e.Role
        })
        .ToListAsync();
        }

        public async Task<Employee> GetEmployeeById(int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(string role)
        {
            return await _context.Employees
                .Where(e => e.Role == role)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByPushEndpointAsync(string pushEndpoint)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.PushEndpoint == pushEndpoint);
        }
    }
}
