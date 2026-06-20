# Cardapio Online Backend

Backend da plataforma de cardapio online, construido em ASP.NET Core com arquitetura em camadas, EF Core e autenticacao JWT para as rotas administrativas.

## Visao geral

O projeto expoe uma API HTTP para:

- consultar e administrar produtos
- cadastrar e autenticar clientes
- criar e acompanhar pedidos
- configurar dados do estabelecimento
- armazenar configuracoes de integracoes externas
- fazer upload de imagens servidas pelo proprio backend

O sistema separa claramente os fluxos publicos e administrativos:

- publico: cardapio, criacao de pedidos, cadastro/autenticacao de clientes e leitura do estabelecimento
- administrativo: login JWT, gestao de produtos, clientes, pedidos, uploads, estabelecimento e integracoes

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
│     ├─ Application/
│     ├─ Domain/
│     └─ Infrastructure/
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

Clientes nao recebem JWT. A autenticacao do cliente hoje retorna um `ClientDto` com os dados e resumo do historico.

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

- `Estabelecimento`
  Dados publicos do restaurante/loja, logo, categoria, endereco, WhatsApp e horarios.
- `Product`
  Produto do cardapio com nome, descricao, preco, categoria, imagem e flag `IsActive`.
- `Client`
  Cliente com contato, endereco completo e senha hash.
- `Order`
  Pedido com numero unico, cliente vinculado opcionalmente, endereco, origem, status, total e itens.
- `OrderItem`
  Snapshot dos itens do pedido, incluindo nome do produto, quantidade e preco unitario.
- `Integration`
  Configuracoes de integracoes externas em uma estrutura unificada.

## Enums do dominio

### Categorias de produto

- `Hamburguer`
- `Pizza`
- `Bebida`
- `Sobremesa`
- `Porcao`
- `Outro`

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

### Products

- `GET /api/Products`
  Publico. Lista produtos ativos com paginacao.
  Filtros: `page`, `pageSize`, `category`
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
  "category": "Hamburguer",
  "imageUrl": "http://localhost:5115/uploads/20260620/img.png"
}
```

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
  "note": "Sem cebola",
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000000",
      "quantity": 2
    }
  ]
}
```

Notas tecnicas:

- o numero do pedido e gerado automaticamente no formato `P<timestamp><sufixoHex>`
- apenas produtos ativos podem entrar no pedido
- se existir cliente com o mesmo telefone, o pedido e vinculado a ele
- o total e calculado no backend

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
  "closeTime": "23:59"
}
```

### Integrations

- `GET /api/Integrations`
  Protegido. Retorna o overview de todas as integracoes.
- `PUT /api/Integrations/ifood`
- `PUT /api/Integrations/anotai`
- `PUT /api/Integrations/ubereats`
- `PUT /api/Integrations/99food`
- `PUT /api/Integrations/aiagents`
- `PUT /api/Integrations/whatsapp`
- `PUT /api/Integrations/takeblip`
- `PUT /api/Integrations/zenvia`

Payloads esperados:

- `ifood`

```json
{
  "enabled": true,
  "clientId": "xxx",
  "clientSecret": "yyy",
  "merchantId": "zzz"
}
```

- `anotai`

```json
{
  "enabled": true,
  "apiToken": "xxx",
  "accountId": "yyy",
  "webhookUrl": "https://example.com/webhook"
}
```

- `ubereats`

```json
{
  "enabled": true,
  "clientId": "xxx",
  "clientSecret": "yyy",
  "storeId": "zzz",
  "webhookSigningSecret": "abc"
}
```

- `99food`

```json
{
  "enabled": true,
  "clientId": "xxx",
  "clientSecret": "yyy",
  "storeId": "zzz",
  "webhookUrl": "https://example.com/webhook"
}
```

- `aiagents`

```json
{
  "enabled": true,
  "provider": "openai",
  "apiKey": "xxx",
  "model": "gpt-4.1",
  "assistantId": "asst_123",
  "webhookUrl": "https://example.com/webhook"
}
```

- `whatsapp`

```json
{
  "enabled": true,
  "phoneNumberId": "xxx",
  "businessAccountId": "yyy",
  "accessToken": "zzz",
  "appSecret": "abc",
  "verifyToken": "token"
}
```

- `takeblip`

```json
{
  "enabled": true,
  "botShortName": "meu-bot",
  "authorizationKey": "key",
  "webhookUrl": "https://example.com/webhook"
}
```

- `zenvia`

```json
{
  "enabled": true,
  "apiToken": "xxx",
  "channelId": "yyy",
  "webhookUrl": "https://example.com/webhook"
}
```

### Uploads

- `POST /api/Uploads/image`
  Protegido. Faz upload de imagem e retorna URL publica.

## Regras de negocio observadas

- produtos sao retornados apenas se `IsActive = true`
- exclusao de produto e logica, nao fisica
- clientes precisam de email unico e telefone unico
- senha de admin e senha de cliente sao armazenadas com PBKDF2 SHA-256
- pedidos usam snapshot do nome e preco do produto no momento da compra
- listagem de clientes conta apenas pedidos nao cancelados em `ordersCount`
- `totalSpent` do cliente soma apenas pedidos `Entregue`
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
  Regras de negocio disparando `InvalidOperationException`
- `500 Internal Server Error`
  Erro inesperado

Exemplos:

```json
{
  "error": "Ja existe um cliente cadastrado com este e-mail."
}
```

```json
{
  "errors": [
    {
      "propertyName": "Password",
      "errorMessage": "'Password' must not be empty."
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

## Estado atual do projeto

- a solucao principal e `src/Backend.slnx`
- existem migrations versionadas para criacao inicial e evolucao de clientes, logo, integracoes e numero unico de pedido
- a API compila com sucesso em `2026-06-20`
- nao foram encontrados projetos de teste automatizado no repositorio

## Validacao realizada

Comando executado com sucesso:

```powershell
dotnet build src\Backend.slnx
```

## Pontos de atencao

- `appsettings.json` deixa `AdminAuth:PasswordHash` e `AdminAuth:JwtSecret` vazios por padrao, entao a API depende de User Secrets ou configuracao equivalente
- o CORS esta restrito a `http://localhost:4200`
- o endpoint `Api.http` ainda contem o exemplo padrao de `weatherforecast` e nao representa a API atual
- como as migrations sao aplicadas automaticamente no startup, a conexao com o banco precisa estar correta antes de subir a API

## Sugestoes de proximos passos

- adicionar testes automatizados para handlers e controllers
- versionar um `appsettings.Example.json` ou `.env` equivalente para onboarding
- publicar uma colecao de requests atualizada no lugar do `Api.http` padrao
- considerar mascaramento de segredos nas respostas de integracoes, se essa API for consumida por clientes menos confiaveis

