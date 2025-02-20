using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Google.Rpc;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcRichError;
using Microsoft.Extensions.Logging;

namespace Arc4u.gRPC.Interceptors;

public class AuthorizationInterceptor : Interceptor
{
    public AuthorizationInterceptor(ILogger<AuthorizationInterceptor> logger,
                                    IApplicationContext applicationContext,
                                    GrpcMethodInfo grpcMethodInfo)
    {
        _logger = logger;
        _applicationContext = applicationContext;
        _grpcMethodInfo = grpcMethodInfo;
    }

    const string appSettings = "AppSettings";

    private readonly IApplicationContext _applicationContext;
    private readonly ILogger<AuthorizationInterceptor> _logger;
    private readonly GrpcMethodInfo _grpcMethodInfo;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        SetCultureIfExist(context);
        SetActivityIDIfExist(context);

        var targetType = continuation.Target!.GetType();

        // Get the service type for the service we're calling a method in.
        var serviceType = targetType.GenericTypeArguments[0];

        var serviceAspect = _grpcMethodInfo.GetAttributeFor(context.Method, serviceType);

        if (null == _applicationContext.Principal)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.Unauthenticated, "No user context."));
        }
        // if no service aspect is defined an empty one was generated by the GrpcMethodInfo => skip in this case the check.
        if (serviceAspect.Operations.Length > 0 && !_applicationContext.Principal.IsAuthorized(serviceAspect.Scope, serviceAspect.Operations))
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.PermissionDenied, "You don't have the expected rights."));
        }

        try
        {
            return await continuation(request, context).ConfigureAwait(false);
        }
        catch (AppException ae)
        {
            throw new Google.Rpc.Status
            {
                Code = (int)StatusCode.Internal,
                Message = appSettings,
                Details =
                {
                    new ErrorInfo
                    {
                        Reason = JsonSerializer.Serialize(ae.Messages)
                    }
                }
            }.ToException();
        }
        catch (RpcException rcp)
        {
            _logger.Technical().Exception(rcp).Log();
            throw;
        }
        catch (Exception ex)
        {
            _logger.Technical().Exception(ex).Log();
            throw new RpcException(new Grpc.Core.Status(StatusCode.Internal, "An error occurs."));
        }
    }

    private void SetCultureIfExist(ServerCallContext context)
    {
        // Culture and ActivityID was injected?
        var cultureEntry = context.RequestHeaders.Get("culture");
        if (null != cultureEntry && !cultureEntry.IsBinary && null != _applicationContext.Principal)
        {
            try
            {
                var culture = new CultureInfo(cultureEntry.Value);

                Threading.Culture.SetCulture(culture);
                _applicationContext.Principal.Profile.CurrentCulture = culture;
            }
            catch (ArgumentNullException) { }
            catch (CultureNotFoundException) { }
        }
    }

    private void SetActivityIDIfExist(ServerCallContext _)
    {
        _applicationContext.ActivityID = Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        SetCultureIfExist(context);
        SetActivityIDIfExist(context);

        var targetType = continuation.Target!.GetType();

        // Get the service type for the service we're calling a method in.
        var serviceType = targetType.GenericTypeArguments[0];

        var serviceAspect = _grpcMethodInfo.GetAttributeFor(context.Method, serviceType);

        if (null == _applicationContext.Principal)
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.Unauthenticated, "No user context."));
        }

        if (serviceAspect.Operations.Length > 0 && !_applicationContext.Principal.IsAuthorized(serviceAspect.Scope, serviceAspect.Operations))
        {
            throw new RpcException(new Grpc.Core.Status(StatusCode.PermissionDenied, "You don't have the expected rights."));
        }

        try
        {
            await continuation(request, responseStream, context).ConfigureAwait(false);
        }
        catch (AppException ae)
        {
            throw new Google.Rpc.Status
            {
                Code = (int)StatusCode.Internal,
                Message = appSettings,
                Details =
                {
                    new ErrorInfo
                    {
                        Reason = JsonSerializer.Serialize(ae.Messages)
                    }
                }
            }.ToException();
        }
        catch (RpcException rcp)
        {
            _logger.Technical().Exception(rcp).Log();
            throw;
        }
        catch (Exception ex)
        {
            _logger.Technical().Exception(ex).Log();
            throw new RpcException(new Grpc.Core.Status(StatusCode.Internal, "An error occurs."));
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {

        if (!context.Method.StartsWith("/grpc.reflection", StringComparison.InvariantCultureIgnoreCase))
        {
            SetCultureIfExist(context);
            SetActivityIDIfExist(context);

            var targetType = continuation.Target!.GetType();

            // Get the service type for the service we're calling a method in.
            var serviceType = targetType.GenericTypeArguments[0];

            var serviceAspect = _grpcMethodInfo.GetAttributeFor(context.Method, serviceType);

            if (null == _applicationContext.Principal)
            {
                throw new RpcException(new Grpc.Core.Status(StatusCode.Unauthenticated, "No user context."));
            }

            if (serviceAspect.Operations.Length > 0 && !_applicationContext.Principal.IsAuthorized(serviceAspect.Scope, serviceAspect.Operations))
            {
                throw new RpcException(new Grpc.Core.Status(StatusCode.PermissionDenied, "You don't have the expected rights."));
            }
        }

        try
        {
            await continuation(requestStream, responseStream, context).ConfigureAwait(false);
        }
        catch (AppException ae)
        {
            throw new Google.Rpc.Status
            {
                Code = (int)StatusCode.Internal,
                Message = appSettings,
                Details =
                {
                    new ErrorInfo
                    {
                        Reason = JsonSerializer.Serialize(ae.Messages)
                    }
                }
            }.ToException();
        }
        catch (RpcException rcp)
        {
            _logger.Technical().Exception(rcp).Log();
            throw;
        }
        catch (Exception ex)
        {
            _logger.Technical().Exception(ex).Log();
            throw new RpcException(new Grpc.Core.Status(StatusCode.Internal, "An error occurs."));
        }
    }

}
