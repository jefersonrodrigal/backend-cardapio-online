# Cardapio Online — Backend

Backend da plataforma de cardapio online, construido em ASP.NET Core com arquitetura em camadas, EF Core e autenticacao JWT para as rotas administrativas.

## Visao geral

O projeto expoe uma API HTTP para:

- consultar e administrar categorias do cardapio
- consultar e administrar produtos vinculados a categorias
- controlar estoque e historico de movimentacoes
- cadastrar e autenticar clientes
- criar e acompanhar pedidos (com taxa de entrega automatica para pedidos do tipo Entrega)
- configurar dados do estabelecimento, incluindo taxa de entrega
- armazenar configuracoes de integracoes externas
- fazer upload de imagens servidas pelo proprio backend

O sistema separa claramente os fluxos publicos e administrativos:

- publico: categorias, cardapio, criacao de pedidos, cadastro/autenticacao de clientes e leitura do estabelecimento
- administrativo: login JWT, gestao de categorias, produtos, estoque, clientes, pedidos, uploads, estabelecimento e integracoes

## Stack tecnica

- .NET `10.0`
- ASP.NET Core Web API
- Entity Framework Core `9.0.5`
- SQL Server
- MediatR `12.5.0`
- FluentValidation `11.11.0`
- JWT Bearer Authentication
- OpenAPI nativo do ASP.NET Core

## Arquitetura

O codigo segue um estilo proximo de Clean Architecture / Onion Architecture.

- `src/main/Api`
  Camada de exposicao HTTP, configuracao de middleware, autenticacao, CORS e controllers.
- `src/main/Application`
  Casos de uso, commands, queries, DTOs, validacoes e pipeline behaviors.
- `src/main/Domain`
  Entidades e enums do dominio.
- `src/main/Infrastructure`
  Persistencia com EF Core, `DbContext`, configuracao de DI e migrations.

### Fluxo interno

1. O controller recebe a requisicao HTTP.
2. O controller delega a execucao para MediatR.
3. O pipeline de validacao roda via FluentValidation.
4. O handler acessa `IApplicationDbContext`.
5. O EF Core persiste ou consulta dados no SQL Server.
6. O resultado volta como DTO serializado em JSON.

## Estrutura do repositorio

```text
.
├─ src/
│  ├─ Backend.slnx
│  ├─ workload-install.ps1
│  └─ main/
│     ├─ Api/
│     │  ├─ Controllers/
│     │  │  ├─ AuthController.cs
│     │  │  ├─ CategoriesController.cs
│     │  │  ├─ ClientsController.cs
│     │  │  ├─ EstabelecimentoController.cs
│     │  │  ├─ IntegrationsController.cs
│     │  │  ├─ InventoryController.cs
│     │  │  ├─ OrdersController.cs
│     │  │  ├─ ProductsController.cs
│     │  │  └─ UploadsController.cs
│     │  └─ Middleware/
│     ├─ Application/
│     │  └─ Features/
│     │     ├─ Categories/
│     │     ├─ Clients/
│     │     ├─ Estabelecimento/
│     │     ├─ Integrations/
│     │     ├─ Inventory/
│     │     ├─ Orders/
│     │     └─ Products/
│     ├─ Domain/
│     │  ├─ Entities/
│     │  └─ Enums/
│     └─ Infrastructure/
│        ├─ Data/
│        └─ Migrations/
├─ .gitignore
└─ README.md
```

## Requisitos

- SDK do .NET `10.0`
- SQL Server acessivel pela string de conexao configurada
- PowerShell, se for usar os scripts utilitarios

## Como executar localmente

### 1. Restaurar dependencias

```powershell
dotnet restore src\Backend.slnx
```

### 2. Configurar secrets de desenvolvimento

O projeto usa `UserSecretsId` em `src/main/Api/Api.csproj`.

O jeito mais simples e usar o script:

```powershell
cd src\main\Api
.\init-dev-secrets.ps1
```

Opcionalmente:

```powershell
.\init-dev-secrets.ps1 -AdminEmail "admin@cardapioonline.local" -AdminPassword "SuaSenhaForte123!" -JwtSecret "chave-com-pelo-menos-32-caracteres"
```

O script grava os seguintes valores em User Secrets:

- `AdminAuth:Email`
- `AdminAuth:PasswordHash`
- `AdminAuth:JwtIssuer`
- `AdminAuth:JwtAudience`
- `AdminAuth:JwtSecret`
- `AdminAuth:TokenExpirationMinutes`

### 3. Ajustar a conexao com o banco

Por padrao, `src/main/Api/appsettings.json` aponta para:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CARDAPIOONLINE_DB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Se necessario, altere a connection string para o seu ambiente.

### 4. Executar a API

Na raiz:

```powershell
dotnet run --project src\main\Api\Api.csproj
```

Ou dentro de `src/main/Api`:

```powershell
dotnet run
```

### 5. URLs padrao de desenvolvimento

Segundo `launchSettings.json`:

- `http://localhost:5115`
- `https://localhost:7272`

### 6. OpenAPI

Em ambiente `Development`, a aplicacao chama `MapOpenApi()`. Em execucao local, a especificacao costuma ficar disponivel em:

- `http://localhost:5115/openapi/v1.json`

## Comandos uteis

```powershell
dotnet restore src\Backend.slnx
dotnet build src\Backend.slnx
dotnet run --project src\main\Api\Api.csproj
```

### Migrations

As migrations existentes ficam em `src/main/Infrastructure/Migrations`.

O startup executa automaticamente:

```csharp
await db.Database.MigrateAsync();
```

Ou seja, ao subir a API, as migrations pendentes sao aplicadas automaticamente no banco configurado.

Se precisar gerar novas migrations manualmente:

```powershell
dotnet ef migrations add NomeDaMigration --project src\main\Infrastructure\Infrastructure.csproj --startup-project src\main\Api\Api.csproj
```

## Configuracao

### `Api`

- `Api:BaseUrl`
  URL base publica da API. A aplicacao falha ao iniciar se esse valor estiver vazio.

### `AdminAuth`

- `AdminAuth:Email`
- `AdminAuth:PasswordHash`
- `AdminAuth:JwtIssuer`
- `AdminAuth:JwtAudience`
- `AdminAuth:JwtSecret`
- `AdminAuth:TokenExpirationMinutes`

Regras importantes:

- `JwtSecret` precisa ter pelo menos 32 caracteres
- `TokenExpirationMinutes` precisa ser maior que zero
- se qualquer chave obrigatoria estiver vazia, a API nao sobe

## Seguranca e autenticacao

### Admin

As rotas administrativas usam JWT Bearer.

Endpoint de login:

- `POST /api/Auth/login`

Payload:

```json
{
  "email": "admin@cardapioonline.local",
  "password": "Admin@123"
}
```

Resposta:

```json
{
  "token": "<jwt>",
  "expiresAt": "2026-06-20T20:00:00Z",
  "email": "admin@cardapioonline.local"
}
```

Use o token no header:

```http
Authorization: Bearer <jwt>
```

### Clientes

Clientes nao recebem JWT. A autenticacao do cliente retorna um `ClientDto` com os dados e resumo do historico.

Endpoint:

- `POST /api/Clients/authenticate`

## CORS

Existe uma policy chamada `Angular` permitindo apenas:

- `http://localhost:4200`

Com:

- qualquer header
- qualquer metodo

Se o frontend rodar em outra origem, sera necessario atualizar `Program.cs`.

## Uploads e arquivos estaticos

O backend chama `UseStaticFiles()` e salva imagens em `wwwroot/uploads/<yyyyMMdd>/`.

Endpoint:

- `POST /api/Uploads/image`

Regras:

- requer autenticacao
- limite de requisicao: `10_000_000` bytes
- aceita apenas `Content-Type` iniciado por `image/`

Resposta:

```json
{
  "url": "http://localhost:5115/uploads/20260620/arquivo.png"
}
```

## Banco de dados

### Provider

- SQL Server via `UseSqlServer`

### Comportamento da connection string

A camada de infraestrutura normaliza a connection string:

- remove `User ID` e `Password` quando `Integrated Security=True`
- forza `TrustServerCertificate=True`
- forza `MultipleActiveResultSets=True`
- habilita retry automatico com `EnableRetryOnFailure(3)`

### Entidades principais

- `Category`
  Categoria do cardapio com slug unico gerado automaticamente do nome, nome de exibicao e ordem de apresentacao. Categorias sao usadas para organizar os produtos no cardapio publico e no painel administrativo.
- `Estabelecimento`
  Dados publicos do restaurante/loja: logo, categoria, endereco, WhatsApp, horarios, taxa de entrega (`DeliveryFee`) e flag `SendOrderTrackingViaWhatsApp` para disparo automatico de atualizacoes de status via WhatsApp.
- `Product`
  Produto do cardapio com nome, descricao, preco, slug de categoria, imagem, flag `IsActive`, configuracoes de estoque e suporte a promocao (`IsOnPromotion`, `PromotionalPrice`).
- `InventoryMovement`
  Historico auditavel de entradas, vendas, cancelamentos, ajustes e perdas de estoque.
- `Client`
  Cliente com contato, endereco completo e senha hash.
- `Order`
  Pedido com numero unico, cliente vinculado opcionalmente, endereco, origem, tipo (`OrderType`), taxa de entrega (`DeliveryFee`), status, total e itens.
- `OrderItem`
  Snapshot dos itens do pedido, incluindo produto vinculado, nome do produto, quantidade e preco unitario. O preco unitario reflete o preco efetivo no momento da compra: preco promocional quando o produto esta em promocao, preco normal caso contrario.
- `Integration`
  Configuracoes de integracoes externas em uma estrutura unificada.

## Categorias de produto

As categorias sao entidades dinamicas gerenciadas via CRUD administrativo. O slug e gerado automaticamente a partir do nome no momento da criacao (ex: "Hamburguer" → `hamburguer`, "Porcao" → `porcao`) e nao pode ser alterado posteriormente, garantindo estabilidade das referencias nos produtos.

As 6 categorias iniciais sao criadas automaticamente pela migration:

| Slug | Nome | Ordem |
|---|---|---|
| `hamburguer` | Hamburguer | 1 |
| `pizza` | Pizza | 2 |
| `bebida` | Bebida | 3 |
| `sobremesa` | Sobremesa | 4 |
| `porcao` | Porcao | 5 |
| `outro` | Outro | 6 |

Uma categoria nao pode ser excluida enquanto houver produtos ativos vinculados a ela.

## Enums do dominio

### Status do pedido

- `Pendente`
- `EmPreparo`
- `EmEntrega`
- `Entregue`
- `Cancelado`

### Origem do pedido

- `WhatsApp`
- `IFood`
- `Site`

### Providers de integracao

- `IFood`
- `Anotai`
- `UberEats`
- `NinetyNineFood`
- `AiAgents`
- `WhatsApp`
- `TakeBlip`
- `Zenvia`

## Paginacao

Listagens de produtos, clientes e pedidos retornam `PaginatedResult<T>`:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 5,
  "total": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

Parametros comuns:

- `page`
- `pageSize`

## Endpoints

### Auth

- `POST /api/Auth/login`
  Login administrativo e emissao de JWT.

### Categories

- `GET /api/Categories`
  Publico. Lista categorias ativas ordenadas por `SortOrder`.
- `POST /api/Categories`
  Protegido. Cria categoria. O slug e gerado automaticamente do nome.
- `PUT /api/Categories/{id}`
  Protegido. Atualiza nome e ordem. O slug nao e alterado.
- `DELETE /api/Categories/{id}`
  Protegido. Exclui categoria. Falha com 422 se houver produtos ativos na categoria.

Resposta de listagem:

```json
[
  { "id": 1, "slug": "hamburguer", "name": "Hamburguer", "sortOrder": 1 },
  { "id": 2, "slug": "bebida",     "name": "Bebida",     "sortOrder": 3 }
]
```

Payload de criacao:

```json
{
  "name": "Sobremesa",
  "sortOrder": 4
}
```

Payload de atualizacao:

```json
{
  "name": "Sobremesas Especiais",
  "sortOrder": 4
}
```

### Products

- `GET /api/Products`
  Publico. Lista produtos ativos com paginacao.
  Filtros: `page`, `pageSize`, `category` (slug da categoria)
- `POST /api/Products`
  Protegido. Cria produto.
- `PUT /api/Products/{id}`
  Protegido. Atualiza produto.
- `DELETE /api/Products/{id}`
  Protegido. Faz soft delete, desativando o produto.

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

O campo `category` recebe o slug da categoria (minusculas). Valores sao normalizados para minusculas antes da persistencia.

Os campos de promocao seguem estas regras:
- `isOnPromotion: false` ou omitido: produto sem promocao; `promotionalPrice` e ignorado e salvo como `null`
- `isOnPromotion: true`: `promotionalPrice` e obrigatorio, deve ser maior que zero e menor que `price`
- quando o produto esta em promocao, o pedido grava o `promotionalPrice` como `UnitPrice` no `OrderItem`

### Clients

- `POST /api/Clients`
  Publico. Cadastra cliente.
- `POST /api/Clients/authenticate`
  Publico. Autentica cliente por email e senha.
- `GET /api/Clients`
  Protegido. Lista clientes.
  Filtros: `page`, `pageSize`, `search`

Payload de cadastro:

```json
{
  "name": "Maria Silva",
  "email": "maria@email.com",
  "phone": "11999999999",
  "zipCode": "01234-567",
  "street": "Rua A",
  "number": "123",
  "neighborhood": "Centro",
  "city": "Sao Paulo",
  "state": "SP",
  "complement": "Apto 12",
  "password": "SenhaSegura123"
}
```

Payload de autenticacao:

```json
{
  "email": "maria@email.com",
  "password": "SenhaSegura123"
}
```

### Orders

- `POST /api/Orders`
  Publico. Cria pedido.
- `GET /api/Orders`
  Protegido. Lista pedidos.
  Filtros: `page`, `pageSize`, `date` no formato aceito por `DateOnly.TryParse`
- `PUT /api/Orders/{id}/advance`
  Protegido. Avanca o status do pedido.
- `PUT /api/Orders/{id}/cancel`
  Protegido. Cancela o pedido.

Payload de criacao:

```json
{
  "clientName": "Maria Silva",
  "clientPhone": "11999999999",
  "address": "Rua A, 123 - Centro - Sao Paulo/SP",
  "source": "Site",
  "orderType": "Entrega",
  "note": "Sem cebola",
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000000",
      "quantity": 2
    }
  ]
}
```

O campo `orderType` aceita os valores `Entrega`, `Retirada` e `ConsumoLocal`. Quando omitido ou nulo, o pedido e tratado como entrega.

Notas tecnicas:

- o numero do pedido e gerado automaticamente no formato `P<timestamp><sufixoHex>`
- apenas produtos ativos podem entrar no pedido
- se existir cliente com o mesmo telefone, o pedido e vinculado a ele
- o total e calculado no backend: `soma dos itens + taxa de entrega (quando aplicavel)`
- a taxa de entrega e lida do registro do `Estabelecimento` no momento da criacao do pedido e gravada como snapshot no campo `DeliveryFee` do pedido
- pedidos com `orderType` igual a `Entrega` (ou sem `orderType`) recebem a taxa; `Retirada` e `ConsumoLocal` nao recebem taxa
- produtos com controle de estoque baixam quantidade na criacao do pedido
- cancelamentos de pedidos ainda nao entregues repõem o estoque dos itens controlados

### Inventory

- `GET /api/Inventory`
  Protegido. Lista produtos ativos com dados de estoque.
  Filtros: `page`, `pageSize`, `status`, `search`
- `GET /api/Inventory/movements`
  Protegido. Lista movimentacoes de estoque.
  Filtros: `productId`, `page`, `pageSize`
- `POST /api/Inventory/movements`
  Protegido. Registra entrada, perda ou ajuste manual.

Payload de movimentacao:

```json
{
  "productId": "00000000-0000-0000-0000-000000000000",
  "type": "entrada",
  "quantity": 10,
  "newQuantity": null,
  "reason": "Compra de reposicao"
}
```

### Estabelecimento

- `GET /api/Estabelecimento`
  Publico. Retorna os dados atuais do estabelecimento.
- `PUT /api/Estabelecimento`
  Protegido. Cria ou atualiza a configuracao.

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
  "deliveryFee": 5.00
}
```

O campo `deliveryFee` e obrigatorio no payload. Use `0` para nao cobrar taxa. Valores negativos sao normalizados para `0`.

### Integrations

Todos os endpoints de integracoes requerem autenticacao.

- `GET /api/Integrations`
  Retorna o overview de todas as integracoes.
- `PUT /api/Integrations/ifood`
- `PUT /api/Integrations/anotai`
- `PUT /api/Integrations/ubereats`
- `PUT /api/Integrations/99food`
- `PUT /api/Integrations/aiagents`
- `PUT /api/Integrations/whatsapp`
- `PUT /api/Integrations/takeblip`
- `PUT /api/Integrations/zenvia`

Payloads esperados:

- `ifood` — `{ "enabled", "clientId", "clientSecret", "merchantId" }`
- `anotai` — `{ "enabled", "apiToken", "accountId", "webhookUrl" }`
- `ubereats` — `{ "enabled", "clientId", "clientSecret", "storeId", "webhookSigningSecret" }`
- `99food` — `{ "enabled", "clientId", "clientSecret", "storeId", "webhookUrl" }`
- `aiagents` — `{ "enabled", "provider", "apiKey", "model", "assistantId", "webhookUrl" }`
- `whatsapp` — `{ "enabled", "phoneNumberId", "businessAccountId", "accessToken", "appSecret", "verifyToken" }`
- `takeblip` — `{ "enabled", "botShortName", "authorizationKey", "webhookUrl" }`
- `zenvia` — `{ "enabled", "apiToken", "channelId", "webhookUrl" }`

### Uploads

- `POST /api/Uploads/image`
  Protegido. Faz upload de imagem e retorna URL publica.

## Regras de negocio observadas

- categorias nao podem ser excluidas enquanto houver produtos ativos vinculados
- slugs de categoria sao gerados automaticamente a partir do nome e sao imutaveis
- produtos sao retornados apenas se `IsActive = true`
- exclusao de produto e logica, nao fisica
- clientes precisam de email unico e telefone unico
- senha de admin e senha de cliente sao armazenadas com PBKDF2 SHA-256
- pedidos usam snapshot do nome e preco efetivo do produto no momento da compra
- o preco efetivo e o `PromotionalPrice` quando `IsOnPromotion = true`, ou o `Price` normal caso contrario
- `PromotionalPrice` so e aceito quando `IsOnPromotion = true` e deve ser maior que zero e menor que `Price`; quando `IsOnPromotion` e falso, `PromotionalPrice` e zerado no backend independentemente do valor enviado
- a taxa de entrega aplicada ao pedido e um snapshot do valor configurado no estabelecimento no momento da criacao; alteracoes futuras na taxa nao afetam pedidos ja criados
- a taxa de entrega incide apenas sobre pedidos do tipo `Entrega` ou sem `orderType`; `Retirada` e `ConsumoLocal` tem `DeliveryFee = 0`
- o `Total` do pedido ja inclui a taxa de entrega: `Total = soma(itens com preco efetivo) + DeliveryFee`
- listagem de clientes conta apenas pedidos nao cancelados em `ordersCount`
- `totalSpent` do cliente soma apenas pedidos `Entregue` (incluindo a taxa de entrega, pois o `Total` ja a contempla)
- a configuracao de estabelecimento e tratada como registro unico

## Tratamento de erros

O middleware global padroniza respostas:

- `400 Bad Request`
  Erros de validacao do FluentValidation
- `401 Unauthorized`
  Falha de autenticacao ou acesso indevido
- `404 Not Found`
  Recurso nao encontrado
- `409 Conflict`
  Violacao de unicidade no banco
- `422 Unprocessable Entity`
  Regras de negocio disparando `InvalidOperationException` (ex: excluir categoria com produtos ativos)
- `500 Internal Server Error`
  Erro inesperado

Exemplos:

```json
{
  "error": "Nao e possivel excluir uma categoria que possui produtos ativos."
}
```

```json
{
  "errors": [
    {
      "propertyName": "Name",
      "errorMessage": "'Name' must not be empty."
    }
  ]
}
```

## Observabilidade

### Logging

`appsettings.json` define:

- `Default`: `Information`
- `Microsoft.AspNetCore`: `Warning`

Em `Development`, tambem ha:

- `Microsoft.EntityFrameworkCore.Database.Command`: `Information`

Isso faz com que queries SQL aparecam no log local durante desenvolvimento.

## Migrations

| Migration | Descricao |
|---|---|
| `20260614211105_InitialCreate` | Schema inicial com Estabelecimento, Products, Clients, Orders e OrderItems |
| `20260614222358_AddEstabelecimentoLogo` | Campo `LogoUrl` no Estabelecimento |
| `20260615134836_AddClientAuthentication` | Autenticacao de clientes com hash de senha |
| `20260615142217_AddClientFullAddress` | Endereco completo do cliente |
| `20260616145439_AddIntegrations` | Tabela de integracoes externas |
| `20260620141928_AddUniqueOrderNumber` | Numero unico de pedido |
| `20260620144545_AlignEstabelecimentoUpdatedAtWithDatabase` | Ajuste de `UpdatedAt` gerado pelo banco |
| `20260623132455_AddInventoryControl` | Controle de estoque e movimentacoes |
| `20260624000000_AddCategories` | Tabela `Categories`, seed das 6 categorias iniciais e normalizacao dos slugs em `Products` |
| `20260626000651_AddDeliveryFee` | Campo `DeliveryFee` em `Estabelecimentos` e campos `DeliveryFee` + `OrderType` em `Orders` |
| `20260626162000_AddOrderTrackingWhatsAppSetting` | Flag `SendOrderTrackingViaWhatsApp` no Estabelecimento |
| `20260627000001_AddProductPromotion` | Campos `IsOnPromotion` e `PromotionalPrice` em `Products` |

## Pontos de atencao

- `appsettings.json` deixa `AdminAuth:PasswordHash` e `AdminAuth:JwtSecret` vazios por padrao, entao a API depende de User Secrets ou configuracao equivalente
- o CORS esta restrito a `http://localhost:4200`; para producao, atualizar `Program.cs`
- como as migrations sao aplicadas automaticamente no startup, a conexao com o banco precisa estar correta antes de subir a API
- o campo `category` nos produtos armazena slugs em minusculas; slugs fora da tabela de categorias ficam visiveis apenas nos filtros, mas nao aparecem no cardapio publico

## Sugestoes de proximos passos

- adicionar testes automatizados para handlers e controllers
- versionar um `appsettings.Example.json` ou `.env` equivalente para onboarding
- publicar uma colecao de requests atualizada
- considerar mascaramento de segredos nas respostas de integracoes
- restringir CORS para a origem de producao do frontend
