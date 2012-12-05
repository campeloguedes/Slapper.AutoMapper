﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Slapper.Tests
{
    [TestFixture]
    public class MapTests : TestBase
    {
        public class Customer
        {
            public int CustomerId;
            public string FirstName;
            public string LastName;
            public IList<Order> Orders;
        }

        public class Order
        {
            public int OrderId;
            public decimal OrderTotal;
            public IList<OrderDetail> OrderDetails;
        }

        public class OrderDetail
        {
            public int OrderDetailId;
            public decimal OrderDetailTotal;
        }

        public class MapTestModels
        {
            public class CustomerWithMultipleIdAttributes
            {
                [Slapper.AutoMapper.Id]
                public int Customer_Id;

                [Slapper.AutoMapper.Id]
                public string Customer_Type;

                public string FirstName;

                public string LastName;

                public List<Order> Orders;
            }

            public class PersonWithFields
            {
                public int Id;
                public string FirstName;
                public string LastName;
            }

            public class PersonWithProperties
            {
                public int Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
            }

            public class CustomerWithOrdersList
            {
                public int Id;
                public string FirstName;
                public string LastName;
                public List<Order> Orders;
            }

            public class CustomerWithOrdersICollection
            {
                public int Id;
                public string FirstName;
                public string LastName;
                public ICollection<Order> Orders;
            }

            public class Order
            {
                public int Id;
                public decimal OrderTotal;
                public IList<OrderDetail> OrderDetails;
            }

            public class OrderDetail
            {
                public int Id;
                public decimal OrderDetailTotal;
                public Product Product;
            }

            public class Product
            {
                public int Id;
                public string ProductName;
            }
        }

        [Test]
        public void Can_Map_Matching_Field_Names()
        {
            // Arrange
            const int id = 1;
            const string firstName = "Bob";
            const string lastName = "Smith";

            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName }
                                 };

            // Act
            var customer = Slapper.AutoMapper.Map<MapTestModels.PersonWithFields>( dictionary );
            
            // Assert
            Assert.NotNull( customer );
            Assert.That( customer.Id == id );
            Assert.That( customer.FirstName == firstName );
            Assert.That( customer.LastName == lastName );
        }

        [Test]
        public void Can_Map_Matching_Property_Names()
        {
            // Arrange
            const int id = 1;
            const string firstName = "Bob";
            const string lastName = "Smith";

            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName }
                                 };

            // Act
            var customer = Slapper.AutoMapper.Map<MapTestModels.PersonWithProperties>( dictionary );
            
            // Assert
            Assert.NotNull( customer );
            Assert.That( customer.Id == id );
            Assert.That( customer.FirstName == firstName );
            Assert.That( customer.LastName == lastName );
        }

        [Test]
        public void Can_Map_Complex_Nested_Members()
        {
            // Arrange
            const int id = 1;
            const string firstName = "Bob";
            const string lastName = "Smith";
            const int orderId = 1;
            const decimal orderTotal = 50.50m;

            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId },
                                     { "Orders_OrderTotal", orderTotal }
                                 };

            // Act
            var customer = Slapper.AutoMapper.Map<MapTestModels.CustomerWithOrdersList>( dictionary );
            
            // Assert
            Assert.NotNull( customer );
            Assert.That( customer.Id == id );
            Assert.That( customer.FirstName == firstName );
            Assert.That( customer.LastName == lastName );
            Assert.NotNull( customer.Orders );
            Assert.That( customer.Orders.Count == 1 );
            Assert.That( customer.Orders.First().Id == orderId );
            Assert.That( customer.Orders.First().OrderTotal == orderTotal );
        }

        /// <summary>
        /// When mapping, it internally keeps a cache of instantiated objects with the key being the
        /// hash of the objects identifier hashes summed together so when another record with the exact
        /// same identifier hash is detected, it will re-use the existing instantiated object instead of
        /// creating a second one alleviating the burden of the consumer of the library to group objects
        /// by their identifier.
        /// </summary>
        [Test]
        public void Can_Detect_Duplicate_Parent_Members_And_Properly_Instantiate_The_Object_Only_Once()
        {
            // Arrange
            const int id = 1;
            const string firstName = "Bob";
            const string lastName = "Smith";
            const int orderId = 1;
            const decimal orderTotal = 50.50m;

            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId },
                                     { "Orders_OrderTotal", orderTotal }
                                 };

            var dictionary2 = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId + 1 },
                                     { "Orders_OrderTotal", orderTotal + 1 }
                                 };

            var listOfDictionaries = new List<Dictionary<string, object>> { dictionary, dictionary2 };

            // Act
            var customers = Slapper.AutoMapper.Map<MapTestModels.CustomerWithOrdersList>( listOfDictionaries );

            var customer = customers.FirstOrDefault();

            // Assert
            Assert.That( customers.Count() == 1 );
            Assert.NotNull( customer );
            Assert.That( customer.Id == id );
            Assert.That( customer.FirstName == firstName );
            Assert.That( customer.LastName == lastName );
            Assert.NotNull( customer.Orders );
            Assert.That( customer.Orders.Count == 2 );
            Assert.That( customer.Orders[ 0 ].Id == orderId );
            Assert.That( customer.Orders[ 0 ].OrderTotal == orderTotal );
            Assert.That( customer.Orders[ 1 ].Id == orderId + 1 );
            Assert.That( customer.Orders[ 1 ].OrderTotal == orderTotal + 1 );
        }

        [Test]
        public void Can_Handle_Nested_Members_That_Implements_ICollection()
        {
            // Arrange
            const int id = 1;
            const string firstName = "Bob";
            const string lastName = "Smith";
            const int orderId = 1;
            const decimal orderTotal = 50.50m;

            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId },
                                     { "Orders_OrderTotal", orderTotal }
                                 };

            var dictionary2 = new Dictionary<string, object>
                                 {
                                     { "Id", id },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId + 1 },
                                     { "Orders_OrderTotal", orderTotal + 1 }
                                 };

            var listOfDictionaries = new List<Dictionary<string, object>> { dictionary, dictionary2 };

            // Act
            var customers = Slapper.AutoMapper.Map<MapTestModels.CustomerWithOrdersICollection>( listOfDictionaries );

            var customer = customers.FirstOrDefault();

            // Assert
            Assert.That( customer.Orders.Count == 2 );
        }

        [Test]
        public void Can_Handle_Mapping_Objects_With_Multiple_Identifiers()
        {
            // Arrange
            const int customerId = 1;
            const string customerType = "Commercial";
            const string firstName = "Bob";
            const string lastName = "Smith";
            const int orderId = 1;
            const decimal orderTotal = 50.50m;

            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Customer_Id", customerId },
                                     { "Customer_Type", customerType },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId },
                                     { "Orders_OrderTotal", orderTotal }
                                 };

            var dictionary2 = new Dictionary<string, object>
                                 {
                                     { "Customer_Id", customerId },
                                     { "Customer_Type", customerType },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId + 1 },
                                     { "Orders_OrderTotal", orderTotal + 1 }
                                 };

            var dictionary3 = new Dictionary<string, object>
                                 {
                                     { "Customer_Id", customerId + 1 },
                                     { "Customer_Type", customerType },
                                     { "FirstName", firstName },
                                     { "LastName", lastName },
                                     { "Orders_Id", orderId + 1 },
                                     { "Orders_OrderTotal", orderTotal + 1 }
                                 };

            var listOfDictionaries = new List<Dictionary<string, object>> { dictionary, dictionary2, dictionary3 };

            // Act
            var customers = Slapper.AutoMapper.Map<MapTestModels.CustomerWithMultipleIdAttributes>( listOfDictionaries );

            // Assert
            Assert.That( customers.Count() == 2 );
            Assert.That( customers.First().Orders.Count == 2 );
            Assert.That( customers.ToList()[ 1 ].Orders.First().Id == orderId + 1 );
        }

        [Test]
        public void Can_Map_To_Multiple_Objects()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "FirstName", "Bob" },
                                     { "LastName", "Smith" },
                                     { "Orders_Id", 1 },
                                     { "Orders_OrderTotal", 50.50m }
                                 };

            var dictionary2 = new Dictionary<string, object>
                                 {
                                     { "Id", 2 },
                                     { "FirstName", "Jane" },
                                     { "LastName", "Doe" },
                                     { "Orders_Id", 2 },
                                     { "Orders_OrderTotal", 23.40m }
                                 };

            var listOfDictionaries = new List<Dictionary<string, object>> { dictionary, dictionary2 };

            // Act
            var customers = Slapper.AutoMapper.Map<MapTestModels.CustomerWithOrdersICollection>( listOfDictionaries );

            // Assert
            Assert.That( customers.Count() == 2 );
            Assert.That( customers.ToList()[ 0 ].FirstName == "Bob" );
            Assert.That( customers.ToList()[ 1 ].FirstName == "Jane" );
        }

        [Test]
        public void Can_Handle_Mapping_Deeply_Nested_Members()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "FirstName", "Bob" },
                                     { "LastName", "Smith" },
                                     { "Orders_Id", 1 },
                                     { "Orders_OrderTotal", 50.50m },
                                     { "Orders_OrderDetails_Id", 1 },
                                     { "Orders_OrderDetails_OrderDetailTotal", 50.50m },
                                     { "Orders_OrderDetails_Product_Id", 546 },
                                     { "Orders_OrderDetails_Product_ProductName", "Black Bookshelf" }
                                 };

            // Act
            var customer = Slapper.AutoMapper.Map<MapTestModels.CustomerWithOrdersICollection>( dictionary );
            
            // Assert
            Assert.That( customer.Orders.Count == 1 );
            Assert.That( customer.Orders.First().OrderDetails.Count == 1 );
            Assert.That( customer.Orders.First().OrderDetails.First().Product.ProductName == "Black Bookshelf" );
        }

        [Test]
        public void Can_Handle_Resolving_Duplicate_Nested_Members()
        {
            // Arrange
            var dictionary = new Dictionary<string, object>
                                 {
                                     { "CustomerId", 1 },
                                     { "FirstName", "Bob" },
                                     { "LastName", "Smith" },
                                     { "Orders_OrderId", 1 },
                                     { "Orders_OrderTotal", 50.50m },

                                     { "Orders_OrderDetails_OrderDetailId", 1 },
                                     { "Orders_OrderDetails_OrderDetailTotal", 25.00m }
                                 };

            var dictionary2 = new Dictionary<string, object>
                                 {
                                     { "CustomerId", 1 },
                                     { "FirstName", "Bob" },
                                     { "LastName", "Smith" },
                                     { "Orders_OrderId", 1 },
                                     { "Orders_OrderTotal", 50.50m },

                                     { "Orders_OrderDetails_OrderDetailId", 2 },
                                     { "Orders_OrderDetails_OrderDetailTotal", 25.50m }
                                 };

            var list = new List<IDictionary<string, object>> { dictionary, dictionary2 };

            // Act
            var customers = Slapper.AutoMapper.Map<Customer>( list );

            // Assert
            Assert.That( customers.Count() == 1 );

            // We should only have one Order object
            Assert.That( customers.FirstOrDefault().Orders.Count == 1 );

            // We should only have one Order object and two OrderDetail objects
            Assert.That( customers.FirstOrDefault().Orders.FirstOrDefault().OrderDetails.Count == 2 );
        }

        [Test]
        public void Can_Handle_Mapping_A_Single_Dynamic_Object()
        {
            // Arrange
            dynamic dynamicCustomer = new ExpandoObject();
            dynamicCustomer.Id = 1;
            dynamicCustomer.FirstName = "Bob";
            dynamicCustomer.LastName = "Smith";
            dynamicCustomer.Orders_Id = 1;
            dynamicCustomer.Orders_OrderTotal = 50.50m;

            // Act
            var customer = Slapper.AutoMapper.MapDynamic<MapTestModels.CustomerWithOrdersICollection>( dynamicCustomer ) as MapTestModels.CustomerWithOrdersICollection;

            // Assert
            Assert.NotNull( customer );
            Assert.That( customer.Orders.Count == 1 );
        }

        [Test]
        public void Can_Handle_Mapping_Nested_Members_Using_Dynamic()
        {
            // Arrange
            var dynamicCustomers = new List<object>();

            for ( int i = 0; i < 5; i++ )
            {
                dynamic customer = new ExpandoObject();
                customer.Id = i;
                customer.FirstName = "FirstName" + i;
                customer.LastName = "LastName" + i;
                customer.Orders_Id = i;
                customer.Orders_OrderTotal = i + 0m;

                dynamicCustomers.Add( customer );
            }

            // Act
            var customers = Slapper.AutoMapper.MapDynamic<MapTestModels.CustomerWithOrdersICollection>( dynamicCustomers );

            // Assert
            Assert.That( customers.Count() == 5 );
            Assert.That( customers.First().Orders.Count == 1 );
        }

        [Test]
        public void Will_Throw_An_Exception_If_The_Type_Is_Not_Dynamic()
        {
            // Arrange
            var someObject = new object();

            // Act
            TestDelegate test = () => Slapper.AutoMapper.MapDynamic<MapTestModels.CustomerWithOrdersICollection>( someObject );

            // Assert
            Assert.Throws<ArgumentException>( test );
        }

        [Test]
        public void Will_Throw_An_Exception_If_The_List_Items_Are_Not_Dynamic()
        {
            // Arrange
            var someObjectList = new List<object> { null };

            // Act
            TestDelegate test = () => Slapper.AutoMapper.MapDynamic<MapTestModels.CustomerWithOrdersICollection>( someObjectList );

            // Assert
            Assert.Throws<ArgumentException>( test );
        }
    }
}
