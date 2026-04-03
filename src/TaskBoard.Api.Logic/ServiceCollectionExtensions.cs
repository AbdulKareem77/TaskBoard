using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.Api.Logic.Mappings;
using TaskBoard.Api.Logic.Shared.Authorization;
using TaskBoard.Api.Logic.Shared.ErrorHandling;
using TaskBoard.Api.Logic.Shared.Validation;

namespace TaskBoard.Api.Logic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiLogic(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        services.AddAutoMapper(typeof(TaskBoardMappingProfile).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        return services;
    }
}
