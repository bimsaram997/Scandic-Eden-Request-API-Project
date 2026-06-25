using EdenRequest.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetEmployeeByEmailAndPassword(string email, string password);
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

    }
}
