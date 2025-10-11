# API Integration - Sincronizador A3 y GIM

## 📋 Descripción General

Sistema de integración y sincronización automatizada entre los sistemas **A3** (ERP) y **GIM** (sistema de gestión). Esta solución implementa una arquitectura limpia (Clean Architecture) en .NET 9 que permite:

- **Sincronización programada** de datos entre A3 y GIM mediante jobs configurables
- **Gestión de horarios** con expresiones CRON para ejecuciones automáticas
- **Historial de ejecuciones** con métricas detalladas de éxito y fallos
- **Manejo robusto de errores** con políticas de reintentos y resiliencia
- **Monitoreo de salud** (Health Checks) para todos los componentes críticos

## 🏗️ Arquitectura del Proyecto

La solución sigue los principios de **Clean Architecture** y **Domain-Driven Design (DDD)**, organizada en 5 módulos principales:

```
ApiIntegration/
├── Api/                    # Capa de Presentación
├── Application/            # Capa de Aplicación
├── Domain/                 # Capa de Dominio
├── Infrastructure/         # Capa de Infraestructura
└── Tests/                  # Pruebas Unitarias e Integración
```

---

## 📦 Módulos del Proyecto

### 1️⃣ **Api** - Capa de Presentación
**Responsabilidad**: Exponer endpoints HTTP y configurar el pipeline de la aplicación.

**Componentes principales**:
- **Controllers/**
  - `SchedulerController`: Gestión de configuraciones de jobs (actualizar CRON, habilitar/deshabilitar)
  - `HistoryController`: Consulta del historial de ejecuciones con paginación y filtros
  - `HealthController`: Endpoints de health checks para monitoreo
  
- **Middleware/**
  - `ExceptionHandlingMiddleware`: Manejo centralizado de excepciones con respuestas HTTP estandarizadas
  
- **HealthChecks/**
  - `ApiHealthCheck`: Verifica que la API esté respondiendo
  - `DatabaseHealthCheck`: Verifica conectividad con PostgreSQL
  - `QuartzHealthCheck`: Verifica el estado del scheduler (jobs en ejecución, metadata)

**Características**:
- Documentación automática con Swagger/OpenAPI
- Serialización de enums como strings
- Health checks en `/health`, `/health/ready`, `/health/live`
- Configuración centralizada en `Program.cs`

---

### 2️⃣ **Application** - Capa de Aplicación
**Responsabilidad**: Orquestar la lógica de negocio mediante el patrón Mediator (CQRS).

**Componentes principales**:

- **Commands/** - Comandos para operaciones de escritura:
  - `UpdateMaterialsCommand`: Sincronizar materiales desde A3
  - `UpdateProvidersCommand`: Sincronizar proveedores
  - `UpdateWarehousesCommand`: Sincronizar almacenes
  - `UpdateMerchandiseEntryCommand`: Sincronizar entradas de mercancía
  - `UpdateStockPhotoValuationsCommand`: Sincronizar valoraciones fotográficas
  - `UpdateSyncSchedulerCommand`: Actualizar configuración de jobs

- **Queries/** - Consultas para operaciones de lectura:
  - `GetHistoryQuery`: Obtener historial de ejecuciones con filtros
  - `GetFailedResultQuery`: Obtener detalles de items fallidos

- **Handlers/** - Implementación de los comandos y queries:
  - Un handler por cada comando/query
  - Validación de reglas de negocio
  - Coordinación con repositorios y servicios

- **Abstractions/**
  - `IMediator`: Patrón Mediator para desacoplar Controllers de Handlers
  - `ISchedulerService`: Abstracción del scheduler de Quartz
  - `IRequestHandler<TRequest, TResponse>`: Interface genérico para handlers

- **Dtos/** - Objetos de transferencia de datos:
  - DTOs de A3 y GIM para mapeo de datos externos
  - `GetHistoryQueryResponse`: Respuesta paginada del historial
  - `SyncSchedulerUpdateRequest`: Request para actualizar schedules

**Características**:
- Implementación del patrón **CQRS** (Command Query Responsibility Segregation)
- Patrón **Mediator** para desacoplamiento
- **AutoMapper** para mapeo de entidades a DTOs
- Validaciones de negocio centralizadas

---

### 3️⃣ **Domain** - Capa de Dominio
**Responsabilidad**: Contener las entidades del negocio y las reglas de dominio puras (sin dependencias externas).

**Entidades principales**:

- **`SyncSchedule`**: Configuración de jobs programados
  ```csharp
  - Id (Guid)
  - JobKey (string): Identificador único del job
  - CronExpression (string): Expresión CRON para programación
  - Enabled (bool): Estado activo/inactivo
  - LastModifiedUtc (DateTime?)
  ```

- **`ExecutionHistory`**: Registro histórico de ejecuciones
  ```csharp
  - Id (Guid)
  - JobKey (string)
  - StartedAtUtc (DateTime)
  - FinishedAtUtc (DateTime)
  - ExtractedCount (int): Registros obtenidos
  - SuccessCount (int): Registros sincronizados exitosamente
  - FailedCount (int): Registros fallidos
  - Summary (string?): Resumen de la ejecución
  ```

- **`FailedItem`**: Detalles de items que fallaron en la sincronización
  ```csharp
  - Id (Guid)
  - ExecutionHistoryId (Guid): FK a ExecutionHistory
  - ItemIdentifier (string): Identificador del item
  - ErrorMessage (string): Mensaje de error
  - FailedAtUtc (DateTime)
  ```

**Interfaces**:
- `ISyncRepository`: Repositorio para persistencia de datos
- `ISyncJobService`: Servicio para ejecutar jobs de sincronización

**Enums**:
- Estados de sincronización
- Tipos de operaciones (Create, Update, Delete)

---

### 4️⃣ **Infrastructure** - Capa de Infraestructura
**Responsabilidad**: Implementar detalles técnicos, acceso a datos, integraciones externas y servicios de infraestructura.

**Componentes principales**:

- **Persistence/** - Entity Framework Core
  - `ApplicationDbContext`: DbContext principal con configuraciones
  - `Migrations/`: Migraciones de base de datos
  - Repositorios concretos implementando interfaces del dominio

- **HttpClients/** - Clientes HTTP para APIs externas
  - `A3ApiClient`: Cliente para integración con A3 (ERP)
  - `GimApiClient`: Cliente para integración con GIM
  - Configuración de políticas de resiliencia (Polly)

- **Quartz/** - Scheduler de jobs
  - `QuartzSchedulerService`: Implementación de `ISchedulerService`
  - `QuartzScheduleBackgroundService`: Carga inicial de jobs desde BD
  - `QuartzJobExceptionListener`: Listener para capturar excepciones
  - `GenericSyncJob`: Job genérico que ejecuta sincronizaciones

- **Jobs/**
  - `GenericSyncJob`: Job reutilizable que delega a los handlers correspondientes

- **Services/**
  - Implementaciones de servicios de dominio
  - `SyncJobService`: Orquesta la ejecución de trabajos de sincronización
  - `EfSyncRepository`: Repositorio con Entity Framework
  - `ResilientSyncRepository`: Decorator con políticas de resiliencia

- **Policies/** - Políticas de Resiliencia con Polly
  - Retry policies (reintentos exponenciales)
  - Circuit breaker (interruptor de circuito)
  - Timeout policies
  - Fallback policies

- **DependencyInjections/**
  - Extensiones para registro de servicios
  - Configuración de HttpClients
  - Configuración de Quartz con PostgreSQL

**Características**:
- **Quartz.NET** con persistencia en PostgreSQL para clustering
- **Polly** para resiliencia (reintentos, circuit breaker, timeout)
- **Entity Framework Core** con migraciones automáticas
- **Npgsql** para PostgreSQL

---

### 5️⃣ **Tests** - Pruebas
**Responsabilidad**: Garantizar la calidad y correcto funcionamiento del sistema.

**Tipos de pruebas**:
- Pruebas unitarias de handlers y servicios
- Pruebas de integración de APIs HTTP
- Pruebas de repositorios con base de datos en memoria

---

## 🔧 Tecnologías Utilizadas

| Categoría | Tecnología                        |
|-----------|-----------------------------------|
| **Framework** | .NET 9, ASP.NET Core              |
| **Base de Datos** | PostgreSQL 15+                    |
| **ORM** | Entity Framework Core 9           |
| **Scheduler** | Quartz.NET 3.x (con persistencia) |
| **Resiliencia** | Polly (retry, circuit breaker)    |
| **Documentación** | Swagger/OpenAPI                   |
| **Health Checks** | AspNetCore.HealthChecks           |
| **Containerización** | Docker, Docker Compose            |
| **Logging** | Microsoft.Extensions.Logging      |
| **Serialización** | System.Text.Json                  |

---

## 🚀 Jobs de Sincronización

El sistema incluye los siguientes jobs configurables:

| Job Key | Descripción | Frecuencia Recomendada |
|---------|-------------|------------------------|
| `UpdateMaterials` | Sincronizar catálogo de materiales | Diaria (2 AM) |
| `UpdateProviders` | Sincronizar proveedores | Diaria (1 AM) |
| `UpdateWarehouses` | Sincronizar almacenes | Semanal |
| `UpdateMerchandiseEntry` | Sincronizar entradas de mercancía | Cada 4 horas |
| `UpdateStockPhotoValuations` | Sincronizar valoraciones fotográficas | Diaria (3 AM) |

Cada job:
- Se ejecuta según su expresión CRON configurada
- Registra métricas en `ExecutionHistory`
- Guarda items fallidos en `FailedItem` para análisis
- Implementa reintentos automáticos con backoff exponencial

---

## 🗄️ Base de Datos

### Tablas Principales

**`SyncSchedules`**: Configuración de jobs programados
- Permite habilitar/deshabilitar jobs
- Modificar expresiones CRON en tiempo de ejecución

**`ExecutionHistories`**: Historial de ejecuciones
- Métricas de cada ejecución (extraídos, exitosos, fallidos)
- Timestamps de inicio y fin
- Resumen de la operación

**`FailedItems`**: Registro de fallos individuales
- Identificador del item que falló
- Mensaje de error específico
- Relación con la ejecución histórica

**Esquema Quartz** (prefijo `quartz.qrtz_`):
- Tablas de Quartz.NET para persistencia de jobs y triggers
- Soporta clustering para alta disponibilidad

---

## 📡 Endpoints de la API

### Scheduler
```http
PATCH /api/scheduler/update
```
Actualiza la configuración de un job (CRON, estado enabled)

### History
```http
GET /api/history?sortBy=StartedAt&sortOrder=Descending&offset=0&limit=20
```
Obtiene historial paginado de ejecuciones con filtros

```http
GET /api/history/failed/{executionId}
```
Obtiene items fallidos de una ejecución específica

### Health Checks
```http
GET /health          # Estado general
GET /health/ready    # Readiness probe (K8s)
GET /health/live     # Liveness probe (K8s)
```

---

## ⚙️ Configuración

### Variables de Entorno

Crear archivo `.env` en la raíz:

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5434;Database=syncdb;Username=syncuser;Password=syncpass
A3Api__BaseUrl=https://a3-api.example.com
A3Api__ApiKey=your-a3-api-key
GimApi__BaseUrl=https://gim-api.example.com
GimApi__ApiKey=your-gim-api-key
```

### appsettings.json

```json
{
  "Quartz": {
    "TablePrefix": "quartz.qrtz_",
    "MaxConcurrency": 10,
    "EnableClustering": false
  }
}
```

---

## 🐳 Ejecución con Docker

### Iniciar servicios

```bash
docker-compose up -d
```

Esto levanta:
- PostgreSQL en puerto `5434`
- API en puerto `8080`

### Ver logs

```bash
docker-compose logs -f api
```

### Detener servicios

```bash
docker-compose down
```

---

## 💻 Ejecución Local (Desarrollo)

### Prerrequisitos
- .NET 9 SDK
- PostgreSQL 15+
- IDE (Rider, Visual Studio, VS Code)

### 1. Configurar Base de Datos

```bash
# Iniciar PostgreSQL con Docker
docker-compose up -d postgres
```

### 2. Aplicar Migraciones

Navegar a la carpeta `Api`:

```bash
cd Api
```

**Crear una nueva migración**:
```bash
dotnet ef migrations add NombreMigracion \
  --project ../Infrastructure/Infrastructure.csproj \
  --startup-project ./Api.csproj \
  -o Persistence/Migrations
```

**Aplicar migraciones**:
```bash
dotnet ef database update --startup-project ./Api.csproj
```

> **Nota**: Las migraciones se aplican automáticamente al iniciar la aplicación gracias a `ApplyDatabaseMigrations()` en `Program.cs`.

### 3. Ejecutar la API

```bash
cd Api
dotnet run
```

La API estará disponible en:
- Swagger: `https://localhost:5001` o `http://localhost:5000`
- Health: `https://localhost:5001/health`

### 4. Ejecutar Tests

```bash
cd Tests
dotnet test
```

---

## 📊 Monitoreo y Observabilidad

### Health Checks

El sistema expone varios endpoints para monitoreo:

- **`/health`**: Estado completo del sistema
  - API health
  - Database connectivity
  - Quartz scheduler status
  - PostgreSQL connection pool

- **`/health/ready`**: Para Kubernetes readiness probes
- **`/health/live`**: Para Kubernetes liveness probes

### Logging

Los logs se configuran por nivel en `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Quartz": "Information"
    }
  }
}
```

### Métricas de Jobs

Cada ejecución registra:
- ✅ Total de registros extraídos
- ✅ Registros sincronizados exitosamente
- ❌ Registros fallidos con detalles
- ⏱️ Duración de la ejecución
- 📝 Resumen descriptivo

---

## 🔒 Manejo de Errores

### Estrategia de Resiliencia

1. **Reintentos Exponenciales**: 3 intentos con backoff (2s, 4s, 8s)
2. **Circuit Breaker**: Abre el circuito tras 5 fallos consecutivos
3. **Timeout**: 10 minutos máximo por job
4. **Fallback**: Registra errores y continúa con el siguiente item

### Middleware de Excepciones

Captura y formatea todas las excepciones:
- `HttpException`: Códigos HTTP personalizados
- `ApplicationException`: Errores de negocio (400)
- `InfrastructureException`: Errores técnicos (500)
- Excepciones no controladas: 500 con mensaje genérico

---

## 🧪 Patrones de Diseño Implementados

- ✅ **Clean Architecture**: Separación por capas con dependencias hacia el dominio
- ✅ **CQRS**: Separación de comandos y queries
- ✅ **Mediator Pattern**: Desacoplamiento entre Controllers y Handlers
- ✅ **Repository Pattern**: Abstracción del acceso a datos
- ✅ **Unit of Work**: Transacciones gestionadas por EF Core
- ✅ **Decorator Pattern**: `ResilientSyncRepository` añade resiliencia
- ✅ **Factory Pattern**: Creación de policies de Polly
- ✅ **Dependency Injection**: IoC container de .NET

---

## 📝 Convenciones de Código

- **Nombres de Jobs**: PascalCase (ej: `UpdateMaterials`)
- **Endpoints**: kebab-case en URLs
- **DTOs**: Sufijo `Dto` o `Request`/`Response`
- **Commands**: Sufijo `Command`
- **Queries**: Sufijo `Query`
- **Handlers**: Sufijo `CommandHandler` o `QueryHandler`

---

## 🤝 Contribución

Para contribuir al proyecto:

1. Crear una rama desde `main`: `git checkout -b feature/nueva-funcionalidad`
2. Realizar cambios siguiendo las convenciones
3. Ejecutar tests: `dotnet test`
4. Crear Pull Request con descripción detallada

---

## 📧 Contacto

**Development Team**  
Email: raymel.ramos@businessinsights.es

---

## 📄 Licencia

Este proyecto es propiedad de *. Todos los derechos reservados.

---

**Última actualización**: Octubre 2025  
**Versión**: 1.0.0
