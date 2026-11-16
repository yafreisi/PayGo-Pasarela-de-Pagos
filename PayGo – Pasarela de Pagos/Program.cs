using System;
using System.Collections.Generic;

namespace PayGoApp
{
    // ==========================================
    // 1. ESTRUCTURA DE LA TRANSACCIÓN
    // ==========================================
    public class Transaction
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }

        public Transaction(decimal amount, string currency, string paymentMethod)
        {
            TransactionId = Guid.NewGuid().ToString();
            Amount = amount;
            Currency = currency;
            PaymentMethod = paymentMethod;
            Status = "PENDING";
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] ID: {TransactionId} | {Amount} {Currency} | Método: {PaymentMethod} | Estado: {Status}";
        }
    }

    // ==========================================
    // 2. RESPONSABILIDAD ÚNICA: VALIDACIÓN
    // ==========================================
    public class PaymentValidator
    {
        // Única responsabilidad: Validar que los datos del pago cumplan las reglas de negocio
        public bool Validate(decimal amount, string currency)
        {
            if (amount <= 0) return false;
            if (string.IsNullOrWhiteSpace(currency)) return false;
            return true;
        }
    }

    // ==========================================
    // 3. RESPONSABILIDAD ÚNICA: REPOSITORIO (DATOS)
    // ==========================================
    public class PaymentRepository
    {
        // Única responsabilidad: Gestionar el almacenamiento y la recuperación del historial
        private readonly List<Transaction> _database = new List<Transaction>();

        public void Save(Transaction transaction)
        {
            _database.Add(transaction);
        }

        public List<Transaction> GetAll()
        {
            return _database;
        }
    }

    // ==========================================
    // 4. RESPONSABILIDAD ÚNICA: PROCESAMIENTO
    // ==========================================
    public class PaymentProcessor
    {
        private readonly PaymentValidator _validator;
        private readonly PaymentRepository _repository;

        public PaymentProcessor(PaymentValidator validator, PaymentRepository repository)
        {
            _validator = validator;
            _repository = repository;
        }

        public Transaction ExecutePayment(decimal amount, string currency, string paymentMethod)
        {
            var transaction = new Transaction(amount, currency, paymentMethod);

            // 1. Validar usando el servicio especializado
            bool isValid = _validator.Validate(amount, currency);

            if (!isValid)
            {
                transaction.Status = "REJECTED_INVALID_DATA";
            }
            else
            {
                // 2. Simular aprobación si pasa la validación
                transaction.Status = "APPROVED";
            }

            // 3. Guardar usando el repositorio especializado
            _repository.Save(transaction);

            return transaction;
        }
    }

    // ==========================================
    // 5. EJECUCIÓN INTERACTIVA PRINCIPAL
    // ==========================================
    internal class Program
    {
        static void Main(string[] args)
        {
            var validator = new PaymentValidator();
            var repository = new PaymentRepository();
            var processor = new PaymentProcessor(validator, repository);

            bool continuar = true;

            Console.WriteLine("==========================================");
            Console.WriteLine("    PAYGO - PASARELA DE PAGOS (CON SRP)   ");
            Console.WriteLine("==========================================\n");

            while (continuar)
            {
                // 1. SELECCIÓN DEL MÉTODO DE PAGO
                Console.WriteLine("--- ELIJA EL MÉTODO DE PAGO ---");
                Console.WriteLine("1. TARJETA DE CRÉDITO");
                Console.WriteLine("2. TRANSFERENCIA BANCARIA");
                Console.WriteLine("3. PAYPAL");
                Console.Write("Seleccione una opción (1-3): ");

                string opcionMetodo = Console.ReadLine();
                string paymentMethod = "";

                switch (opcionMetodo)
                {
                    case "1":
                        paymentMethod = "TARJETA DE CRÉDITO";
                        break;
                    case "2":
                        paymentMethod = "TRANSFERENCIA BANCARIA";
                        break;
                    case "3":
                        paymentMethod = "PAYPAL";
                        break;
                    default:
                        Console.WriteLine("[! Error] Opción de método de pago inválida. Intente de nuevo.\n");
                        continue;
                }

                // 2. SELECCIÓN DEL TIPO DE MONEDA
                Console.WriteLine("\n--- ELIJA EL TIPO DE MONEDA ---");
                Console.WriteLine("1. USD");
                Console.WriteLine("2. EUR");
                Console.WriteLine("3. DOP");
                Console.Write("Seleccione una opción (1-3): ");

                string opcionMoneda = Console.ReadLine();
                string currency = "";

                switch (opcionMoneda)
                {
                    case "1":
                        currency = "USD";
                        break;
                    case "2":
                        currency = "EUR";
                        break;
                    case "3":
                        currency = "DOP";
                        break;
                    default:
                        Console.WriteLine("[! Error] Opción de moneda inválida. Intente de nuevo.\n");
                        continue;
                }

                // 3. SOLICITAR EL MONTO
                Console.Write("\nIngrese el monto a pagar: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    Console.WriteLine("[! Error] Monto no válido.\n");
                    continue;
                }

                // Procesamos el pago a través del servicio dedicado
                var result = processor.ExecutePayment(amount, currency, paymentMethod);

                Console.WriteLine($"\n[Resultado]: {result}");
                Console.WriteLine("------------------------------------------\n");

                Console.Write("¿Desea realizar otro pago? (s/n): ");
                if (Console.ReadLine().Trim().ToLower() != "s")
                {
                    continuar = false;
                }
                Console.WriteLine();
            }

            Console.WriteLine("\n==========================================");
            Console.WriteLine("       HISTORIAL GENERAL DE PAGOS         ");
            Console.WriteLine("==========================================");

            foreach (var record in repository.GetAll())
            {
                Console.WriteLine(record);
            }

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}