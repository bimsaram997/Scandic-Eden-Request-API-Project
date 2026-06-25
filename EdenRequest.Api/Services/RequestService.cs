using EdenRequest.Api.Data;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{

    public record PlaceBulkRequestDto(int EmployeeId, int RoomListId, string? RoomNumber, List<BulkLineDto> Items);
    public record BulkLineDto(int ItemId, int Quantity, string UnitType);
    public interface IRequestService
    {
        Task<RequestHeader> PlaceRequestAsync(PlaceBulkRequestDto dto);
        Task<IEnumerable<RequestHeader>> GetAllRequestsAsync();
        Task<RequestHeader> ChangeStatusAsync(int requestId, string newStatus);
        Task<RequestHeader?> GetRequestByIdAsync(int id);
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
                CheckGeneralRequest  = string.IsNullOrWhiteSpace(dto.RoomNumber) ? true : false
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

        public async Task<RequestHeader> ChangeStatusAsync(int requestId, string newStatus)
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new KeyNotFoundException("Housekeeping request was not found.");

            if (newStatus != "Acknowledged" && newStatus != "Delivered")
                throw new ArgumentException("Invalid status update value provided.");

            request.Status = newStatus;
            request.UpdatedAt = DateTime.UtcNow;

            await _requestRepository.UpdateAsync(request);
            return request;
        }
    }
}
