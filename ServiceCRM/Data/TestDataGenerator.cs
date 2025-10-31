using Microsoft.EntityFrameworkCore;
using ServiceCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCRM.Data
{
    public class TestDataGenerator
    {
        private readonly ServiceCrmContext _context;
        private readonly Random _random = new Random();

        private readonly string[] _deviceTypes = new[]
        {
            "Смартфон", "Ноутбук", "Планшет", "Компьютер", "Монитор",
            "Принтер", "Телевизор", "Фотоаппарат", "Игровая консоль", "Умные часы"
        };

        private readonly string[] _brands = new[]
        {
            "Apple", "Samsung", "Xiaomi", "Huawei", "Lenovo",
            "Asus", "Dell", "HP", "Sony", "LG", "Canon", "Nikon"
        };

        private readonly string[] _models = new[]
        {
            "iPhone 13", "Galaxy S21", "Redmi Note 10", "MateBook", "ThinkPad",
            "ZenBook", "XPS 13", "Pavilion", "Bravia", "Gram", "EOS R5", "Z7"
        };

        private readonly string[] _issues = new[]
        {
            "Не включается", "Разбит экран", "Не работает кнопка питания",
            "Проблемы с батареей", "Не заряжается", "Не работает Wi-Fi",
            "Попала вода", "Глючит система", "Не работает динамик",
            "Треснул корпус", "Перегревается", "Медленно работает"
        };

        private readonly string[] _counterparties = new[]
        {
            "Иванов Иван", "Петров Петр", "Сидорова Анна", "Кузнецов Алексей",
            "Смирнова Мария", "Попов Дмитрий", "Васильева Екатерина",
            "ООО ТехноСервис", "ИП Сидоров", "ЗАО КомпьютерМир"
        };

        public TestDataGenerator(ServiceCrmContext context)
        {
            _context = context;
        }

        public async Task GenerateTestOrdersAsync(int count = 100, int serviceCenterId = 1)
        {
            var serviceCenter = await _context.ServiceCenters
                .FirstOrDefaultAsync(sc => sc.Id == serviceCenterId);

            if (serviceCenter == null)
            {
                throw new ArgumentException("Service center not found");
            }

            var orders = new List<Order>();
            var startDate = DateTime.Now.AddMonths(-6); // Данные за последние 6 месяцев

            for (int i = 0; i < count; i++)
            {
                var order = new Order
                {
                    ServiceCenterId = serviceCenterId,
                    CreatedAt = GetRandomDate(startDate, DateTime.Now),
                    Status = GetRandomStatus(),
                    DeviceType = GetRandomItem(_deviceTypes),
                    Brand = GetRandomItem(_brands),
                    Model = GetRandomItem(_models),
                    Issue = GetRandomItem(_issues),
                    Counterparty = GetRandomItem(_counterparties),
                    Amount = GetRandomAmount()
                };

                // Генерация номера заказа
                serviceCenter.OrdersCount++;
                order.OrderNumber = $"ORD-{serviceCenter.OrdersCount}";

                orders.Add(order);
            }

            _context.Orders.AddRange(orders);
            await _context.SaveChangesAsync();
        }

        private DateTime GetRandomDate(DateTime start, DateTime end)
        {
            int range = (end - start).Days;
            return start.AddDays(_random.Next(range))
                       .AddHours(_random.Next(0, 24))
                       .AddMinutes(_random.Next(0, 60));
        }

        private OrderStatus GetRandomStatus()
        {
            var statuses = Enum.GetValues<OrderStatus>();
            return statuses[_random.Next(statuses.Length)];
        }

        private string GetRandomItem(string[] array)
        {
            return array[_random.Next(array.Length)];
        }

        private decimal GetRandomAmount()
        {
            // Суммы от 500 до 20000 рублей
            return _random.Next(500, 20001);
        }

        public async Task ClearTestDataAsync()
        {
            var testOrders = await _context.Orders.ToListAsync();
            _context.Orders.RemoveRange(testOrders);
            await _context.SaveChangesAsync();
        }
    }
}