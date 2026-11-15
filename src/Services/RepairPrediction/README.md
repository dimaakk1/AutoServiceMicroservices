# RepairPrediction — прогноз тривалості ремонту та ризику затримки

Тут є навчання, оцінка та локальний gRPC-сервер. Сідер і експорт CSV залишаються в `tools/MlSeed`. AppHost запускає підготовлений сервер разом із `Orders`; Docker Compose поки його не запускає.

| Файл | Призначення |
|---|---|
| `dataset.py` | Перевіряє CSV, готує ознаки й розділяє замовлення за датою. |
| `train_duration.py` | Навчає базову модель, Ridge і випадковий ліс; підбирає параметри та зберігає результат. |
| `train_delay.py` | Порівнює логістичну регресію та випадковий ліс із базовим прогнозом ризику затримки. |
| `requirements.txt` | Фіксує версії scikit-learn і gRPC. |
| `prediction_server.py` | Завантажує готові моделі один раз і обслуговує `PredictRepair`. |
| `test_prediction_server.py` | Перевіряє виклики gRPC на локальному сервері з навченими моделями. |

Запуск із кореня репозиторію на Python 3.12:

```powershell
python -m venv artifacts/ml/training-venv
& .\artifacts\ml\training-venv\Scripts\python.exe -m pip install -r src/Services/RepairPrediction/requirements.txt
& .\artifacts\ml\training-venv\Scripts\python.exe src/Services/RepairPrediction/train_duration.py
& .\artifacts\ml\training-venv\Scripts\python.exe src/Services/RepairPrediction/train_delay.py
```

Вхідний файл — `artifacts/ml/repair-orders.csv`. Щоб змінити шлях, передай `--input`; для іншої папки результатів — `--output`.

1. Ознаки: вік авто на дату запису, пробіг, майстер, день тижня, година, кількість послуг та окремий індикатор для кожної послуги. Ідентифікатор замовлення, мітка затримки та плановий строк не використовуються для прогнозу тривалості.
2. Найстаріші приблизно 70% замовлень — навчання, наступні 15% — валідація, найновіші 15% — тест. Записи одного дня залишаються разом. Перетворення ознак навчається лише разом із моделлю на дозволеній для навчання частині.
3. Базовий прогноз — медіана тривалості. Для Ridge підбирається `alpha`, для випадкового лісу — глибина й мінімальна кількість прикладів у листку. Переможець визначається за валідаційною MAE (середньою абсолютною похибкою в хвилинах). Після вибору моделі й параметрів конфігурації перенавчаються на навчанні та валідації; фінальний тест використовується лише для звіту.

Результати в `artifacts/ml/duration`: `duration.joblib` — обрана модель із перетворенням ознак; `metrics.json` — межі вибірок, параметри, похибки та контрольна сума CSV. Завантажувати через joblib слід лише власні довірені файли. Результати на синтетичних замовленнях не підтверджують точність на реальних ремонтах.

## Ризик затримки

Затримка (`delayed = 1`) означає, що ремонт завершився пізніше погодженого терміну. Скрипт використовує той самий поділ за датою й додає `planned_budget_minutes` — кількість хвилин від запису до погодженого завершення. Фактична тривалість і результат ремонту не потрапляють до ознак.

Базова модель дає однаковий прогноз для всіх замовлень — частку затримок у даних навчання. Для логістичної регресії підбирається `C`, для лісу — максимальна глибина й мінімальна кількість прикладів у листку. Найкраща з двох моделей обирається за Brier score на валідації — середнім квадратом різниці між прогнозованою ймовірністю та міткою 0 або 1. Менше — краще. Після вибору конфігурації перенавчаються на навчанні й валідації, а тест використовується лише для підсумкової оцінки.

У `artifacts/ml/delay/metrics.json` поле `models.*.trials` містить усі перевірені параметри, а `models.*.test` — Brier score, ROC AUC, precision, recall і матрицю помилок при порозі 0,5. У `artifacts/ml/delay/delay.joblib` зберігається обрана модель разом із перетворенням ознак.

## Локальний gRPC API

Контракт — `contracts/protobuf/repair_prediction.proto`. З кореня репозиторію встанови залежності й згенеруй Python-код. Якщо використовуєш venv PyCharm, підстав його `python.exe` замість наведеного шляху.

```powershell
$predictionPython = '.\artifacts\ml\training-venv\Scripts\python.exe'
& $predictionPython -m pip install -r src/Services/RepairPrediction/requirements.txt
& $predictionPython -m grpc_tools.protoc -I contracts/protobuf --python_out=src/Services/RepairPrediction --grpc_python_out=src/Services/RepairPrediction contracts/protobuf/repair_prediction.proto
```

Згенеровані `_pb2.py` та `_pb2_grpc.py` не додаються до Git. Повтори генерацію після зміни `.proto`.

Якщо обидва файли моделей уже є, перенавчати їх для запуску не потрібно:

```powershell
& $predictionPython src/Services/RepairPrediction/prediction_server.py
```

Сервер слухає лише `127.0.0.1:50051` без TLS для локального запуску. Порт можна змінити через `--port`. Параметр `--model-dir` задає батьківську папку для `duration/duration.joblib` і `delay/delay.joblib` (типово `artifacts/ml`). Використовуй лише довірені файли моделей.

`PredictRepair` приймає місцевий час запису у форматі `2026-10-01T10:00:00` без зміщення UTC, ID майстра, рік випуску, пробіг, унікальні ID послуг і загальну кількість одиниць послуг. Завжди повертає `estimated_duration_minutes`. Якщо передати `planned_budget_minutes`, повертає також `delay_probability` від 0 до 1. Якщо плану немає, не передавай це поле: у відповіді ризик буде відсутнім, а не нульовим. Некоректні вхідні дані повертають `INVALID_ARGUMENT`.

Тест сам запускає й зупиняє сервер на вільному локальному порту:

```powershell
& $predictionPython -m unittest discover -s src/Services/RepairPrediction -p 'test_prediction_server.py' -v
```

## Запуск через AppHost

Один раз підготуй залежності, згенеруй gRPC-код (команди вище) і переконайся, що обидві моделі лежать у `artifacts/ml`. Потім запускай AppHost у режимі Development: у панелі Aspire має з’явитися `repair-prediction`. Вручну запускати Python або відкривати PyCharm не потрібно.

AppHost шукає Python у `src/Services/RepairPrediction/.venv`, `artifacts/ml/training-venv` і `artifacts/ml/.venv` (у такому порядку). Для іншого середовища можна задати повний шлях до інтерпретатора через `RepairPrediction__PythonExecutable`. AppHost виділяє порт, задає `REPAIR_PREDICTION_PORT` для Python і передає gRPC-адресу `Orders` через `Services__RepairPredictionUrl`.

`Orders` не чекає на Python при запуску. Запис на ремонт не викликає ML; прогноз запитується окремо через `Orders.API` (також доступний через Gateway):

- `GET /api/Orders/Order/{id}/estimated-duration` — власник замовлення отримує `{ "estimatedDurationMinutes": 94.5 }`. Чуже замовлення повертає `404`, як і неіснуюче.
- `GET /api/Orders/Order/{id}/delay-risk` — тільки `Admin` отримує `{ "delayProbability": 0.35 }` (від 0 до 1). Без погодженого терміну відповідь `204` без тіла.

Обидва маршрути потребують автентифікації. Якщо бракує даних для моделі (наприклад, пробігу чи призначеного майстра) або gRPC недоступний, прогноз повертає `204` без тіла, а не число `0`.
