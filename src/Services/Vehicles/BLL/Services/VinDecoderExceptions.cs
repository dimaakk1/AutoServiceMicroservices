namespace AutoserviceVehicle.BLL.Services;

public sealed class VinNotFoundException() : Exception("Не вдалося знайти достатньо інформації про автомобіль за цим VIN.");

public sealed class VinProviderUnavailableException(Exception? innerException = null)
    : Exception("Сервіс VIN-декодування тимчасово недоступний. Спробуйте пізніше.", innerException);
