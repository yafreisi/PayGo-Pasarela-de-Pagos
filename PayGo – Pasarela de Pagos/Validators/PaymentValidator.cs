using System;
using System.Linq;

namespace PayGoApp.Validators
{
    /// <summary>
    /// Proporciona reglas de validación sintáctica y financiera para los instrumentos
    /// de pago ingresados en la pasarela PayGo antes de su tokenización.
    /// </summary>
    public static class PaymentValidator
    {
        /// <summary>
        /// Verifica la validez estructural de un número de tarjeta mediante el algoritmo de Luhn.
        /// </summary>
        /// <param name="cardNumber">Número de tarjeta sin espacios ni guiones (PAN).</param>
        /// <returns><c>true</c> si el dígito de control es válido; de lo contrario, <c>false</c>.</returns>
        public static bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || !cardNumber.All(char.IsDigit))
                return false;

            // El estándar ISO/IEC 7812 establece longitudes entre 13 y 19 dígitos para tarjetas bancarias
            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            int sum = 0;
            bool alternate = false;

            // Recorrido de derecha a izquierda duplicando cada segundo dígito según especificación Luhn
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }
                sum += digit;
                alternate = !alternate;
            }

            return (sum % 10 == 0);
        }

        /// <summary>
        /// Valida que la fecha de expiración de la tarjeta no se encuentre vencida respecto a la fecha UTC actual.
        /// </summary>
        /// <param name="month">Mes de expiración (1 a 12).</param>
        /// <param name="year">Año de expiración en formato de cuatro dígitos (ej. 2028).</param>
        /// <returns><c>true</c> si la tarjeta está vigente; de lo contrario, <c>false</c>.</returns>
        public static bool IsExpirationDateValid(int month, int year)
        {
            if (month < 1 || month > 12 || year < DateTime.UtcNow.Year)
                return false;

            // La tarjeta es válida hasta el último día del mes indicado
            var lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc);
            return lastDayOfMonth >= DateTime.UtcNow;
        }
    }
}