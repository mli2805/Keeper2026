using Caliburn.Micro;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;

namespace KeeperWpf 
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IConfiguration _configuration;

        public int CarCount { get; private set; }

        public ShellViewModel(IConfiguration configuration, CarRepository carRepository)
        {
            _configuration = configuration;

            // Example usage of carRepository
            var cars = carRepository.GetAllCars().Result;
            CarCount = cars.Count;
        }
    }
}