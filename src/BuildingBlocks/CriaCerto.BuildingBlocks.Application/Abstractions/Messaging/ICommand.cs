using CriaCerto.BuildingBlocks.Abstractions.Results;
using MediatR;

namespace CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;