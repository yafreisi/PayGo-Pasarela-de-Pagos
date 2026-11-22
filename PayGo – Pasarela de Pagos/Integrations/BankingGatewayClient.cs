using System;
using System.Threading;
using System.Threading.Tasks;

namespace PayGoApp.Integrations
{
    /// <summary>
    /// Representa la respuesta normalizada recibida desde el banco adquirente bajo estándar ISO 8583.
    /// </summary>
    public class BankResponse
    {
        /// <summary>Código ISO 8583 (ej. "00" = Aprobada, "51" = Fondos insuficientes).</summary>
        public string IsoCode { get; set; } = "00";

        /// <summary>Descripción legible del estado devuelto por la red interbancaria.</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cliente encargado de la comunicación segura con el procesador adquirente
    /// y las redes interbancarias externas (Visa / Mastercard).
    /// </summary>
    public class BankingGatewayClient
    {
        /// <summary>
        /// Envía una trama de autorización cifrada al banco adquirente y traduce
        /// el código de respuesta bancario al modelo interno de PayGo.
        /// </summary>
        /// <param name="tokenizedPayload">Carga útil cifrada con los datos tokenizados del pago.</param>
        /// <param name="cancellationToken">Token para abortar la operación si se excede el SLA bancario.</param>
        /// <returns>Respuesta normalizada del banco con código ISO 8583.</returns>
        /// <exception cref="ArgumentException">Se lanza si la carga útil cifrada está vacía.</exception>
        /// <exception cref="TimeoutException">Se produce si el banco no responde dentro de los 15 segundos reglamentarios.</exception>
        public async Task<BankResponse> SendAuthorizationAsync(string tokenizedPayload, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tokenizedPayload))
                throw new ArgumentException("La trama cifrada no puede estar vacía.", nameof(tokenizedPayload));

            // Se exige control de tiempo de espera de 15 segundos para cumplir con los SLA interbancarios
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                // Simulación de transmisión segura bajo TLS 1.3 hacia el nodo adquirente
                await Task.Delay(100, cts.Token);

                // Si el banco devuelve "05" (Do Not Honor) o "51" (Fondos insuficientes),
                // no se debe reintentar automáticamente para evitar bloqueos antifraude
                return new BankResponse
                {
                    IsoCode = "00",
                    Message = "Transacción aprobada por el banco emisor."
                };
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Tiempo de espera agotado al comunicarse con el procesador bancario.");
            }
        }
    }
}