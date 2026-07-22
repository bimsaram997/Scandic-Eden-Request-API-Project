using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.Dtos;
using EdenRequest.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EdenRequest.Api.Services
{
    public interface IExtraWorkRequestService
    {
        Task<ExtraWorkRequestDto> CreateRequestAsync(CreateExtraWorkRequestDto dto);
        Task<ExtraWorkRequestDto?> GetRequestByIdAsync(int id);
        Task<PagedResponse<ExtraWorkRequestDto>> GetAllExtraRequestsAsync(AllExtraWorkQueryDto filters);
        Task<ExtraWorkRequest> UpdateStatusAsync(int extraRequestId, UpdateExtraWorkRequestDto request);
    }
    public class ExtraWorkRequestService: IExtraWorkRequestService
    {
        private readonly IExtraWorkRequestRepository _extraWorkRequestRepository;
        public ExtraWorkRequestService(IExtraWorkRequestRepository extraWorkRequestRepository)
        {
            _extraWorkRequestRepository = extraWorkRequestRepository;

        }
        public async Task<ExtraWorkRequestDto> CreateRequestAsync(CreateExtraWorkRequestDto dto)
        {
            var newRequest = new ExtraWorkRequest
            {
                RoomNumber = dto.RoomNumber,
                ListNumber = dto.ListNumber,
                RequestedById = dto.RequestedById,
                AssignedToId = dto.AssignedToId,
                Notes = dto.Notes,
                AddedDate = DateTime.UtcNow,
                Status = "Pending",

                ExtraRequestLine = dto.Lines.Select(line => new ExtraRequestLine
                {
                    ExtraWorkItemId = line.ExtraWorkItemId,
                    Quantity = line.Quantity
                }).ToList()
            };

            var savedRequest = await _extraWorkRequestRepository.CreateBulkAsync(newRequest);

            var resultDto = await GetRequestByIdAsync(newRequest.Id);

            return resultDto ?? throw new Exception("Error retrieving the newly created extra work request.");


        }

        public async Task<ExtraWorkRequestDto?> GetRequestByIdAsync(int id)
        {
            var request = await _extraWorkRequestRepository.GetByIdAsync(id);

            if (request == null) return null;

            return new ExtraWorkRequestDto
            {
                Id = request.Id,
               
                RoomNumber = request.RoomNumber ?? null,
               ListNumber = request.ListNumber ,

                RequestedById = request.RequestedById,
                RequestedByEmployee = request.RequestedBy != null
                    ? request.RequestedBy.Name
                    : "Unknown",

                AssignedToId = request.AssignedToId,
                AssignedToEmployee = request.AssignedTo != null
                    ?request.AssignedTo.Name
                    : "Unassigned",

                UpdatedById = request.UpdatedById,
                UpdatedByEmployee = request.UpdatedBy != null
                    ? request.UpdatedBy.Name
                    : "N/A",

                AddedDate = request.AddedDate,
                AcknowledgedDate = request.AcknowledgedDate,
                DoneDate = request.DoneDate,
                Notes = request.Notes,
                Status = request.Status,

                Lines = request.ExtraRequestLine.Select(line => new ExtraRequestLineDto
                {
                    Id = line.Id,
                    ExtraWorkItemId = line.ExtraWorkItemId,
                    ExtraWorkItemName = line.ExtraWorkItem?.Name ?? "Unknown Item",
                    Quantity = line.Quantity
                }).ToList()
            };
        }
        public async Task<PagedResponse<ExtraWorkRequestDto>> GetAllExtraRequestsAsync(AllExtraWorkQueryDto filters)
        {
            var pagedResult = await _extraWorkRequestRepository.GetPagedExtraWorkRequestsAsync(filters);

            var mappedData = pagedResult.Data.Select(request => new ExtraWorkRequestDto
            {
                Id = request.Id,
              
                RoomNumber = request.RoomNumber ?? null,
                ListNumber = request.ListNumber,
                Status = request.Status,
                AddedDate = request.AddedDate,
                Notes = request.Notes,

                RequestedById = request.RequestedById,
                RequestedByEmployee = request.RequestedBy != null
                    ? request.RequestedBy.Name
                    : "Unknown",

                AssignedToId = request.AssignedToId,
                AssignedToEmployee = request.AssignedTo != null
                    ? request.AssignedTo.Name
                    : "Unassigned",

                UpdatedById = request.UpdatedById,
                UpdatedByEmployee = request.UpdatedBy != null
                    ? request.UpdatedBy.Name
                    : "N/A",

                Lines = request.ExtraRequestLine.Select(line => new ExtraRequestLineDto
                {
                    Id = line.Id,
                    ExtraWorkItemId = line.ExtraWorkItemId,
                    ExtraWorkItemName = line.ExtraWorkItem?.Name ?? "Unknown Item",
                    Quantity = line.Quantity
                }).ToList()
            }).ToList();

            return new PagedResponse<ExtraWorkRequestDto>
            {
                Data = mappedData,
                TotalCount = pagedResult.TotalCount
            };
        }

        public async Task<ExtraWorkRequest> UpdateStatusAsync(int extraRequestId, UpdateExtraWorkRequestDto request)
        {
            var requestToUpdate = await _extraWorkRequestRepository.GetByIdAsync(extraRequestId);
            if (requestToUpdate == null)
                throw new KeyNotFoundException("Extra Work request was not found.");

            if (requestToUpdate.Status == request.Status)
                throw new ArgumentException("Same Status");

            requestToUpdate.Status = request.Status;
            requestToUpdate.UpdatedById = request.UpdatedById;
            if(request.Status == "Acknowledge")
            {
                requestToUpdate.AcknowledgedDate = DateTime.UtcNow;
            }else if(request.Status == "Done")
            {
                requestToUpdate.DoneDate = DateTime.UtcNow;
            }
            

            await _extraWorkRequestRepository.UpdateStatusAsync(requestToUpdate);
            return requestToUpdate;
        }

      
    }
}
