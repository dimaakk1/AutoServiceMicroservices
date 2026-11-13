using AutoServiceCatalog.BLL.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Part;

namespace AutoServiceCatalog.API.Grpc
{
    public class PartServiceGrpcImpl : PartService.PartServiceBase
    {
        private readonly IServiceService _serviceService;
        private readonly ILogger<PartServiceGrpcImpl> _logger;

        public PartServiceGrpcImpl(IServiceService serviceService, ILogger<PartServiceGrpcImpl> logger)
        {
            _serviceService = serviceService;
            _logger = logger;
        }

        public override async Task<PartReply> GetPart(GetPartRequest request, ServerCallContext context)
        {
            try
            {
                var service = await _serviceService.GetByIdAsync(request.Id);
                if (service == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Part not found"));
                }

                return new PartReply
                {
                    Id = service.ServiceId,
                    Name = service.Name,
                    Price = (double)service.Price,
                    CategoryId = service.CategoryId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting part with id {PartId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetPartsByIdsReply> GetPartsByIds(GetPartsByIdsRequest request, ServerCallContext context)
        {
            try
            {
                var reply = new GetPartsByIdsReply();
                foreach (var ids in request.Ids.Distinct().Chunk(500))
                {
                    var services = await _serviceService.GetByIdsAsync(ids, context.CancellationToken);
                    reply.Parts.AddRange(services.Select(service => new PartReply
                    {
                        Id = service.ServiceId,
                        Name = service.Name,
                        Price = (double)service.Price,
                        CategoryId = service.CategoryId
                    }));
                }

                return reply;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}
