using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace KeeperWpf; 

public class ShellViewModel : Screen, IShell
{
    private readonly IConfiguration _configuration;

    public int CarCount { get; private set; }

    public ShellViewModel(IConfiguration configuration, CarRepository carRepository)
    {
        _configuration = configuration;
        var backupFolder = Path.Combine(_configuration["DataFolder"] ?? "", "backup");

        ConvertFromTextFiles(backupFolder, carRepository).Wait();

        // Example usage of carRepository
        var cars = carRepository.GetAllCars().Result;
        CarCount = cars.Count;
    }

    private async Task ConvertFromTextFiles(string backupFolder, CarRepository carRepository)
    {
        KeeperModel? model = TxtLoader.LoadAllFromTextFiles(backupFolder);
        if (model == null)
        {
            // Handle loading error
            return;
        }

        foreach (var item in model.Cars)
        {
            await carRepository.AddCar(item);
        }

    }
}