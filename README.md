# Cardapio Online — Backend

API RESTful construida em ASP.NET Core com Clean Architecture, CQRS via MediatR, EF Core e autenticacao JWT dupla (admin + cliente).

## Indice

- [Stack tecnica](#stack-tecnica)
- [Arquitetura](#arquitetura)
- [Como executar](#como-executar)
- [Configuracao](#configuracao)
- [Autenticacao](#autenticacao)
- [Entidades do dominio](#entidades-do-dominio)
- [Endpoints](#endpoints)
- [Logica de negocio](#logica-de-negocio)
- [Tratamento de erros](#tratamento-de-erros)
- [Migrations](#migrations)

---

## Stack tecnica

- .NET `10.0`
- ASP.NET Core Web API
- Entity Framework Core `9.0.5` + SQL Server
- MediatR `12.5.0`
- FluentValidation `11.11.0`
- JWT Bearer Authentication (dois esquemas independentes)
- OpenAPI nativo do ASP.NET Core

---

## Arquitetura

O projeto segue Clean Architecture / Onion Architecture com quatro camadas:

```
src/main/
├── Api/            # Controllers, middleware, configuracao HTTP, CORS, autenticacao
├── Application/    # Commands, Queries, DTOs, validacoes (FluentValidation), pipeline behaviors
├── Domain/         # Entidades e enums (sem dependencias)
└── Infrastructure/ # EF Core, DbContext, migrations, DI, seeder
```

### Fluxo de uma requisicao

1. Controller recebe a requisicao HTTP
2. Controller cria um Command ou Query e envia ao MediatR (`sender.Send(...)`)
3. `ValidationBehavior` executa FluentValidation antes de qualquer handler
4. Handler executa a logica e persiste via `IApplicationDbContext`
5. EF Core persiste ou consulta dados no SQL Server
6. Resultado retorna como DTO serializado em JSON

### Estrutura de pastas

```
src/
├── Backend.slnx
├── workload-install.ps1
└── main/
    ├── Api/
    │   ├── Controllers/
    │   │   ├── AuthController.cs
    │   │   ├── CategoriesController.cs
    │   │   ├── ClientsController.cs
    │   │   ├── EstabelecimentoController.cs
    │   │   ├── IntegrationsController.cs
    │   │   ├── InventoryController.cs
    │   │   ├── NeighborhoodDeliveryFeesController.cs
    │   │   ├── OrdersController.cs
    │   │   ├── ProductsController.cs
    │   │   └── UploadsController.cs
    │   └── Middleware/
    ├── Application/
    │   └── Features/
    │       ├── AdditionalGroups/
    │       ├── Categories/
    │       ├── Clients/
    │       ├── Estabelecimento/
    │       ├── Integrations/
    │       ├── Inventory/
    │       ├── NeighborhoodDeliveryFees/
    │       ├── Orders/
    │       └── Products/
    ├── Domain/
    │   ├── Entities/
    │   └── Enums/
    └── Infrastructure/
        ├── Data/
        └── Migrations/
```

---

## Como executar

### Pre-requisitos

- SDK do .NET `10.0`
- SQL Server acessivel pela connection string configurada
- PowerShell (para o script de secrets)

### 1. Restaurar dependencias

```powershell
dotnet restore src\Backend.slnx
```

### 2. Configurar secrets de desenvolvimento

```powershell
cd src\main\Api
.\init-dev-secrets.ps1
```

Com parametros customizados:

```powershell
.\init-dev-secrets.ps1 -AdminEmail "admin@cardapioonline.local" -AdminPassword "SuaSenhaForte123!" -JwtSecret "chave-com-pelo-menos-32-caracteres"
```

O script grava via `dotnet user-secrets`:
- `AdminAuth:Email`
- `AdminAuth:PasswordHash`
- `AdminAuth:JwtIssuer`
- `AdminAuth:JwtAudience`
- `AdminAuth:JwtSecret`
- `AdminAuth:TokenExpirationMinutes`

### 3. Ajustar a connection string

`src/main/Api/appsettings.json` aponta por padrao para:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CARDAPIOONLINE_DB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

### 4. Executar

```powershell
dotnet run --project src\main\Api\Api.csproj
```

As migrations pendentes sao aplicadas automaticamente no startup via `MigrateAsync()`.

### URLs de desenvolvimento

- HTTP: `http://localhost:5115`
- HTTPS: `https://localhost:7272`
- OpenAPI: `http://localhost:5115/openapi/v1.json`

---

## Configuracao

### `Api`

| Chave | Descricao |
|---|---|
| `Api:BaseUrl` | URL base publica da API. A aplicacao falha ao iniciar se estiver vazia |

### `AdminAuth`

| Chave | Descricao |
|---|---|
| `AdminAuth:Email` | Email do administrador |
| `AdminAuth:PasswordHash` | Hash PBKDF2-SHA256 da senha |
| `AdminAuth:JwtIssuer` | Issuer do JWT admin |
| `AdminAuth:JwtAudience` | Audience do JWT admin |
| `AdminAuth:JwtSecret` | Chave secreta (minimo 32 caracteres) |
| `AdminAuth:TokenExpirationMinutes` | Expiracao do token (deve ser > 0) |

### `ClientAuth`

| Chave | Descricao |
|---|---|
| `ClientAuth:JwtIssuer` | Issuer do JWT cliente |
| `ClientAuth:JwtAudience` | Audience do JWT cliente |
| `ClientAuth:TokenExpirationMinutes` | Expiracao do token do cliente |

### CORS

A policy `Angular` permite apenas `http://localhost:4200`. Altere `Program.cs` se o frontend rodar em outra origem.

### Logging

Em desenvolvimento, `Microsoft.EntityFrameworkCore.Database.Command: Information` exibe as queries SQL no console.

---

## Autenticacao

### Admin JWT

- Login: `POST /api/Auth/login`
- Header: `Authorization: Bearer <token>`
- Rotas protegidas usam `[Authorize]`

Payload:

```json
{ "email": "admin@cardapioonline.local", "password": "Admin@123" }
```

Resposta:

```json
{ "token": "<jwt>", "expiresAt": "...", "email": "admin@cardapioonline.local" }
```

### Cliente JWT

- Login: `POST /api/Clients/authenticate`
- Header: `Authorization: Bearer <token>` com scheme `ClientAuth`
- Rotas protegidas usam `[Authorize(AuthenticationSchemes = ClientAuthOptions.AuthenticationScheme)]`

Os dois esquemas sao completamente independentes; um token admin nao funciona em rota de cliente e vice-versa.

### Senhas

Armazenadas como hash PBKDF2-SHA256 com salt. `IAdminPasswordHasher` e `IClientPasswordHasher` encapsulam a logica.

---

## Entidades do dominio

### Estabelecimento

| Campo | Tipo | Descricao |
|---|---|---|
| Id | int | PK |
| Name | string | Nome do estabelecimento |
| LogoUrl | string | URL da logo |
| Category | string | Tipo (hamburgueria, pizzaria...) |
| Address | string | Endereco do estabelecimento |
| Whatsapp | string | Numero WhatsApp |
| OpenTime | TimeOnly | Horario de abertura |
| CloseTime | TimeOnly | Horario de fechamento |
| SendOrderTrackingViaWhatsApp | bool | Envia link de rastreamento via WhatsApp |
| PreparationTimeMinutes | int | Tempo medio de preparo (minutos) |
| DeliverySafetyMarginMinutes | int | Margem de seguranca no prazo (minutos) |
| InstagramUrl | string? | Link Instagram |
| FacebookUrl | string? | Link Facebook |
| TikTokUrl | string? | Link TikTok |
| TwitterUrl | string? | Link Twitter/X |
| UpdatedAt | DateTimeOffset | Ultima atualizacao |

### Product

| Campo | Tipo | Descricao |
|---|---|---|
| Id | Guid | PK |
| Name | string | Nome |
| Description | string | Descricao |
| Price | decimal | Preco regular |
| Category | string | Slug da categoria |
| ImageUrl | string | URL da imagem |
| IsOnPromotion | bool | Em promocao |
| PromotionalPrice | decimal? | Preco promocional |
| IsActive | bool | Visivel no cardapio |
| TrackInventory | bool | Controla estoque |
| StockQuantity | int | Quantidade em estoque |
| LowStockThreshold | int | Limite de alerta de estoque baixo |
| CreatedAt | DateTimeOffset | Data de criacao |
| RowVersion | byte[] | Controle de concorrencia otimista |

### Category

| Campo | Tipo | Descricao |
|---|---|---|
| Id | int | PK |
| Slug | string | Identificador unico (gerado do nome, imutavel) |
| Name | string | Nome exibido |
| SortOrder | int | Ordem de exibicao |
| IsActive | bool | Ativa no cardapio |

Categorias seed: `hamburguer`, `pizza`, `bebida`, `sobremesa`, `porcao`, `outro`.

### AdditionalGroup / AdditionalItem

Grupos de personalizacao vinculados a um produto (ex: "Escolha o molho"). Cada grupo define `MinSelections` e `MaxSelections`. Itens de um grupo tem `Name` e `Price`.

### Client

| Campo | Tipo | Descricao |
|---|---|---|
| Id | Guid | PK |
| Name | string | Nome completo |
| Email | string | Email (unico) |
| Phone | string | Telefone (unico) |
| ZipCode | string | CEP |
| Street | string | Logradouro |
| Number | string | Numero |
| Neighborhood | string | Bairro |
| City | string | Cidade |
| State | string | Estado |
| Complement | string | Complemento |
| PasswordHash | string | Hash PBKDF2-SHA256 |
| RegisteredAt | DateOnly | Data de cadastro |

### Order

| Campo | Tipo | Descricao |
|---|---|---|
| Id | Guid | PK |
| Number | string | Numero unico (`P{yyMMddHHmm}{hexAleatorio}`) |
| ClientId | Guid? | Cliente vinculado (opcional) |
| ClientName | string | Nome do cliente (snapshot) |
| ClientPhone | string | Telefone (snapshot) |
| Address | string | Endereco de entrega |
| Total | decimal | Total incluindo taxa de entrega |
| DeliveryFee | decimal | Taxa de entrega (snapshot) |
| Status | OrderStatus | Status atual |
| Source | OrderSource | Origem (WhatsApp/IFood/Site) |
| OrderType | string? | Entrega / Retirada / ConsumoLocal |
| Note | string? | Observacao do cliente |
| Date | DateOnly | Data do pedido |
| CreatedAt | DateTimeOffset | Timestamp de criacao (UTC) |
| EstimatedPreparationMinutes | int? | Tempo de preparo estimado |
| EstimatedTravelMinutes | int? | Tempo de deslocamento estimado |
| EstimatedDeliveryMinutes | int? | Tempo total estimado |
| EstimatedDeliveryDistanceKm | decimal? | Distancia estimada |
| EstimatedReadyAt | DateTimeOffset? | Previsao de ficar pronto (UTC) |
| EstimatedDeliveryDeadlineAt | DateTimeOffset? | Prazo limite (UTC) |
| MarkedDelayedAt | DateTimeOffset? | Quando foi marcado como atrasado (UTC) |
| DeliveryStartedAt | DateTimeOffset? | Quando saiu para entrega (UTC) |

**Status (`OrderStatus`):** `Pendente` → `EmPreparo` → `EmEntrega` → `Entregue` (ou `EmAtraso` / `Cancelado`)

**Origem (`OrderSource`):** `WhatsApp`, `IFood`, `Site`

### OrderItem / OrderItemAdditional

Snapshot dos itens: `ProductId`, `ProductName`, `Quantity`, `UnitPrice` (preco promocional se aplicavel), adicionais selecionados com nome e preco unitario.

### NeighborhoodDeliveryFee

| Campo | Tipo | Descricao |
|---|---|---|
| Id | int | PK |
| Neighborhood | string | Nome do bairro (unico) |
| Fee | decimal | Valor da taxa |
| IsActive | bool | Taxa ativa |

Lookup feito por comparacao case-insensitive (collation SQL Server). O nome deve coincidir exatamente com o retorno do ViaCep (`response.bairro`).

### InventoryMovement

| Campo | Tipo | Descricao |
|---|---|---|
| Id | Guid | PK |
| ProductId | Guid | Produto |
| Type | InventoryMovementType | Entrada / Venda / Cancelamento / Ajuste / Perda |
| Quantity | int | Quantidade movimentada |
| BalanceBefore | int | Saldo antes |
| BalanceAfter | int | Saldo apos |
| Reason | string | Motivo |
| OrderId | Guid? | Pedido associado |
| CreatedAt | DateTimeOffset | Timestamp |

### Integration

Configuracoes de integracoes externas em registro unico por provider. Providers: `IFood`, `Anotai`, `UberEats`, `NinetyNineFood`, `AiAgents`, `WhatsApp`, `TakeBlip`, `Zenvia`.

---

## Endpoints

### Auth

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| POST | `/api/Auth/login` | Publica | Login admin, retorna JWT |

### Estabelecimento

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| GET | `/api/Estabelecimento` | Publica | Dados do estabelecimento |
| PUT | `/api/Estabelecimento` | Admin | Criar ou atualizar |

Payload:

```json
{
  "name": "Pizzaria Exemplo",
  "logoUrl": "http://localhost:5115/uploads/20260620/logo.png",
  "category": "Pizzaria",
  "address": "Av. Principal, 1000",
  "whatsapp": "5511999999999",
  "openTime": "18:00",
  "closeTime": "23:59",
  "sendOrderTrackingViaWhatsApp": false,
  "preparationTimeMinutes": 30,
  "deliverySafetyMarginMinutes": 10,
  "instagramUrl": "https://instagram.com/pizzariaexemplo",
  "facebookUrl": null,
  "tikTokUrl": null,
  "twitterUrl": null
}
```

### Produtos

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| GET | `/api/Products` | Publica | Listar ativos paginados (`page`, `pageSize`, `category`) |
| POST | `/api/Products` | Admin | Criar produto |
| PUT | `/api/Products/{id}` | Admin | Atualizar produto |
| DELETE | `/api/Products/{id}` | Admin | Soft delete |

Payload de criacao/atualizacao:

```json
{
  "name": "X-Burger",
  "description": "Pao, carne e queijo",
  "price": 29.9,
  "category": "hamburguer",
  "imageUrl": "http://localhost:5115/uploads/20260620/img.png",
  "trackInventory": false,
  "stockQuantity": 0,
  "lowStockThreshold": 0,
  "isOnPromotion": true,
  "promotionalPrice": 24.9
}
```

### Categorias

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| GET | `/api/Categories` | Publica | Listar ativas por `SortOrder` |
| POST | `/api/Categories` | Admin | Criar (slug gerado do nome) |
| PUT | `/api/Categories/{id}` | Admin | Atualizar nome e ordem (slug imutavel) |
| DELETE | `/api/Categories/{id}` | Admin | Excluir (recusado se houver produtos ativos) |

### Clientes

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| POST | `/api/Clients` | Publica | Cadastrar cliente |
| POST | `/api/Clients/authenticate` | Publica | Login do cliente |
| GET | `/api/Clients` | Admin | Listar clientes (`page`, `pageSize`, `search`) |

### Pedidos

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| POST | `/api/Orders` | Publica | Criar pedido |
| GET | `/api/Orders/estimate` | Publica | Previa de estimativa (`address`, `orderType`, `neighborhood`) |
| GET | `/api/Orders` | Admin | Listar pedidos (`page`, `pageSize`, `date`, `search`, `activeOnly`) |
| GET | `/api/Orders/client` | Cliente | Historico do cliente autenticado |
| GET | `/api/Orders/track/{id}` | Publica | Rastrear pedido |
| PUT | `/api/Orders/track/{id}/delivered` | Publica | Cliente confirma recebimento |
| PUT | `/api/Orders/{id}/advance` | Admin | Avancar status |
| PUT | `/api/Orders/{id}/delay` | Admin | Marcar como atrasado |
| PUT | `/api/Orders/{id}/cancel` | Admin | Cancelar pedido |

Payload de criacao:

```json
{
  "clientName": "Maria Silva",
  "clientPhone": "11999999999",
  "address": "Rua A, 123 - Centro - Sao Paulo/SP",
  "neighborhood": "Centro",
  "source": "Site",
  "orderType": "Entrega",
  "note": "Sem cebola",
  "items": [
    { "productId": "00000000-0000-0000-0000-000000000000", "quantity": 2 }
  ]
}
```

### Adicionais

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| GET | `/api/Products/{productId}/additional-groups` | Publica | Listar grupos do produto |
| POST | `/api/Products/{productId}/additional-groups` | Admin | Criar grupo |
| PUT | `/api/Products/{productId}/additional-groups/{groupId}` | Admin | Atualizar grupo |
| DELETE | `/api/Products/{productId}/additional-groups/{groupId}` | Admin | Excluir grupo |
| POST | `/api/Products/{productId}/additional-groups/{groupId}/items` | Admin | Criar item |
| PUT | `/api/Products/{productId}/additional-groups/{groupId}/items/{itemId}` | Admin | Atualizar item |
| DELETE | `/api/Products/{productId}/additional-groups/{groupId}/items/{itemId}` | Admin | Excluir item |

### Taxas de Entrega por Bairro

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| GET | `/api/NeighborhoodDeliveryFees` | Admin | Listar taxas |
| POST | `/api/NeighborhoodDeliveryFees` | Admin | Criar taxa |
| PUT | `/api/NeighborhoodDeliveryFees/{id}` | Admin | Atualizar taxa |
| DELETE | `/api/NeighborhoodDeliveryFees/{id}` | Admin | Excluir taxa |

### Estoque

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| GET | `/api/Inventory` | Admin | Produtos com status de estoque (`page`, `pageSize`, `status`, `search`) |
| GET | `/api/Inventory/movements` | Admin | Historico de movimentacoes (`productId`, `page`, `pageSize`) |
| POST | `/api/Inventory/movements` | Admin | Registrar movimentacao |

### Integracoes (todas requerem Admin)

| Metodo | Rota |
|---|---|
| GET | `/api/Integrations` |
| PUT | `/api/Integrations/ifood` |
| PUT | `/api/Integrations/anotai` |
| PUT | `/api/Integrations/ubereats` |
| PUT | `/api/Integrations/99food` |
| PUT | `/api/Integrations/aiagents` |
| PUT | `/api/Integrations/whatsapp` |
| PUT | `/api/Integrations/takeblip` |
| PUT | `/api/Integrations/zenvia` |

### Upload

| Metodo | Rota | Auth | Descricao |
|---|---|---|---|
| POST | `/api/Uploads/image` | Admin | Upload de imagem (max 10 MB, `image/*`) |

Imagens salvas em `wwwroot/uploads/{yyyyMMdd}/` e servidas como arquivos estaticos.

---

## Logica de negocio

### Taxa de entrega por bairro

- Configurada no admin via `NeighborhoodDeliveryFees`
- Lookup no backend por comparacao case-insensitive com o campo `neighborhood` do pedido
- O campo `neighborhood` no payload de criacao do pedido deve corresponder ao nome exato cadastrado no admin
- No frontend, o bairro vem do ViaCep (`response.bairro`)
- Taxa e snapshot no momento da criacao do pedido; alteracoes futuras nao afetam pedidos ja criados
- Pedidos `Retirada` e `ConsumoLocal` tem `DeliveryFee = 0`

### Estimativa de entrega

- Calculada por `IDeliveryEstimateService` usando endereco de origem/destino, CEP quando disponivel, `PreparationTimeMinutes` e `DeliverySafetyMarginMinutes`
- Formula: `estimatedTravelMinutes = ceil(km / velocidadeMedia * 60) + DeliverySafetyMarginMinutes`
- Preview disponivel em `GET /api/Orders/estimate` antes da criacao
- Snapshot persistido no pedido; alteracoes nas configuracoes do estabelecimento nao recalculam pedidos antigos
- Datas/horas mantidas em UTC internamente; convertidas para horario de Brasilia nas respostas

### Atraso automatico

`OrderDelayMonitorService` (HostedService) roda a cada 1 minuto e marca como `EmAtraso` pedidos que:
- Possuem `EstimatedDeliveryDeadlineAt`
- Nao foram finalizados (`Entregue` ou `Cancelado`)
- Passaram do prazo e ainda nao foram marcados como atrasados

O admin tambem pode marcar manualmente via `PUT /api/Orders/{id}/delay`. Pedidos `EmAtraso` ainda podem ser avancados para `Entregue`.

### Confirmacao de entrega pelo cliente

Disponivel em `PUT /api/Orders/track/{id}/delivered` apenas quando:
- O admin avancou o status para `EmEntrega` (gravando `DeliveryStartedAt`)
- O status atual ainda e `EmEntrega` ou `EmAtraso`

### Numeracao de pedidos

Formato: `P{yyMMddHHmm}{4 chars hex aleatorios}`, com restricao `UNIQUE` no banco.

### Promocoes

- `isOnPromotion: true` exige `promotionalPrice > 0` e `promotionalPrice < price`
- Quando em promocao, `OrderItem.UnitPrice` recebe `PromotionalPrice`
- `isOnPromotion: false` zera `PromotionalPrice` no backend independentemente do valor enviado

### Adicionais no pedido

Snapshot do nome e preco no momento do pedido. Mesmo produto com adicionais diferentes gera itens distintos no carrinho.

### Controle de estoque

- Opcional por produto (`TrackInventory`)
- Criacao do pedido deduz estoque dos itens controlados
- Cancelamento do pedido restaura o estoque

### Categorias

- Slug gerado automaticamente do nome na criacao e imutavel depois
- Exclusao bloqueada se houver produtos ativos vinculados (422)

### Paginacao

Todas as listagens retornam `PaginatedResult<T>`:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "total": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

## Tratamento de erros

| Excecao | Status | Descricao |
|---|---|---|
| `ValidationException` | 400 | Erros de validacao do FluentValidation |
| `UnauthorizedAccessException` | 401 | Acesso negado |
| `KeyNotFoundException` | 404 | Recurso nao encontrado |
| `DbUpdateConcurrencyException` | 409 | Conflito de concorrencia otimista |
| `DbUpdateException` (unique constraint) | 409 | Violacao de unicidade |
| `InvalidOperationException` | 422 | Regra de negocio violada |
| Qualquer outra | 500 | Erro inesperado |

---

## Migrations

| Migration | O que faz |
|---|---|
| `20260614211105_InitialCreate` | Schema inicial: Estabelecimentos, Products, Clients, Orders, OrderItems |
| `20260614222358_AddEstabelecimentoLogo` | Campo `LogoUrl` em Estabelecimentos |
| `20260615134836_AddClientAuthentication` | Campo `PasswordHash` em Clients |
| `20260615142217_AddClientFullAddress` | Endereco completo em Clients |
| `20260616145439_AddIntegrations` | Tabela Integrations |
| `20260620141928_AddUniqueOrderNumber` | Restricao unique em `Order.Number` |
| `20260620144545_AlignEstabelecimentoUpdatedAtWithDatabase` | Default de `UpdatedAt` no banco |
| `20260623132455_AddInventoryControl` | Tabela InventoryMovements e campos de estoque em Products |
| `20260624000000_AddCategories` | Tabela Categories, seed das 6 categorias, normalizacao de `Product.Category` |
| `20260626000651_AddDeliveryFee` | `Order.DeliveryFee`, `Order.OrderType` |
| `20260626162000_AddOrderTrackingWhatsAppSetting` | `Estabelecimento.SendOrderTrackingViaWhatsApp` |
| `20260627000001_AddProductPromotion` | `Product.IsOnPromotion`, `Product.PromotionalPrice` |
| `20260627212017_AddSocialMediaLinks` | Links de redes sociais em Estabelecimentos |
| `20260630120000_AddOrderDeliveryEstimate` | Campos de estimativa de entrega em Orders |
| `20260630123000_AddOrderDeliveryStartedAt` | `Order.DeliveryStartedAt` |
| `20260630124500_RemoveDeliveryAverageSpeedSetting` | Remove campo de velocidade media do Estabelecimento |
| `20260701011022_AddNeighborhoodDeliveryFees` | Tabela NeighborhoodDeliveryFees |
| `20260701012911_RemoveEstabelecimentoDeliveryFee` | Remove `DeliveryFee` do Estabelecimento (substituido por taxa por bairro) |

Para gerar nova migration:

```powershell
dotnet ef migrations add NomeDaMigration --project src\main\Infrastructure\Infrastructure.csproj --startup-project src\main\Api\Api.csproj --configuration Release
```

Use `--configuration Release` quando o backend estiver rodando em Debug para evitar bloqueio de DLL.
