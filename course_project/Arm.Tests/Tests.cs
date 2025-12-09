using course_project.Services;
using course_project.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arm.Tests
{
    // ТЕСТЫ ДЛЯ АВТОРИЗАЦИИ
    [TestClass]
    public class AuthServiceTests
    {
        [TestMethod]
        public void HashPassword_SameInput_ReturnsSameHash()
        {
            // Arrange (Подготовка)
            string password = "password123";

            // Act (Действие)
            string hash1 = AuthService.HashPassword(password);
            string hash2 = AuthService.HashPassword(password);

            // Assert (Проверка)
            Assert.AreEqual(hash1, hash2, "Хеши для одного и того же пароля должны совпадать");
        }

        [TestMethod]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "admin";
            string hash = AuthService.HashPassword(password);

            // Act
            bool result = AuthService.VerifyPassword(password, hash);

            // Assert
            Assert.IsTrue(result, "Проверка должна пройти успешно для правильного пароля");
        }

        [TestMethod]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            string password = "admin";
            string wrongPassword = "root";
            string hash = AuthService.HashPassword(password);

            // Act
            bool result = AuthService.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.IsFalse(result, "Проверка должна провалиться для неправильного пароля");
        }
    }

    // ТЕСТЫ ДЛЯ МОДЕЛЕЙ (РАСЧЕТЫ)
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void SaleItemDisplay_TotalItemAmount_CalculatesCorrectly()
        {
            // Arrange
            var item = new SaleItemDisplay
            {
                ProductName = "Принтер",
                Quantity = 2,
                PriceAtTimeOfSale = 5000m // Цена 5000
            };

            // Act
            decimal total = item.TotalItemAmount;

            // Assert
            // 2 штуки * 5000 цена = 10000
            Assert.AreEqual(10000m, total, "Сумма позиции должна считаться как Цена * Количество");
        }

        [TestMethod]
        public void Report_Initialization_ListsAreNotNull()
        {
            // Arrange & Act
            var report = new Report();

            // Assert
            // Проверяем, что списки создаются сразу, чтобы не было ошибки NullReferenceException
            Assert.IsNotNull(report.Sales, "Список продаж не должен быть null после создания отчета");
            Assert.IsNotNull(report.TopSellingProducts, "Список топ-товаров не должен быть null");
        }
    }

    // ТЕСТЫ ДЛЯ СЕРВИСА ПРОДАЖ (ЛОГИКА ФИЛЬТРАЦИИ)
    [TestClass]
    public class SaleServiceTests
    {
        [TestMethod]
        public void GetAllSaleForPeriod_FiltersCorrectly()
        {
            /* 
             * Трюк для теста: Мы не можем использовать реальный файл JSON, 
             * так как пути в тестах отличаются. 
             * Мы используем "Рефлексию", чтобы подсунуть данные в приватный список _sales
             * в обход загрузки файла.
             */

            // Arrange
            var service = new SaleService();
            
            // Создаем тестовые данные
            var salesData = new List<Sale>
            {
                new Sale { SaleId = 1, SaleDate = new DateTime(2023, 10, 01) }, // Октябрь
                new Sale { SaleId = 2, SaleDate = new DateTime(2023, 11, 15) }, // Ноябрь (попадет в выборку)
                new Sale { SaleId = 3, SaleDate = new DateTime(2023, 12, 01) }  // Декабрь
            };

            // Взламываем приватное поле _sales, чтобы записать туда данные без файла
            var fieldInfo = typeof(SaleService).GetField("_sales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue(service, salesData);

            // Act
            // Ищем продажи за Ноябрь
            var startDate = new DateTime(2023, 11, 01);
            var endDate = new DateTime(2023, 11, 30);
            var result = service.GetAllSaleForPeriod(startDate, endDate);

            // Assert
            Assert.AreEqual(1, result.Count, "Должна найтись ровно одна продажа за ноябрь");
            Assert.AreEqual(2, result[0].SaleId, "ID найденной продажи должен быть 2");
        }
    }
}