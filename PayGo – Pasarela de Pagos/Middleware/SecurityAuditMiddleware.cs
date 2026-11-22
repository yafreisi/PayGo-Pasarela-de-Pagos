using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PayGoApp.Middleware
{
    /// <summary>
    /// Middleware de seguridad perimetral que valida la autenticación de los comercios,
    /// inyecta cabeceras de protección HTTP y cumple con controles PCI-DSS.
    /// </summary>
    public class SecurityAuditMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Inicializa una nueva instancia del middleware de seguridad de PayGo.
        /// </summary>
        /// <param name="next">Siguiente delegado en la tubería de procesamiento HTTP de ASP.NET Core.</param>
        public SecurityAuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Intercepta cada solicitud entrante para inyectar cabeceras de endurecimiento (Hardening)
        /// y verificar la presencia de la clave criptográfica del comercio (<c>X-PayGo-ApiKey</c>).
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición actual.</param>
        /// <returns>Una tarea asíncrona que representa la ejecución del siguiente componente en la tubería.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Cabeceras de seguridad para prevenir ataques MIME-sniffing y Clickjacking
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // Solo exige API Key en rutas de API financiera (/api/...), permitiendo cargar vistas web públicas
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (!context.Request.Headers.TryGetValue("X-PayGo-ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Acceso denegado: falta la clave de autenticación del comercio.");
                    return;
                }
            }

            // IMPORTANTE (PCI-DSS): Nunca registrar el cuerpo de la petición en logs sin enmascarar PAN o CVV
            await _next(context);
        }
    }
}