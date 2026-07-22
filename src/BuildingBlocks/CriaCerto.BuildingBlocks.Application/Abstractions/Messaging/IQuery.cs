using CriaCerto.BuildingBlocks.Abstractions.Results;
using MediatR;

namespace CriaCerto.BuildingBlocks.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;