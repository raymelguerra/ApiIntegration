namespace Application.Abstractions
{
    public interface IRequest<TResponse>;

    public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
    
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default);
    }
        
    public struct Unit
    {
        public static readonly Unit Value = new Unit();
    }
}