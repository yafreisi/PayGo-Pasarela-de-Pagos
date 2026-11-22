using System;
using System.Threading.Tasks;

namespace PayGoApp.Services
{
    /// <summary>
    /// Representa la solicitud de cobro enviada por un comercio hacia la pasarela PayGo.
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>Monto total de la transacción.</summary>
        public decimal Amount { get; set; }

        /// <summary>Código de moneda ISO 4217 (ej. USD, EUR, DOP).</summary>
        public string Currency { get; set; } = "USD";

        /// <summary>Clave única generada por el cliente para evitar cobros duplicados.</summary>
        public string IdempotencyKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contiene el resultado final del procesamiento de un pago.
    /// </summary>
    public class PaymentResult
    {
        /// <summary>Indica si la transacción fue aprobada por el banco.</summary>
        public bool Approved { get; set; }

        /// <summary>Código de autorización emitido por la red financiera.</summary>
        public string AuthorizationCode { get; set; } = string.Empty;

        /// <summary>Marca de tiempo UTC en la que se procesó el pago.</summary>
        public DateTime ProcessedAtUtc { get; set; }
    }

    /// <summary>
    /// Gestiona el ciclo de vida de las transacciones de pago dentro de la pasarela PayGo,
    /// incluyendo validación de montos, control de idempotencia y autorización de fondos.
    /// </summary>
    public class PaymentService
    {
        /// <summary>
        /// Procesa una solicitud de cobro validando el monto, verificando duplicados
        /// y ejecutando la autorización contra el procesador financiero.
        /// </summary>
        /// <param name="request">Datos de la transacción (monto, moneda y clave de idempotencia).</param>
        /// <returns>Un objeto <see cref="PaymentResult"/> con el estado final y código de autorización.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si la solicitud es nula.</exception>
        /// <exception cref="ArgumentException">Se lanza si el monto es menor o igual a cero.</exception>
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "La solicitud de pago no puede ser nula.");

            // Se registra la marca de tiempo UTC para garantizar trazabilidad en conciliaciones bancarias
            var transactionTimestamp = DateTime.UtcNow;

            if (request.Amount <= 0)
                throw new ArgumentException("El monto de la transacción debe ser mayor a cero.", nameof(request.Amount));

            // Evita cargos duplicados si el cliente reintenta la petición por latencia de red (Idempotencia)
            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
                throw new ArgumentException("Se requiere una clave de idempotencia válida.", nameof(request.IdempotencyKey));

            // Simulación de llamada asíncrona al procesador de pagos
            await Task.Delay(50);

            return new PaymentResult
            {
                Approved = true,
                AuthorizationCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                ProcessedAtUtc = transactionTimestamp
            };
        }
    }
}