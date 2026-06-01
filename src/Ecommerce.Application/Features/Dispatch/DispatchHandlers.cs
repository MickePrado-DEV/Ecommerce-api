using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Application.Features.Dispatch.Services;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Emums;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Dispatch;

public record GetDispatchSettingsQuery : IRequest<Result<DispatchSettingsDto>>;

public class GetDispatchSettingsQueryHandler(IDispatchRepository dispatch)
    : IRequestHandler<GetDispatchSettingsQuery, Result<DispatchSettingsDto>>
{
    public async Task<Result<DispatchSettingsDto>> Handle(GetDispatchSettingsQuery request, CancellationToken ct)
    {
        var s = await dispatch.GetOrCreateSettingsAsync(ct);
        return Result.Ok(new DispatchSettingsDto(
            s.DefaultClusterRadiusKm,
            s.DefaultMaxStopsPerRoute,
            s.DefaultMaxStopsPerBatch,
            s.DefaultRouteOriginType.ToString(),
            s.AllowOriginSelection));
    }
}

public record UpdateDispatchSettingsCommand(UpdateDispatchSettingsRequest Body) : IRequest<Result<DispatchSettingsDto>>;

public class UpdateDispatchSettingsCommandHandler(IDispatchRepository dispatch)
    : IRequestHandler<UpdateDispatchSettingsCommand, Result<DispatchSettingsDto>>
{
    public async Task<Result<DispatchSettingsDto>> Handle(UpdateDispatchSettingsCommand request, CancellationToken ct)
    {
        var s = await dispatch.GetOrCreateSettingsAsync(ct);
        var b = request.Body;
        if (b.DefaultClusterRadiusKm is { } r) s.DefaultClusterRadiusKm = r;
        if (b.DefaultMaxStopsPerRoute is { } rs) s.DefaultMaxStopsPerRoute = rs;
        if (b.DefaultMaxStopsPerBatch is { } bs) s.DefaultMaxStopsPerBatch = bs;
        if (!string.IsNullOrWhiteSpace(b.DefaultRouteOriginType)
            && Enum.TryParse<DeliveryRouteOriginType>(b.DefaultRouteOriginType, true, out var ot))
            s.DefaultRouteOriginType = ot;
        if (b.AllowOriginSelection is { } ao) s.AllowOriginSelection = ao;
        await dispatch.UpdateSettingsAsync(s, ct);
        return Result.Ok(new DispatchSettingsDto(
            s.DefaultClusterRadiusKm,
            s.DefaultMaxStopsPerRoute,
            s.DefaultMaxStopsPerBatch,
            s.DefaultRouteOriginType.ToString(),
            s.AllowOriginSelection));
    }
}

public record GetDispatchQueueQuery(DispatchQueueFilter Filter) : IRequest<Result<PagedDispatchQueueDto>>;

public class GetDispatchQueueQueryHandler(IDispatchRepository dispatch)
    : IRequestHandler<GetDispatchQueueQuery, Result<PagedDispatchQueueDto>>
{
    public async Task<Result<PagedDispatchQueueDto>> Handle(GetDispatchQueueQuery request, CancellationToken ct)
    {
        var (items, total) = await dispatch.ListReadyQueueAsync(request.Filter, ct);
        var dtos = items.Select(o => new DispatchQueueOrderDto(
            o.Id,
            o.OrderNumber,
            o.Status.ToString(),
            o.DispatchStatus.ToString(),
            o.Total,
            o.CreatedAt,
            o.Address?.AddressText ?? DispatchBatchService.FormatAddress(o.Address),
            o.Address?.Latitude,
            o.Address?.Longitude)).ToList();
        return Result.Ok(new PagedDispatchQueueDto(dtos, total, request.Filter.Page, request.Filter.PageSize));
    }
}

public record AutoCreateDispatchBatchesCommand(AutoCreateBatchesRequest Body) : IRequest<Result<CreateBatchesResultDto>>;

public class AutoCreateDispatchBatchesCommandHandler(DispatchBatchService batches)
    : IRequestHandler<AutoCreateDispatchBatchesCommand, Result<CreateBatchesResultDto>>
{
    public Task<Result<CreateBatchesResultDto>> Handle(AutoCreateDispatchBatchesCommand request, CancellationToken ct) =>
        batches.AutoCreateBatchesAsync(request.Body, ct);
}

public record ManualCreateDispatchBatchCommand(ManualCreateBatchRequest Body) : IRequest<Result<DispatchBatchDetailDto>>;

public class ManualCreateDispatchBatchCommandHandler(DispatchBatchService batches)
    : IRequestHandler<ManualCreateDispatchBatchCommand, Result<DispatchBatchDetailDto>>
{
    public Task<Result<DispatchBatchDetailDto>> Handle(ManualCreateDispatchBatchCommand request, CancellationToken ct) =>
        batches.CreateManualBatchAsync(request.Body, ct);
}

public record ListDispatchBatchesQuery : IRequest<Result<IReadOnlyList<DispatchBatchSummaryDto>>>;

public class ListDispatchBatchesQueryHandler(IDispatchRepository dispatch)
    : IRequestHandler<ListDispatchBatchesQuery, Result<IReadOnlyList<DispatchBatchSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<DispatchBatchSummaryDto>>> Handle(ListDispatchBatchesQuery request, CancellationToken ct)
    {
        var batches = await dispatch.ListBatchesAsync(ct);
        return Result.Ok((IReadOnlyList<DispatchBatchSummaryDto>)batches.Select(RouteMapping.MapBatchSummary).ToList());
    }
}

public record GetDispatchBatchQuery(Guid BatchId) : IRequest<Result<DispatchBatchDetailDto>>;

public class GetDispatchBatchQueryHandler(IDispatchRepository dispatch)
    : IRequestHandler<GetDispatchBatchQuery, Result<DispatchBatchDetailDto>>
{
    public async Task<Result<DispatchBatchDetailDto>> Handle(GetDispatchBatchQuery request, CancellationToken ct)
    {
        var batch = await dispatch.GetBatchDetailAsync(request.BatchId, ct);
        return batch is null
            ? Result.Fail<DispatchBatchDetailDto>(AdminErrors.NotFound("Batch", request.BatchId))
            : Result.Ok(DispatchBatchService.MapBatchDetail(batch));
    }
}

public record CreateRouteFromBatchCommand(Guid BatchId, CreateRouteFromBatchRequest Body) : IRequest<Result<DeliveryRouteDetailDto>>;

public class CreateRouteFromBatchCommandHandler(RoutePlannerService planner)
    : IRequestHandler<CreateRouteFromBatchCommand, Result<DeliveryRouteDetailDto>>
{
    public Task<Result<DeliveryRouteDetailDto>> Handle(CreateRouteFromBatchCommand request, CancellationToken ct) =>
        planner.CreateRouteFromBatchAsync(request.BatchId, request.Body, ct);
}

public record ListDeliveryRoutesQuery : IRequest<Result<IReadOnlyList<DeliveryRouteSummaryDto>>>;

public class ListDeliveryRoutesQueryHandler(IDispatchRepository dispatch)
    : IRequestHandler<ListDeliveryRoutesQuery, Result<IReadOnlyList<DeliveryRouteSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<DeliveryRouteSummaryDto>>> Handle(ListDeliveryRoutesQuery request, CancellationToken ct)
    {
        var routes = await dispatch.ListRoutesAsync(ct);
        return Result.Ok((IReadOnlyList<DeliveryRouteSummaryDto>)routes.Select(RouteMapping.MapRouteSummary).ToList());
    }
}

public record GetDeliveryRouteQuery(Guid RouteId) : IRequest<Result<DeliveryRouteDetailDto>>;

public class GetDeliveryRouteQueryHandler(IDispatchRepository dispatch)
    : IRequestHandler<GetDeliveryRouteQuery, Result<DeliveryRouteDetailDto>>
{
    public async Task<Result<DeliveryRouteDetailDto>> Handle(GetDeliveryRouteQuery request, CancellationToken ct)
    {
        var route = await dispatch.GetRouteDetailAsync(request.RouteId, ct);
        return route is null
            ? Result.Fail<DeliveryRouteDetailDto>(AdminErrors.NotFound("Route", request.RouteId))
            : Result.Ok(RouteMapping.MapRouteDetail(route));
    }
}

public record AssignDeliveryRouteCommand(Guid RouteId, AssignRouteRequest Body) : IRequest<Result<DeliveryRouteDetailDto>>;

public class AssignDeliveryRouteCommandHandler(RouteAssignmentService assignment)
    : IRequestHandler<AssignDeliveryRouteCommand, Result<DeliveryRouteDetailDto>>
{
    public Task<Result<DeliveryRouteDetailDto>> Handle(AssignDeliveryRouteCommand request, CancellationToken ct) =>
        assignment.AssignAsync(request.RouteId, request.Body.DriverId, ct);
}

public record StartDeliveryRouteCommand(Guid RouteId) : IRequest<Result<DeliveryRouteDetailDto>>;

public class StartDeliveryRouteCommandHandler(RouteExecutionService execution)
    : IRequestHandler<StartDeliveryRouteCommand, Result<DeliveryRouteDetailDto>>
{
    public Task<Result<DeliveryRouteDetailDto>> Handle(StartDeliveryRouteCommand request, CancellationToken ct) =>
        execution.StartAsync(request.RouteId, ct);
}

public record FinishDeliveryRouteCommand(Guid RouteId) : IRequest<Result<DeliveryRouteDetailDto>>;

public class FinishDeliveryRouteCommandHandler(RouteExecutionService execution)
    : IRequestHandler<FinishDeliveryRouteCommand, Result<DeliveryRouteDetailDto>>
{
    public Task<Result<DeliveryRouteDetailDto>> Handle(FinishDeliveryRouteCommand request, CancellationToken ct) =>
        execution.FinishAsync(request.RouteId, ct);
}

public record MarkStopDeliveredCommand(Guid StopId) : IRequest<Result<DeliveryRouteDetailDto>>;

public class MarkStopDeliveredCommandHandler(RouteExecutionService execution)
    : IRequestHandler<MarkStopDeliveredCommand, Result<DeliveryRouteDetailDto>>
{
    public Task<Result<DeliveryRouteDetailDto>> Handle(MarkStopDeliveredCommand request, CancellationToken ct) =>
        execution.MarkStopDeliveredAsync(request.StopId, ct);
}

public record MarkStopFailedCommand(Guid StopId, FailStopRequest? Body) : IRequest<Result<DeliveryRouteDetailDto>>;

public class MarkStopFailedCommandHandler(RouteExecutionService execution)
    : IRequestHandler<MarkStopFailedCommand, Result<DeliveryRouteDetailDto>>
{
    public Task<Result<DeliveryRouteDetailDto>> Handle(MarkStopFailedCommand request, CancellationToken ct) =>
        execution.MarkStopFailedAsync(request.StopId, request.Body?.Reason, ct);
}
