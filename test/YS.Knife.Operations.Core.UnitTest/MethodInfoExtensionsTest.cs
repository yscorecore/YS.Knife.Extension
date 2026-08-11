using System.ComponentModel;
using System.Reflection;
using YS.Knife.Operations;

namespace YS.Knife.Operations.Core.UnitTest
{
    public class MethodInfoExtensionsTest
    {
        [Fact]
        public void ShouldUseMethodNameAsIdWhenNoOperationAttribute()
        {
            var method = GetMethod(nameof(SampleMethods.MethodWithoutAttribute));

            var operation = method.GetOperation();

            operation.Id.Should().Be(nameof(SampleMethods.MethodWithoutAttribute));
            operation.Description.Should().BeNull();
        }

        [Fact]
        public void ShouldKeepRawValuesForOperationInNonGenericType()
        {
            var method = GetMethod(nameof(SampleMethods.MethodWithAttribute));

            var operation = method.GetOperation();

            // 非泛型类型：保持原样，向后兼容
            operation.Id.Should().Be("op-001");
            operation.Description.Should().Be("do something");
        }

        [Fact]
        public void ShouldReturnConsistentResultForTheSameMethod()
        {
            var method = GetMethod(nameof(SampleMethods.MethodWithAttribute));

            var first = method.GetOperation();
            var second = method.GetOperation();

            second.Should().Be(first);
        }


        [Fact]
        public void ShouldResolveDifferentOperationsForDifferentClosedGenericTypes()
        {
            var userOperation = typeof(GenericService<User>).GetMethod(nameof(GenericService<User>.Create))!.GetOperation();
            var orderOperation = typeof(GenericService<Order>).GetMethod(nameof(GenericService<Order>.Create))!.GetOperation();

            userOperation.Id.Should().Be("create");
            userOperation.Description.Should().Be("创建用户");

            // Order 未定义 DescriptionAttribute，描述回退为类型名
            orderOperation.Id.Should().Be("create");
            orderOperation.Description.Should().Be("创建Order");

            userOperation.Should().NotBe(orderOperation);
        }





        private static MethodInfo GetMethod(string name)
        {
            return typeof(SampleMethods).GetMethod(name, BindingFlags.Public | BindingFlags.Static)!;
        }

        [Description("用户")]
        private class User
        {
        }

        private class Order
        {
        }

        private static class SampleMethods
        {
            public static void MethodWithoutAttribute()
            {
            }
            [Operation("op-001", "do something")]
            public static void MethodWithAttribute()
            {
            }
        }

        private class GenericService<T>
        {
            [Operation("create", "创建{0}")]
            public void Create()
            {
            }


        }


    }
}
