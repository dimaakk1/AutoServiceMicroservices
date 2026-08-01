# Запуск через Docker Compose

Docker Compose та Aspire AppHost є двома незалежними способами запуску. Конфігурація Compose не змінює AppHost.

```powershell
docker compose up --build
```

Gateway буде доступний за адресою `http://localhost:8080`.

Якщо інфраструктуру вже запустив AppHost або потрібні інші порти, задай їх перед запуском:

```powershell
$env:SQLSERVER_PORT = "14330"
$env:MONGODB_PORT = "27018"
$env:REDIS_PORT = "6380"
$env:RABBITMQ_PORT = "5673"
$env:RABBITMQ_MANAGEMENT_PORT = "15673"
$env:GATEWAY_PORT = "8081"
docker compose up --build
```

Зупинка Compose без видалення даних:

```powershell
docker compose down
```

Для Aspire запускай `AppHostt/AppHostt.AppHost` як і раніше. Не запускай одночасно дві копії однієї інфраструктури на однакових портах; за потреби використовуй змінні портів вище для Compose.
