# Payment Microservice

## Overview

The Payment microservice is a production-ready service built with Clean Architecture, CQRS, and event-driven architecture principles. It handles payment processing with idempotency guarantees, simulated external payment gateway integration, and comprehensive event publishing for saga orchestration.

## Architecture

### Clean Architecture Layers

```
Payment.API/                    # Presentation Layer
├── Controllers/               # REST API endpoints
├── Middleware/                # Global exception handling
└── Program.cs                 # DI configuration

Payment.Application/            # Application Layer
├── Common/
│   ├── Behaviors/            # MediatR pipeline behaviors
│   └── Interfaces/           # Abstractions
├── Payments/
│   ├── Commands/             # Write operations (CQRS)
│   ├── Queries/              # Read operations (CQRS)
│   ├── EventHandlers/        # Event consumers
│   └── DTOs/                 # Data transfer objects
└── Validators/               # FluentValidation validators

Payment.Infrastructure/         # Infrastructure Layer
├── Persistence/
│   ├── PaymentDbContext.cs   # EF Core DbContext
│   └── Configurations/       # Entity configurations
├── Services/
│   ├── SimulatedPaymentGateway.cs  # External gateway simulation
│   └── InMemoryEventPublisher.cs   # Event publishing
└── Migrations/               # EF Core migrations

Payment.Domain/                 # Domain Layer
├── Entities/
│   ├── PaymentTransaction.cs # Payment aggregate root
│   └── ProcessedEvent.cs     # Idempotency tracking
├── Enums/                    # Domain enumerations
└── Events/                   # Domain events
```

### Key Design Patterns

1. **Clean Architecture**: Dependency inversion, separation of concerns
2. **CQRS**: Command Query Responsibility Segregation using MediatR
3. **Event-Driven Architecture**: Consumes `OrderCreatedEvent`, publishes `PaymentSucceededEvent`/`PaymentFailedEvent`
4. **Idempotency Pattern**: `ProcessedEvent` table prevents duplicate event processing
5. **Gateway Pattern**: `IPaymentGateway` abstraction for external payment providers
6. **Repository Pattern**: DbContext as unit of work
7. **Pipeline Behavior**: Validation, logging via MediatR behaviors

## Core Features

### 1. Event-Driven Payment Processing

**Event Flow:**
```
OrderCreatedEvent → OrderCreatedEventHandler → ProcessPaymentCommand → SimulatedPaymentGateway
    ↓                                              ↓
ProcessedEvent (idempotency)          PaymentSucceededEvent / PaymentFailedEvent
```

**Idempotency Guarantee:**
- Each event has a unique `EventId`
- `ProcessedEvents` table tracks processed event IDs
- Database unique constraint ensures no duplicate processing
- Prevents double-charging customers even if events are duplicated

### 2. Payment State Machine

```
Pending → Processing → Completed
                   ↓
                 Failed
```

**State Transitions:**
- **Pending**: Initial state when payment is created
- **Processing**: Payment submitted to gateway
- **Completed**: Payment successfully processed
- **Failed**: Payment declined or error occurred

**Controlled State Transitions:**
```csharp
public void MarkAsProcessing()
{
    if (Status != PaymentStatus.Pending)
        throw new InvalidOperationException($"Cannot process payment with status {Status}");
    Status = PaymentStatus.Processing;
}
```

### 3. Simulated Payment Gateway

**SimulatedPaymentGateway.cs** provides realistic external system behavior:

- **80% Success Rate**: Simulates real-world approval rates
- **Random Delays**: 100-500ms to simulate network latency
- **Transaction IDs**: Generates realistic transaction references
- **Error Messages**: Returns appropriate decline reasons
- **Comprehensive Logging**: Full audit trail of gateway interactions

**Sample Output:**
```
Payment Gateway Request: Amount=$149.99, Currency=USD, OrderId=abc-123
Payment Gateway Response: Success, TransactionId=TXN-20260422-abc123, ProcessingTime=342ms
```

### 4. CQRS Commands & Queries

**Commands (Write Operations):**
- `ProcessPaymentCommand`: Process payment with external gateway
  - Validates amount, currency, order ID
  - Calls payment gateway
  - Updates payment status
  - Publishes success/failure events

**Queries (Read Operations):**
- `GetPaymentByOrderIdQuery`: Retrieve payment details for an order
  - Efficient read-only operation
  - Returns PaymentResponseDto
  - 404 if payment not found

### 5. Comprehensive Validation

**FluentValidation Rules:**
```csharp
RuleFor(x => x.OrderId)
    .NotEmpty().WithMessage("Order ID is required")
    .Must(BeValidGuid).WithMessage("Order ID must be a valid GUID");

RuleFor(x => x.Amount)
    .GreaterThan(0).WithMessage("Amount must be greater than 0")
    .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed 1,000,000");
```

**Validation Pipeline:**
- Executes before command handlers via `ValidationBehavior`
- Returns 400 Bad Request with detailed error messages
- Prevents invalid data from reaching domain logic

## API Endpoints

### GET /api/payments/order/{orderId}

Retrieve payment details for a specific order.

**Request:**
```http
GET /api/payments/order/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "amount": 149.99,
  "currency": "USD",
  "status": "Completed",
  "gatewayTransactionId": "TXN-20260422-abc123",
  "processedAt": "2026-04-22T12:30:00Z"
}
```

**Response (404 Not Found):**
```json
{
  "type": "NotFoundException",
  "message": "Payment for order ID '550e8400-e29b-41d4-a716-446655440000' not found",
  "details": []
}
```

## Database Schema

### PaymentTransactions Table

| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | Primary key |
| OrderId | uniqueidentifier | Reference to order (indexed) |
| UserId | uniqueidentifier | Customer who made payment |
| Amount | decimal(18,2) | Payment amount |
| Currency | nvarchar(3) | ISO currency code (USD, EUR, etc.) |
| Status | int | Payment status enum |
| GatewayTransactionId | nvarchar(100) | External gateway transaction ID |
| ProcessedAt | datetime2 | When payment was processed |
| ErrorMessage | nvarchar(500) | Error details if failed |
| CreatedAt | datetime2 | Record creation timestamp |
| UpdatedAt | datetime2 | Last update timestamp |

**Indexes:**
- Primary key on `Id`
- Non-clustered index on `OrderId` for fast lookups

### ProcessedEvents Table

| Column | Type | Description |
|--------|------|-------------|
| Id | uniqueidentifier | Primary key |
| EventId | uniqueidentifier | Unique event identifier (unique constraint) |
| EventType | nvarchar(100) | Event type name |
| ProcessedAt | datetime2 | Processing timestamp |
| PaymentId | uniqueidentifier | Related payment (nullable) |

**Constraints:**
- **Unique constraint** on `EventId` ensures idempotency
- Insert will fail if duplicate event is processed

## Event Integration

### Consumed Events

**OrderCreatedEvent**
```csharp
public class OrderCreatedEvent : INotification
{
    public Guid EventId { get; set; }      // For idempotency
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Handler Logic:**
1. Check if `EventId` already processed (idempotency)
2. Create `ProcessPaymentCommand`
3. Execute payment processing
4. Mark event as processed
5. Publish success/failure event

### Published Events

**PaymentSucceededEvent**
```csharp
{
    "PaymentId": "123e4567-e89b-12d3-a456-426614174000",
    "OrderId": "550e8400-e29b-41d4-a716-446655440000",
    "Amount": 149.99,
    "Currency": "USD",
    "GatewayTransactionId": "TXN-20260422-abc123",
    "ProcessedAt": "2026-04-22T12:30:00Z"
}
```

**PaymentFailedEvent**
```csharp
{
    "PaymentId": "123e4567-e89b-12d3-a456-426614174000",
    "OrderId": "550e8400-e29b-41d4-a716-446655440000",
    "Amount": 149.99,
    "Reason": "Insufficient funds"
}
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PaymentDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Dependency Injection (Program.cs)

```csharp
// Database
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(connectionString, 
        b => b.MigrationsAssembly("Payment.Infrastructure")));

// Application services
builder.Services.AddScoped<IPaymentDbContext>(provider => 
    provider.GetRequiredService<PaymentDbContext>());

// Payment gateway (simulated)
builder.Services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();

// Event publishing
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

// MediatR with validation
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Payment.Application.Common.Behaviors.ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ProcessPaymentCommandValidator).Assembly);
```

## Running the Service

### 1. Update Database

```bash
cd src/Services/Payment/Payment.API
dotnet ef database update
```

This creates:
- `PaymentTransactions` table
- `ProcessedEvents` table
- Necessary indexes and constraints

### 2. Run the Application

```bash
dotnet run
```

Application starts on: `https://localhost:7003` (or configured port)

### 3. Access Swagger UI

Navigate to: `https://localhost:7003/swagger`

## Testing Scenarios

### Scenario 1: Process Payment via Event

**Simulate OrderCreatedEvent:**
```csharp
var orderEvent = new OrderCreatedEvent
{
    EventId = Guid.NewGuid(),
    OrderId = Guid.NewGuid(),
    UserId = Guid.NewGuid(),
    TotalAmount = 149.99m,
    ItemCount = 3,
    CreatedAt = DateTime.UtcNow
};

await _mediator.Publish(orderEvent);
```

**Expected Outcome:**
- Payment created in `Pending` status
- Payment gateway called
- Status updated to `Completed` or `Failed`
- Event published to downstream services
- `ProcessedEvent` record created

### Scenario 2: Idempotency Test

**Send same event twice:**
```csharp
var eventId = Guid.NewGuid();

// First call - processes successfully
await _mediator.Publish(new OrderCreatedEvent { EventId = eventId, ... });

// Second call - skipped due to idempotency
await _mediator.Publish(new OrderCreatedEvent { EventId = eventId, ... });
```

**Expected Outcome:**
- First call: Payment processed, event published
- Second call: Logs "Event already processed", no action taken
- Only ONE payment record in database

### Scenario 3: Query Payment

**HTTP Request:**
```bash
curl -X GET https://localhost:7003/api/payments/order/{orderId}
```

**Expected Response:**
```json
{
  "id": "...",
  "orderId": "...",
  "status": "Completed",
  "amount": 149.99,
  "gatewayTransactionId": "TXN-..."
}
```

## Error Handling

### Global Exception Middleware

**ExceptionHandlingMiddleware** provides consistent error responses:

**ValidationException → 400 Bad Request:**
```json
{
  "type": "ValidationException",
  "message": "One or more validation errors occurred",
  "details": [
    {
      "propertyName": "Amount",
      "errorMessage": "Amount must be greater than 0",
      "attemptedValue": -10
    }
  ]
}
```

**NotFoundException → 404 Not Found:**
```json
{
  "type": "NotFoundException",
  "message": "Payment for order ID '...' not found",
  "details": []
}
```

**Unhandled Exceptions → 500 Internal Server Error:**
```json
{
  "type": "InternalServerError",
  "message": "An unexpected error occurred",
  "details": []
}
```

## Logging

### Structured Logging Examples

```csharp
_logger.LogInformation(
    "Processing payment for Order {OrderId}, Amount: {Amount} {Currency}",
    command.OrderId, command.Amount, command.Currency);

_logger.LogWarning(
    "Event {EventId} of type {EventType} has already been processed",
    notification.EventId, nameof(OrderCreatedEvent));

_logger.LogError(
    "Payment processing failed for Order {OrderId}: {ErrorMessage}",
    command.OrderId, result.ErrorMessage);
```

### Log Correlation

All logs include:
- Timestamp
- Log level
- Category (class name)
- Structured parameters
- Exception details (if applicable)

## Production Considerations

### 1. Replace Simulated Gateway

**Current:**
```csharp
builder.Services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();
```

**Production:**
```csharp
builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();
// or
builder.Services.AddScoped<IPaymentGateway, BraintreePaymentGateway>();
```

Implement `IPaymentGateway` interface with real provider SDK.

### 2. Replace InMemoryEventPublisher

**Current:**
```csharp
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
```

**Production (RabbitMQ):**
```csharp
builder.Services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
```

**Production (Azure Service Bus):**
```csharp
builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
```

### 3. Add Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentDbContext>()
    .AddCheck<PaymentGatewayHealthCheck>("payment_gateway");
```

### 4. Add Distributed Tracing

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("Payment.API")
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation());
```

### 5. Implement Circuit Breaker

Use Polly for resilient external calls:
```csharp
services.AddHttpClient<IPaymentGateway, StripePaymentGateway>()
    .AddTransientHttpErrorPolicy(p => 
        p.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
```

### 6. Add Monitoring & Metrics

- Payment success/failure rates
- Payment processing duration
- Gateway response times
- Event processing lag
- Idempotency cache hit rate

## NuGet Packages

### Payment.Domain
- `MediatR.Contracts` (2.0.1) - Event abstractions

### Payment.Application
- `MediatR` (12.2.0) - CQRS mediator
- `FluentValidation` (11.9.0) - Input validation
- `FluentValidation.DependencyInjectionExtensions` (11.9.0)
- `Microsoft.Extensions.Logging.Abstractions` (8.0.0)
- `Microsoft.EntityFrameworkCore` (8.0.0)

### Payment.Infrastructure
- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)

### Payment.API
- `Microsoft.AspNetCore.OpenApi` (8.0.0)
- `Swashbuckle.AspNetCore` (6.5.0)
- `MediatR` (12.2.0)
- `FluentValidation.AspNetCore` (11.3.0)
- `Microsoft.EntityFrameworkCore.Design` (8.0.0)

## Integration with Other Services

### Order Service → Payment Service
1. Order service creates order
2. Publishes `OrderCreatedEvent` to message broker
3. Payment service consumes event
4. Processes payment with gateway
5. Publishes `PaymentSucceededEvent` or `PaymentFailedEvent`

### Payment Service → Order Service
1. Payment completes successfully
2. Payment service publishes `PaymentSucceededEvent`
3. Order service consumes event
4. Updates order status to `Paid`
5. Triggers fulfillment workflow

### Saga Orchestration Example

```
[Order Service]         [Payment Service]       [Inventory Service]
      |                        |                        |
   Create Order                |                        |
      |--OrderCreatedEvent---->|                        |
      |                   Process Payment              |
      |<--PaymentSucceededEvent|                        |
   Update to "Paid"            |                        |
      |------------------------ReserveInventoryEvent--->|
      |                        |                  Reserve Items
      |<-----------------------InventoryReservedEvent---|
   Update to "Reserved"        |                        |
```

## License

This service is part of the ecommerce-platform project.

---

**Built with:** .NET 8.0, Clean Architecture, CQRS, Event-Driven Architecture, Entity Framework Core, MediatR, FluentValidation
