using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace ConsoleTest
{
    #region Interface

    public interface IShippingStrategy
    {
        double Calculate(Order order);
    }

    #endregion


    #region Shipping Classes

    public class UPS : IShippingStrategy
    {
        private readonly double SHIPPING_COST_RATIO = 0.3d;

        public double Calculate(Order order)
        {
            return order.Cost * SHIPPING_COST_RATIO;
        }

        public override string ToString()
        {
            return "UPS";
        }
    }

    public class FedEx : IShippingStrategy
    {
        private readonly double RUSSIA_AND_USA_SHIPPING_COST_DISCOUNT = 7d;
        private readonly double OTHER_COUNTRIES_SHIPPING_COST_DISCOUNT = 5d;

        public double Calculate(Order order)
        {
            switch(order.Destination.Country)
            {
                case "Russia":
                case "USA":
                    return order.Cost / RUSSIA_AND_USA_SHIPPING_COST_DISCOUNT;

                default: return order.Cost / OTHER_COUNTRIES_SHIPPING_COST_DISCOUNT;
            }
        }

        public override string ToString()
        {
            return "FedEx";
        }
    }

    public class EMS : IShippingStrategy
    {
        public double Calculate(Order order)
        {
            return new Random().NextDouble() * order.Cost;
        }

        public override string ToString()
        {
            return "EMS";
        }
    }

    #endregion


    #region Shipping Factory

    public class ShippingFactory
    {
        Dictionary<string, Type> strategies;

        public ShippingFactory() => LoadTypes();

        public List<IShippingStrategy> GetStrategies()
        {
            var result = new List<IShippingStrategy>();

            if (strategies.Count == 0) throw new Exception("No delivery service found");

            foreach(KeyValuePair<string, Type> pair in strategies)
            {
                result.Add(Activator.CreateInstance(pair.Value) as IShippingStrategy);
            }

            return result;
        }

        public IShippingStrategy GetStrategy(string name)
        {
            var type = GetStrategyType(name);

            if (type == null) throw new NullReferenceException($"Delivery {name} not found or does not exist");

            return Activator.CreateInstance(type) as IShippingStrategy;
        }

        private Type GetStrategyType(string name)
        {
            if (strategies.Keys.Contains(name)) return strategies[name];
            return null;
        }

        private void LoadTypes()
        {
            strategies = new Dictionary<string, Type>();

            var types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types)
            {
                if (type.GetInterface(typeof(IShippingStrategy).ToString()) != null)
                {
                    strategies.Add(type.Name, type);
                }
            }
        }
    }

    #endregion


    #region Order and Address

    public class Order
    {
        public IShippingStrategy ShippingStrategy { get; set; }
        public Address Destination { get; set; }
        public Address Origin { get; set; }
        public double Cost { get; set; }

        public Order(double cost) => this.Cost = cost;
    }

    public class Address
    {
        public string ContactName { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    #endregion


    #region Client Code

    class Program
    {
        private static void DeliveryCostComparison(List<IShippingStrategy> strategies, Order order)
        {
            foreach(IShippingStrategy strategy in strategies)
            {
                Console.WriteLine($"Shipping cost from {strategy} is: {strategy.Calculate(order)}");
            }
        }

        static void Main(string[] args)
        {
            var order = new Order(1000)
            {
                Destination = new Address()
            };

            order.Destination.Country = "Russia";

            var factory = new ShippingFactory();

            Console.WriteLine($"Shipping cost from FedEx is: { factory.GetStrategy("FedEx").Calculate(order) }");

            DeliveryCostComparison(factory.GetStrategies(), order);

            Console.ReadKey();
        }
    }

    #endregion
}
