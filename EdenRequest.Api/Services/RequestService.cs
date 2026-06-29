using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.Repositories;
using EdenRequest.Api.Requests;
using static EdenRequest.Api.Controllers.RequestController;

namespace EdenRequest.Api.Services
{

    public record PlaceBulkRequestDto(int EmployeeId, int RoomListId, string? RoomNumber, List<BulkLineDto> Items, string Notes);
    public record BulkLineDto(int ItemId, int Quantity, string UnitType);
    public interface IRequestService
    {
        Task<RequestHeader> PlaceRequestAsync(PlaceBulkRequestDto dto);
        Task<IEnumerable<RequestHeader>> GetAllRequestsAsync();
        Task<RequestHeader> ChangeStatusAsync(int requestId, UpdateRquestHeaderRequest payload);
        Task<RequestHeader?> GetRequestByIdAsync(int id);
        Task<PagedResponse<RequestHistoryDto>> GetEmployeeHistoryAsync(int employeeId, bool isTeamLeader, HistoryQueryDto filters);
    }
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;

        public RequestService(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        // Mock Business Rule: Validating Room List Allocations for Testing
        private bool IsRoomValidForList(int roomListId, string roomNumber)
        {
            var dummyRoomListRegistry = new Dictionary<int, List<string>>
            {
                { 10, new List<string> { "101", "102", "103" } }, // List 10 contains Floor 1 rooms
                { 20, new List<string> { "201", "202", "203" } }  // List 20 contains Floor 2 rooms
            };

            if (dummyRoomListRegistry.TryGetValue(roomListId, out var allowedRooms))
            {
                return allowedRooms.Contains(roomNumber);
            }
            return false;
        }

        public async Task<RequestHeader> PlaceRequestAsync(PlaceBulkRequestDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("Must include at least one item to request.");

            // Rule: Validate room matching if the cleaner isn't requesting general stocks
            if (!string.IsNullOrWhiteSpace(dto.RoomNumber))
            {
                if (!IsRoomValidForList(dto.RoomListId, dto.RoomNumber))
                {
                    throw new ArgumentException($"Room {dto.RoomNumber} is not part of your active Room List ID {dto.RoomListId}.");
                }
            }

            var header = new RequestHeader
            {
                EmployeeId = dto.EmployeeId,
                RoomListId = dto.RoomListId,
                RoomNumber = string.IsNullOrWhiteSpace(dto.RoomNumber) ? null : dto.RoomNumber,
                Status = "Pending",
                CheckGeneralRequest  = string.IsNullOrWhiteSpace(dto.RoomNumber) ? true : false,
                Notes = dto.Notes
            };

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");

                if (item.UnitType != "Pcs" && item.UnitType != "Trolley")
                    throw new ArgumentException("Invalid unit type selection.");

                header.Lines.Add(new RequestLine
                {
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    UnitType = item.UnitType
                });
            }

            return await _requestRepository.CreateBulkAsync(header);
        }

        public async Task<IEnumerable<RequestHeader>> GetAllRequestsAsync()
        {
            return await _requestRepository.GetAllRequetsAsync();
        }

        public async Task<RequestHeader?> GetRequestByIdAsync(int id)
        {
            var request = await _requestRepository.GetByIdAsync(id);
            return request;
        }

        public async Task<PagedResponse<RequestHistoryDto>> GetEmployeeHistoryAsync(int employeeId, bool isTeamLeader, HistoryQueryDto filterse)
        {
            var pagedResult = await _requestRepository.GetPagedRequestsByEmployeeAsync(employeeId, isTeamLeader, filterse);

            var mappedData = pagedResult.Data.Select(h => new RequestHistoryDto
            {
                Id = h.Id,
                RoomNumber = h.RoomNumber,
                RoomListId = h.RoomListId,
                Status = h.Status.ToString(),
                CreatedAt = h.CreatedAt,
                Notes = h.Notes,
                Name = h.Employee != null ? h.Employee.Name : "Unassigned",
                // 🍇 Maps from .Lines instead of .Items
                Items = h.Lines.Select(l => new ItemLineDto
                {
                    ItemName = l.Item?.Name ?? "Unknown Item",
                    Quantity = l.Quantity,
                    UnitType = l.UnitType
                }).ToList()
            });

            return new PagedResponse<RequestHistoryDto>
            {
                Data = mappedData,
                TotalCount = pagedResult.TotalCount
            };
        }

        public async Task<RequestHeader> ChangeStatusAsync(int requestId, UpdateRquestHeaderRequest update)
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new KeyNotFoundException("Housekeeping request was not found.");

            if (update.Status == request.Status)
                throw new ArgumentException("Same Status");

            request.Status = update.Status;
            request.UpdatedBy = update.UpdatedBy;
            request.UpdatedAt = DateTime.UtcNow;

            await _requestRepository.UpdateAsync(request);
            return request;
        }
    }
}
