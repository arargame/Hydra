using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.DI.HttpContextDI
{
    public interface IHttpContextAccessorAbstraction
    {
        //ClaimsPrincipal User { get; }
        string? GetHeader(string name);
        string? GetClientIpAddress();
        HttpContext HttpContext { get; }
    }

    public class HttpContextAccessorWrapper : IHttpContextAccessorAbstraction
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAccessorWrapper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

        public HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public string? GetHeader(string name)
        {
            return _httpContextAccessor.HttpContext?.Request.Headers[name];
        }

        public string? GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }
    }

}
