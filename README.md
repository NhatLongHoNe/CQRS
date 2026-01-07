# Tóm Tắt Kiến Thức Dự Án CQRS

## 📋 Mục Lục
1. [Tổng Quan Dự Án](#tổng-quan-dự-án)
2. [Kiến Trúc CQRS](#kiến-trúc-cqrs)
3. [Cấu Trúc Thư Mục](#cấu-trúc-thư-mục)
4. [Các Thành Phần Chính](#các-thành-phần-chính)
5. [Luồng Xử Lý](#luồng-xử-lý)
6. [Công Nghệ Sử Dụng](#công-nghệ-sử-dụng)
7. [API Endpoints](#api-endpoints)
8. [Patterns & Best Practices](#patterns--best-practices)

---

## 🎯 Tổng Quan Dự Án

Dự án này là một **Orders API** được xây dựng theo kiến trúc **CQRS (Command Query Responsibility Segregation)** với các đặc điểm:

- **Tách biệt Read/Write Database**: Sử dụng 2 database riêng biệt cho đọc và ghi
- **Event-Driven Architecture**: Sử dụng events để đồng bộ dữ liệu giữa Write DB và Read DB
- **MediatR Pattern**: Sử dụng MediatR để triển khai CQRS pattern
- **Validation**: Sử dụng FluentValidation để validate dữ liệu đầu vào
- **Projections**: Tự động cập nhật Read Database khi có event mới

---

## 🏗️ Kiến Trúc CQRS

### CQRS Pattern là gì?

**CQRS (Command Query Responsibility Segregation)** là một pattern tách biệt việc đọc (Query) và việc ghi (Command) thành 2 luồng xử lý riêng biệt:

- **Commands**: Thay đổi trạng thái của hệ thống (Create, Update, Delete)
- **Queries**: Chỉ đọc dữ liệu, không thay đổi trạng thái

### Lợi ích của CQRS:

1. **Tối ưu hiệu suất**: Read DB có thể được tối ưu riêng cho việc đọc (indexes, denormalization)
2. **Scalability**: Có thể scale Read và Write DB độc lập
3. **Separation of Concerns**: Tách biệt logic đọc và ghi rõ ràng
4. **Flexibility**: Có thể sử dụng các công nghệ khác nhau cho Read và Write

---

## 📁 Cấu Trúc Thư Mục

```
CQRS/
├── Commands/              # Command objects (thay đổi trạng thái)
│   ├── CreateOrderCommand.cs
│   └── CreateOrderCommandValidator.cs
├── Queries/               # Query objects (đọc dữ liệu)
│   ├── GetOrderByIdQuery.cs
│   └── GetOrderSummariesQuery.cs
├── Handlers/              # Xử lý Commands và Queries
│   ├── CreateOrderCommandHandler.cs
│   ├── GetOrderByIdQueryHandler.cs
│   ├── GetOrderSummariesQueryHandler.cs
│   ├── ICommandHandler.cs
│   └── IQueryHandler.cs
├── Events/                # Event objects và publishers
│   ├── OrderCreatedEvent.cs
│   ├── IEventPublisher.cs
│   ├── IEventHandler.cs
│   ├── InProcessEventPublisher.cs
│   └── ConsoleEventPublisher.cs
├── Projections/           # Event handlers để đồng bộ Read DB
│   └── OrderCreatedProjectionHandler.cs
├── Models/                # Domain models
│   └── Order.cs
├── Dtos/                  # Data Transfer Objects
│   ├── OrderDto.cs
│   └── OrderSummaryDto.cs
├── Data/                  # Database contexts
│   ├── WriteDbContext.cs  # Database cho Write operations
│   ├── ReadDbContext.cs   # Database cho Read operations
│   └── AppDbContext.cs    # Database cũ (deprecated)
└── Program.cs             # Entry point và API endpoints
```

---

## 🔧 Các Thành Phần Chính

### 1. Commands (Lệnh)

#### CreateOrderCommand
```csharp
public record CreateOrderCommand(
    string FirstName, 
    string LastName, 
    string Status, 
    decimal TotalCost
) : IRequest<OrderDto>;
```
- **Mục đích**: Tạo order mới
- **Kế thừa**: `IRequest<OrderDto>` từ MediatR
- **Validation**: Được validate bởi `CreateOrderCommandValidator`

#### CreateOrderCommandValidator
- Sử dụng **FluentValidation**
- Rules:
  - `FirstName`: Không được rỗng
  - `LastName`: Không được rỗng
  - `Status`: Không được rỗng
  - `TotalCost`: Phải lớn hơn 0

### 2. Queries (Truy vấn)

#### GetOrderByIdQuery
```csharp
public record GetOrderByIdQuery(int OrderId) : IRequest<OrderDto>;
```
- **Mục đích**: Lấy thông tin chi tiết một order theo ID
- **Trả về**: `OrderDto` hoặc `null` nếu không tìm thấy

#### GetOrderSummariesQuery
```csharp
public record GetOrderSummariesQuery() : IRequest<List<OrderSummaryDto>>;
```
- **Mục đích**: Lấy danh sách tóm tắt tất cả orders
- **Trả về**: `List<OrderSummaryDto>`

### 3. Handlers (Xử lý)

#### CreateOrderCommandHandler
**Trách nhiệm:**
1. Validate command bằng FluentValidation
2. Tạo Order entity mới
3. Lưu vào **WriteDbContext** (Write Database)
4. Publish `OrderCreatedEvent` thông qua MediatR
5. Trả về `OrderDto`

**Đặc điểm:**
- Sử dụng `WriteDbContext` để ghi dữ liệu
- Publish event để trigger projection handler

#### GetOrderByIdQueryHandler
**Trách nhiệm:**
1. Query từ **ReadDbContext** (Read Database)
2. Sử dụng `AsNoTracking()` để tối ưu hiệu suất
3. Trả về `OrderDto` hoặc `null`

**Đặc điểm:**
- Chỉ đọc từ Read DB
- `AsNoTracking()` giúp tăng hiệu suất vì không cần tracking changes

#### GetOrderSummariesQueryHandler
**Trách nhiệm:**
1. Query tất cả orders từ Read DB
2. Project sang `OrderSummaryDto` (kết hợp FirstName + LastName thành CustomerName)
3. Trả về danh sách summaries

### 4. Events (Sự kiện)

#### OrderCreatedEvent
```csharp
public record OrderCreatedEvent(
    int OrderId,
    string FirstName,
    string LastName,
    decimal TotalCost
) : INotification;
```
- **Mục đích**: Thông báo khi một order được tạo
- **Kế thừa**: `INotification` từ MediatR
- **Sử dụng**: Trigger projection để đồng bộ Read DB

#### Event Publishers

**InProcessEventPublisher:**
- Publish events trong cùng process
- Tìm và gọi tất cả handlers đăng ký cho event đó
- Sử dụng `IServiceProvider` để resolve handlers

**ConsoleEventPublisher:**
- Chỉ log event ra console (dùng cho testing/debugging)

### 5. Projections (Chiếu)

#### OrderCreatedProjectionHandler
**Trách nhiệm:**
1. Lắng nghe `OrderCreatedEvent`
2. Tạo Order entity mới từ event data
3. Lưu vào **ReadDbContext** (Read Database)

**Đặc điểm:**
- Implement `INotificationHandler<OrderCreatedEvent>` từ MediatR
- Tự động được gọi khi có `OrderCreatedEvent` được publish
- Đảm bảo Read DB luôn được đồng bộ với Write DB

### 6. Models & DTOs

#### Order (Domain Model)
```csharp
public class Order
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required decimal TotalCost { get; set; }
}
```

#### OrderDto
- DTO để trả về thông tin chi tiết order
- Sử dụng `record` type (immutable)

#### OrderSummaryDto
- DTO để trả về tóm tắt order
- Chứa `CustomerName` (FirstName + LastName) thay vì tách riêng

### 7. Database Contexts

#### WriteDbContext
- **Mục đích**: Database cho Write operations
- **Connection**: `WriteDbConnection` → `WriteDb.db`
- **Sử dụng**: Chỉ cho Commands (Create, Update, Delete)

#### ReadDbContext
- **Mục đích**: Database cho Read operations
- **Connection**: `ReadDbConnection` → `ReadDb.db`
- **Sử dụng**: Chỉ cho Queries (Get, List)
- **Tối ưu**: Có thể được tối ưu riêng cho đọc (indexes, denormalization)

#### AppDbContext (Deprecated)
- Database cũ, không còn sử dụng
- Được comment trong `Program.cs`

---

## 🔄 Luồng Xử Lý

### Luồng Tạo Order (Command Flow)

```
1. Client gửi POST /api/orders với CreateOrderCommand
   ↓
2. Program.cs nhận request → gọi mediator.Send(command)
   ↓
3. MediatR route đến CreateOrderCommandHandler
   ↓
4. Handler validate command (FluentValidation)
   ↓
5. Handler tạo Order entity và lưu vào WriteDbContext
   ↓
6. Handler publish OrderCreatedEvent qua MediatR
   ↓
7. MediatR tự động gọi OrderCreatedProjectionHandler
   ↓
8. ProjectionHandler lưu Order vào ReadDbContext
   ↓
9. Handler trả về OrderDto
   ↓
10. Program.cs trả về HTTP 201 Created với OrderDto
```

### Luồng Đọc Order (Query Flow)

```
1. Client gửi GET /api/orders/{id} với GetOrderByIdQuery
   ↓
2. Program.cs nhận request → gọi mediator.Send(query)
   ↓
3. MediatR route đến GetOrderByIdQueryHandler
   ↓
4. Handler query từ ReadDbContext (AsNoTracking)
   ↓
5. Handler trả về OrderDto hoặc null
   ↓
6. Program.cs trả về HTTP 200 OK hoặc 404 Not Found
```

### Luồng Đọc Danh Sách Orders

```
1. Client gửi GET /api/orders với GetOrderSummariesQuery
   ↓
2. Program.cs nhận request → gọi mediator.Send(query)
   ↓
3. MediatR route đến GetOrderSummariesQueryHandler
   ↓
4. Handler query tất cả orders từ ReadDbContext
   ↓
5. Handler project sang OrderSummaryDto (combine FirstName + LastName)
   ↓
6. Handler trả về List<OrderSummaryDto>
   ↓
7. Program.cs trả về HTTP 200 OK với danh sách
```

---

## 🛠️ Công Nghệ Sử Dụng

### NuGet Packages

1. **MediatR (v14.0.0)**
   - Implement CQRS pattern
   - Mediator pattern để decouple handlers
   - `IRequest<T>` cho Commands/Queries
   - `INotification` cho Events

2. **FluentValidation (v12.1.1)**
   - Validation cho Commands
   - Rule-based validation
   - Tích hợp với MediatR

3. **Entity Framework Core (v9.0.0)**
   - ORM cho database operations
   - Code-First approach
   - Migrations support

4. **Microsoft.EntityFrameworkCore.Sqlite (v9.0.0)**
   - SQLite database provider
   - File-based database

### .NET Version
- **Target Framework**: .NET 9.0
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

---

## 🌐 API Endpoints

### POST /api/orders
**Mục đích**: Tạo order mới

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "status": "Pending",
  "totalCost": 100.50
}
```

**Response:**
- **201 Created**: Trả về OrderDto
- **400 Bad Request**: Validation errors

**Validation Rules:**
- `firstName`: Required, không được rỗng
- `lastName`: Required, không được rỗng
- `status`: Required, không được rỗng
- `totalCost`: Required, phải > 0

### GET /api/orders/{id}
**Mục đích**: Lấy thông tin chi tiết order theo ID

**Response:**
- **200 OK**: Trả về OrderDto
- **404 Not Found**: Order không tồn tại

### GET /api/orders
**Mục đích**: Lấy danh sách tóm tắt tất cả orders

**Response:**
- **200 OK**: Trả về `List<OrderSummaryDto>`

---

## 🎨 Patterns & Best Practices

### 1. CQRS Pattern
- ✅ Tách biệt Commands và Queries
- ✅ Sử dụng MediatR để implement
- ✅ Handlers riêng biệt cho mỗi Command/Query

### 2. Event-Driven Architecture
- ✅ Sử dụng Events để đồng bộ Read DB
- ✅ Projections tự động cập nhật Read DB
- ✅ Decouple giữa Write và Read operations

### 3. Database Separation
- ✅ Write DB và Read DB riêng biệt
- ✅ Write DB: Chỉ cho Commands
- ✅ Read DB: Chỉ cho Queries, được đồng bộ qua Events

### 4. Validation
- ✅ Sử dụng FluentValidation
- ✅ Validation trong Handler
- ✅ Throw ValidationException khi invalid

### 5. DTOs
- ✅ Sử dụng DTOs để trả về data
- ✅ Record types cho immutability
- ✅ Tách biệt Domain Models và DTOs

### 6. Performance Optimization
- ✅ `AsNoTracking()` cho Read queries
- ✅ Separate Read/Write DBs
- ✅ Projections để denormalize data nếu cần

### 7. Error Handling
- ✅ Try-catch trong endpoints
- ✅ ValidationException handling
- ✅ Proper HTTP status codes

### 8. Dependency Injection
- ✅ Constructor injection
- ✅ Scoped services cho DbContexts
- ✅ MediatR tự động resolve handlers

---

## 📊 Sơ Đồ Kiến Trúc

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       │ HTTP Request
       ▼
┌─────────────────────────────────────┐
│         Program.cs                  │
│    (API Endpoints + MediatR)       │
└──────┬──────────────────┬───────────┘
       │                  │
       │ Command          │ Query
       ▼                  ▼
┌──────────────┐   ┌──────────────┐
│   MediatR    │   │   MediatR    │
└──────┬───────┘   └──────┬───────┘
       │                  │
       ▼                  ▼
┌──────────────────┐  ┌──────────────────┐
│ Command Handler  │  │  Query Handler  │
│ (Write Logic)    │  │  (Read Logic)   │
└──────┬───────────┘  └──────┬──────────┘
       │                     │
       │ Write               │ Read
       ▼                     ▼
┌──────────────┐      ┌──────────────┐
│ WriteDbContext│      │ ReadDbContext│
│  (WriteDb.db) │      │  (ReadDb.db) │
└──────┬───────┘      └──────────────┘
       │
       │ Publish Event
       ▼
┌──────────────┐
│   MediatR    │
│  (Events)    │
└──────┬───────┘
       │
       ▼
┌──────────────────────┐
│ Projection Handler   │
│ (Sync to Read DB)    │
└──────┬───────────────┘
       │
       │ Write
       ▼
┌──────────────┐
│ ReadDbContext│
│  (ReadDb.db) │
└──────────────┘
```

---

## 🔑 Điểm Quan Trọng

### Tại sao tách Read/Write DB?

1. **Performance**: Read DB có thể được tối ưu riêng (indexes, denormalization)
2. **Scalability**: Scale độc lập
3. **Consistency**: Write DB là source of truth, Read DB được đồng bộ qua events
4. **Flexibility**: Có thể sử dụng công nghệ khác nhau

### Event-Driven Projections

- Khi có event mới → Projection handler tự động cập nhật Read DB
- Đảm bảo Read DB luôn sync với Write DB
- Có thể có nhiều projections cho cùng một event

### MediatR Benefits

- **Decoupling**: Handlers không phụ thuộc trực tiếp vào API endpoints
- **Testability**: Dễ test handlers độc lập
- **Flexibility**: Dễ thêm behaviors (logging, validation, etc.)
- **Type Safety**: Compile-time checking

---

## 🚀 Mở Rộng Trong Tương Lai

1. **Event Sourcing**: Lưu tất cả events để có thể replay
2. **Message Queue**: Sử dụng RabbitMQ/Kafka cho events
3. **Caching**: Thêm caching layer cho Read DB
4. **Read Replicas**: Nhiều Read DB instances
5. **Saga Pattern**: Xử lý distributed transactions
6. **API Versioning**: Support multiple API versions
7. **Authentication/Authorization**: Thêm security layer

---

## 📝 Ghi Chú

- File `AppDbContext.cs` và `NoCQRS.db` là từ phiên bản cũ (không dùng CQRS)
- Các interfaces `ICommandHandler` và `IQueryHandler` được định nghĩa nhưng không sử dụng (đang dùng MediatR trực tiếp)
- `ConsoleEventPublisher` và `InProcessEventPublisher` được comment trong `Program.cs` (đang dùng MediatR's built-in event publishing)

---

**Tạo bởi**: AI Assistant  
**Ngày**: 2025-01-06  
**Phiên bản**: 1.0
