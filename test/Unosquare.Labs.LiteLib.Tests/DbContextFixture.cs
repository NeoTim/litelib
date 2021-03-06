﻿namespace Unosquare.Labs.LiteLib.Tests
{
    using Database;
    using Helpers;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if !NET461
    using Microsoft.Data.Sqlite;
#endif

    /// <summary>
    /// A TestFixture to test the included methods in LiteDbSet
    /// </summary>
    [TestFixture]
    public partial class DbContextFixture
    {
        public class TypeDefinitionTest : DbContextAsyncFixture
        {
            [Test]
            public void TypeDefinition()
            {
                Assert.Throws<TargetInvocationException>(() =>
                {
                    var context = new TestDbContextWithOutProperties(nameof(TypeDefinition));
                });
            }

            [Test]
            public void TypeDefinition_CustomAttribute()
            {
                using (var context = new TestDbContext(nameof(TypeDefinition_CustomAttribute)))
                {
                    var name = context.Warehouses.TableName;

                    Assert.AreEqual("CustomWarehouse", name);
                }
            }

            [Test]
            public void TypeDefinition_NotMappedAttribute()
            {
                using (var context = new TestDbContext(nameof(TypeDefinition_NotMappedAttribute)))
                {
                    var properties = context.Warehouses.PropertyNames;

                    Assert.AreEqual(3, properties.Count);
                }
            }
        }

        public class InsertRangeTest : DbContextFixture
        {
            [Test]
            public void InsertingDataList()
            {
                using (var context = new TestDbContext(nameof(InsertingDataList)))
                {
                    context.Orders.InsertRange(TestHelper.DataSource);
                    var list = context.Orders.Count();
                    Assert.AreEqual(TestHelper.DataSource.Length, list);
                }
            }

            [Test]
            public void InsertingEmptyDataList_ThrowsArgumentException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    using (var context = new TestDbContext(nameof(InsertingEmptyDataList_ThrowsArgumentException)))
                    {
                        context.Orders.InsertRange(new List<Order>());
                    }
                });
            }
        }

        public class UpdateTest : DbContextFixture
        {
            [Test]
            public void UpdatingEntities()
            {
                using (var context = new TestDbContext(nameof(UpdatingEntities)))
                {
                    foreach (var item in TestHelper.DataSource)
                    {
                        context.Orders.Insert(item);
                    }

                    var list = context.Orders.Select("CustomerName = @CustomerName", new {CustomerName = "John"});
                    foreach (var item in list)
                    {
                        item.ShipperCity = "Atlanta";
                        context.Orders.Update(item);
                    }

                    var updatedList =
                        context.Orders.Select("ShipperCity = @ShipperCity", new {ShipperCity = "Atlanta"});
                    foreach (var item in updatedList)
                    {
                        Assert.AreEqual("Atlanta", item.ShipperCity);
                    }
                }
            }
        }

        public class SingleTest : DbContextFixture
        {
            [Test]
            public void SelectingSingleDataWithCorrectId()
            {
                using (var context = new TestDbContext(nameof(SelectingSingleDataWithCorrectId)))
                {
                    var k = 0;
                    foreach (var item in TestHelper.DataSource)
                    {
                        item.UniqueId = (k++).ToString();
                        context.Orders.Insert(item);
                    }

                    var singleSelect = context.Orders.Single(3);
                    Assert.AreEqual("Margarita", singleSelect.CustomerName);
                }
            }

            [Test]
            public void SelectingSingleDataWithIncorrectId_ThrowsException()
            {
                using (var context = new TestDbContext(nameof(SelectingSingleDataWithIncorrectId_ThrowsException)))
                {
                    var k = 0;

                    foreach (var item in TestHelper.DataSource)
                    {
                        item.UniqueId = (k++).ToString();
                        context.Orders.Insert(item);
                    }

                    Assert.Throws<InvalidOperationException>(() => context.Orders.Single(50));
                }
            }
        }

        public class SetTest : DbContextFixture
        {
            [Test]
            public void SetEntity()
            {
                using (var context = new TestDbContext(nameof(SetEntity)))
                {
                    var names = context.GetSetNames();
                    Assert.IsNotNull(names);
                    Assert.AreEqual(names, new[] {nameof(context.Orders), nameof(context.Warehouses)});

                    var orders = context.Set<Order>();
                    var ordersByName = context.Set(typeof(Order));

                    Assert.AreEqual(context.Orders, orders);
                    Assert.AreEqual(context.Orders, ordersByName);
                }
            }

            [Test]
            public void SelectFromSetName()
            {
                using (var context = new TestDbContext(nameof(SelectFromSetName)))
                {
                    foreach (var item in TestHelper.DataSource)
                    {
                        context.Orders.Insert(item);
                    }

                    var orders = context.Set<Order>();

                    var data = context.Select<Order>(orders, "1=1");
                    Assert.IsNotNull(data);
                    Assert.AreEqual(context.Orders.SelectAll().First().RowId, data.First().RowId, "Same first object");
                }
            }

            [Test]
            public void InvalidSetName_ThrowsArgumentOutOfRangeException()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    using (var context = new TestDbContext(nameof(InvalidSetName_ThrowsArgumentOutOfRangeException)))
                    {
                        context.Set<DbContextFixture>();
                    }
                });
            }

            [Test]
            public void InsertFromContext()
            {
                using (var context = new TestDbContext(nameof(InsertFromContext)))
                {
                    foreach (var item in TestHelper.DataSource)
                    {
                        context.Insert(item);
                    }

                    Assert.AreEqual(TestHelper.DataSource.Length, context.Orders.Count(), "Has data");
                }
            }

            [Test]
            public void DeleteFromContext()
            {
                using (var context = new TestDbContext(nameof(DeleteFromContext)))
                {
                    foreach (var item in TestHelper.DataSource)
                    {
                        context.Insert(item);
                    }

                    Assert.AreEqual(TestHelper.DataSource.Length, context.Orders.Count(), "Has data");

                    foreach (var item in TestHelper.DataSource)
                    {
                        context.Delete(item);
                    }

                    Assert.AreEqual(0, context.Orders.Count(), "Has data");
                }
            }

            [Test]
            public void UpdateFromContext()
            {
                using (var context = new TestDbContext(nameof(UpdateFromContext)))
                {
                    foreach (var item in TestHelper.DataSource)
                    {
                        context.Insert(item);
                    }

                    foreach (var item in TestHelper.DataSource)
                    {
                        item.ShipperCity = "Atlanta";
                        context.Update(item);
                    }

                    var updatedItems =
                        context.Orders.Select("ShipperCity = @ShipperCity", new {ShipperCity = "Atlanta"});
                    Assert.AreEqual(TestHelper.DataSource.Length, updatedItems.Count());
                }
            }
        }
    }
}