using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IExtraWorkItemService
    {
       Task<IEnumerable<ExtraWorkItemDto>> GetAllExtraWorkItemsAsync();
       Task<ExtraWorkItemDto> GetByIdAsync(int id);
        Task<ExtraWorkItemDto> CreateAsync(CreateExtraWorkDto item);
        Task UpdateAsync(UpdateExtraWorkDto item);
    }
    public class ExtraWorkItemService: IExtraWorkItemService
    {
        private readonly IExtraWorkItemRepository _extraWorkItemRepositoryRepository;
        public ExtraWorkItemService(IExtraWorkItemRepository extraWorkItemRepository) {
            _extraWorkItemRepositoryRepository = extraWorkItemRepository;
        }

        public async Task<ExtraWorkItemDto> CreateAsync(CreateExtraWorkDto item)
        {
            var newItem = new ExtraWorkItem
            {
                Name = item.Name,
                CreatedById = item.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            var savedItem = await _extraWorkItemRepositoryRepository.CreateAsync(newItem);

            return new ExtraWorkItemDto
            {
                Id = savedItem.Id,
                Name = savedItem.Name,
                CreatedById = savedItem.CreatedById,
                CreatedAt = savedItem.CreatedAt,
                UpdatedAt = savedItem.UpdatedAt,
                UpdatedById = savedItem.UpdatedById,
            };
        }

        public async Task<IEnumerable<ExtraWorkItemDto>> GetAllExtraWorkItemsAsync()
        {
            var items = await _extraWorkItemRepositoryRepository.GetAllAsync();
            var itemDtos = items.Select(item => new ExtraWorkItemDto
            {
                Id = item.Id,
                Name = item.Name,
                CreatedById = item.CreatedById,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                UpdatedById = item.UpdatedById
            });

            return itemDtos;
        }

        public async Task<ExtraWorkItemDto> GetByIdAsync(int id)
        {
            var item = await _extraWorkItemRepositoryRepository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"ExtraWorkItem with ID {id} not found.");
            }

            return new ExtraWorkItemDto
            {
                Id = item.Id,
                Name = item.Name,
                CreatedById = item.CreatedById,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                UpdatedById = item.UpdatedById
            };
        }

        public async Task UpdateAsync(UpdateExtraWorkDto item)
        {
            var existingItem = await _extraWorkItemRepositoryRepository.GetByIdAsync(item.Id);
            if (existingItem == null)
            {
                throw new KeyNotFoundException($"ExtraWorkItem with ID {item.Id} not found.");
            }

            existingItem.Name = item.Name;
            existingItem.UpdatedById = item.UpdatedById;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _extraWorkItemRepositoryRepository.UpdateAsync(existingItem);
        }
    }
}
