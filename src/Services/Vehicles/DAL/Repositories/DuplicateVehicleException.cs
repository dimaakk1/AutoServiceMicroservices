namespace AutoserviceVehicle.DAL.Repositories;

public sealed class DuplicateVehicleException() : Exception("Автомобіль із цим VIN уже є у вашому списку.");
