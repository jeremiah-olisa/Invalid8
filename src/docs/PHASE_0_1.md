# Phase 0 & 1 TODOs

## 🎯 **Phase 0: Foundation & Setup**

### Project Structure Creation
- [x] Create solution file: `Invalid8.sln`
- [x] Create core project: `src/Invalid8.Core/Invalid8.Core.csproj`
- [x] Create main library: `src/Invalid8/Invalid8.csproj`
- [ ] Create memory provider: `src/Invalid8.Providers.Memory/Invalid8.Providers.Memory.csproj`
- [ ] Create Redis cache provider: `src/Invalid8.Providers.Cache.Redis/Invalid8.Providers.Cache.Redis.csproj`
- [ ] Create Redis message provider: `src/Invalid8.Providers.Message.Redis/Invalid8.Providers.Message.Redis.csproj`
- [ ] Create test projects structure
- [ ] Setup directory build props for consistent NuGet packages

### Core Interfaces & Models
- [x] Define `ICacheProvider` interface (inherits `IDistributedCache`)
- [x] Define `IEventProvider` interface for message bus
- [x] Define `IQueryClient` interface for core operations
- [x] Define `IKeyGenerator` interface for key management
- [x] Create model classes:
  - [x] `CacheEntryOptions`
  - [x] `QueryOptions` 
  - [x] `MutationOptions`
  - [x] `QueryResult<T>`
  - [x] `CacheEvent` base class
  - [x] `CacheInvalidationEvent`
  - [x] `CacheUpdatedEvent`

### Base Provider Infrastructure
- [x] Create `BaseCacheProvider` abstract class implementing `ICacheProvider`
- [x] Create `BaseEventProvider` abstract class implementing `IEventProvider`
- [x] Create `CacheKeyGenerator` implementing `IKeyGenerator`

## 🏗️ **Phase 1: Core Implementation**

### Core Query Client Implementation
- [x] Create `QueryClient` class implementing `IQueryClient`
- [x] Implement `QueryAsync<T>` method:
  - [x] Cache-first logic
  - [x] Query execution on cache miss
  - [x] Cache population
  - [x] Basic stale-while-revalidate pattern
- [x] Implement `MutateAsync<T>` method:
  - [x] Mutation execution
  - [x] Cache invalidation
  - [ ] Event publishing

### Redis Cache Provider (`Invalid8.Providers.Cache.Redis`)
- [x] Add `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet dependency
- [x] Create `RedisCacheProvider` inheriting from `BaseCacheProvider`
- [x] Implement `IDistributedCache` interface methods:
  - [x] `GetAsync<T>`
  - [x] `SetAsync<T>`
  - [x] `RemoveAsync`
  - [x] `RefreshAsync`
- [ ] Add Redis connection management
- [ ] Implement serialization/deserialization
- [ ] Add connection resilience (retry logic)

### Redis Message Provider (`Invalid8.Providers.Message.Redis`)
- [ ] Add `StackExchange.Redis` NuGet dependency
- [ ] Create `RedisEventProvider` inheriting from `BaseEventProvider`
- [ ] Implement Redis Pub/Sub for events:
  - [ ] `PublishAsync` for sending events
  - [ ] `SubscribeAsync` for receiving events
- [ ] Add channel management
- [ ] Implement message serialization

### Memory Provider Implementation
- [ ] Implement `MemoryCacheProvider` inheriting from `BaseCacheProvider`
- [ ] Implement `MemoryEventProvider` inheriting from `BaseEventProvider`
- [ ] Add in-memory storage with `ConcurrentDictionary`
- [ ] Implement basic expiration handling
- [ ] Add thread-safe operations

### Dependency Injection Setup
- [ ] Create extension methods for DI registration:
  - [x] `AddInvalid8Core()` - registers core services
  - [ ] `AddMemoryCacheProvider()` - registers memory cache
  - [ ] `AddMemoryEventProvider()` - registers memory events
  - [x] `AddRedisCacheProvider()` - registers Redis cache
  - [ ] `AddRedisEventProvider()` - registers Redis events
  - [x] `AddQueryClient()` - registers query client

### Basic Unit Tests
- [ ] Create test project for core: `tests/Invalid8.Core.Tests/`
- [ ] Create test project for memory provider: `tests/Invalid8.Providers.Memory.Tests/`
- [ ] Create test project for Redis provider: `tests/Invalid8.Providers.Cache.Redis.Tests/`
- [ ] Write tests for:
  - [ ] Key generation and validation
  - [ ] Basic cache operations (get/set/remove)
  - [ ] Cache expiration behavior
  - [ ] Query client cache hit/miss scenarios
  - [ ] Mutation with invalidation
  - [ ] Event publishing and subscription

### Configuration Management
- [ ] Create options classes:
  - [ ] `RedisCacheOptions` for Redis cache configuration
  - [ ] `RedisEventOptions` for Redis message configuration
  - [ ] `Invalid8Options` for general library configuration
- [ ] Add configuration validation
- [ ] Setup configuration binding from appsettings.json

### Basic Error Handling
- [x] Define custom exception types:
  - [x] `CacheException`
  - [x] `EventBusException`
  - [x] `InvalidKeyException`
- [x] Add basic try-catch patterns
- [x] Implement basic logging

## 🔄 **Phase Completion Criteria**

### Phase 0 Complete When:
- [ ] All projects created with proper references
- [ ] Core interfaces and models defined
- [ ] Base provider classes implemented
- [ ] Solution builds successfully

### Phase 1 Complete When:
- [ ] Memory provider fully functional
- [ ] Redis cache provider working with `IDistributedCache`
- [ ] Redis message provider working with Pub/Sub
- [ ] Query client handles basic cache scenarios
- [ ] Unit tests passing for core functionality
- [ ] DI registration methods available
- [ ] Basic configuration system in place
