using Microsoft.Extensions.DependencyInjection;

namespace Application.Abstractions
{
    public class Sender(IServiceProvider serviceProvider) : ISender
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            dynamic handler = serviceProvider.GetRequiredService(handlerType);

            return await handler.Handle((dynamic)request, cancellationToken);
        }
    }
}