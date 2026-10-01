# Prueba Técnica .NET 10 - API REST Minimal API

## Requisitos Técnicos
* **Versión de .NET:** 10.0 (SDK 10 o superior)
* **Base de Datos:** SQLite (Entity Framework Core)
* **Arquitectura / Patrones:** CQRS con MediatR, Middleware de Seguridad, FluentValidation.

---

## Cómo Ejecutar el Proyecto

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/MrDovido/PruebaTecnica
   cd PruebaTecnica
   ```

2. **Instalar .NET:**
   [https://dotnet.microsoft.com/es-es/download/dotnet/10.0](https://dotnet.microsoft.com/es-es/download/dotnet/10.0)

3. **Correr el programa:**
   ```bash
   dotnet run
   ```

---

## Cómo Crear/Aplicar migraciones SQLite

* Al correr el programa (`dotnet run`) se ejecuta automáticamente la creación y migración de los datos (`app.db`).

---

## API Key de prueba y ejemplos de Headers

Todas las peticiones requieren la cabecera de autenticación `x-api-key: SecretApiKey123456`.

### Usuarios (Users)

* **Crear Usuario (POST):**
  ```bash
  curl --location 'http://localhost:5177/users' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456' \
  --data-raw '{
    "name": "David Baez",
    "email": "David@test.com",
    "password": "123456"
  }'
  ```

* **Listar Usuarios (GET):**
  ```bash
  curl --location 'http://localhost:5177/users' \
  --header 'Content-Type: application/json' \
  --header 'X-API-KEY: SecretApiKey123456'
  ```

* **Listar Usuario Individual (GET):**
  ```bash
  curl --location 'http://localhost:5177/users/1' \
  --header 'Content-Type: application/json' \
  --header 'X-API-KEY: SecretApiKey123456'
  ```

* **Modificar Usuario (PUT):**
  ```bash
  curl --location --request PUT 'http://localhost:5177/users/1' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456' \
  --data-raw '{
    "name": "Juan Pérez Modificado",
    "email": "juan.perez@email.com",
    "isActive": true
  }'
  ```

* **Eliminar Usuario (DELETE):**
  ```bash
  curl --location --request DELETE 'http://localhost:5177/users/1' \
  --header 'Content-Type: application/json' \
  --header 'X-API-KEY: SecretApiKey123456'
  ```

---

### Direcciones (Addresses)

* **Crear Dirección para un Usuario (POST):**
  ```bash
  curl --location 'http://localhost:5177/users/1/addresses' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456' \
  --data '{
    "street": "Calle Pitiantuta 1234",
    "city": "Asunción",
    "country": "Paraguay",
    "zipCode": "9999"
  }'
  ```

* **Consultar Dirección por Usuario (GET):**
  ```bash
  curl --location 'http://localhost:5177/users/2/addresses' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456'
  ```

* **Modificar Dirección (PUT):**
  ```bash
  curl --location --request PUT 'http://localhost:5177/addresses/2' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456' \
  --data '{
    "street": "Calle modificada 1",
    "city": "CDE",
    "country": "Paraguay",
    "zipCode": "1209"
  }'
  ```

* **Eliminar Dirección (DELETE):**
  ```bash
  curl --location --request DELETE 'http://localhost:5177/addresses/1' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456'
  ```

---

### Conversión de Divisas (Currency)

* **Listar Monedas (GET):**
  ```bash
  curl --location 'http://localhost:5177/currencies' \
  --header 'Content-Type: application/json' \
  --header 'X-API-KEY: SecretApiKey123456'
  ```

* **Crear Moneda (POST):**
  ```bash
  curl --location 'http://localhost:5177/currencies' \
  --header 'Content-Type: application/json' \
  --header 'x-api-key: SecretApiKey123456' \
  --data '{
    "code": "ARS",
    "name": "Peso Argentino",
    "rateToBase": 3.82
  }'
  ```

* **Conversión de Divisas ARS a PYG (POST):**
  ```bash
  curl --location 'http://localhost:5177/currency/convert' \
  --header 'Content-Type: application/json' \
  --header 'X-API-KEY: SecretApiKey123456' \
  --data '{
    "fromCurrencyCode": "ARS",
    "toCurrencyCode": "PYG",
    "amount": 100
  }'
  ```

* **Conversión de Divisas USD a PYG (POST):**
  ```bash
  curl --location 'http://localhost:5177/currency/convert' \
  --header 'Content-Type: application/json' \
  --header 'X-API-KEY: SecretApiKey123456' \
  --data '{
    "fromCurrencyCode": "USD",
    "toCurrencyCode": "PYG",
    "amount": 100
  }'
  ```

* **Aclaración**
**Borrado en cascada**: Me falto agregar el borrado en cascada, para cuando se elimina el usuario, tambien borrar sus direcciones asociadas.
**ZipCode en address**: Se utilizo el zipcode como numero de casa.

