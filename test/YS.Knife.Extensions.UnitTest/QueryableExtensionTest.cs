
namespace YS.Knife.Extensions.UnitTest
{

    [Mapper(typeof(User), typeof(UserDto))]
    public class QueryableExtensionTest
    {
        [Fact]
        public void ShouldAppendOrderById()
        {
            var ids = GetUsers().TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasOrderByOtherProperty()
        {
            var ids = GetUsers().OrderBy(p => p.Age).TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 3, 1, 2, 5 }).Should().BeTrue();
        }

        [Fact]
        public void ShouldAppendOrderByIdWhenHasSkipMethod()
        {
            var ids = GetUsers().Skip(0).TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasTakeMethod()
        {
            var ids = GetUsers().Take(int.MaxValue).TryOrderByEntityKey().Select(p => p.UserId);
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasWhereMethod()
        {
            var ids = GetUsers().Where(p => p.Name != null).TryOrderByEntityKey().Select(p => p.UserId);
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasSelectManyMethod()
        {
            var ids = GetUsers().SelectMany(p => Enumerable.Repeat(p, 3)).TryOrderByEntityKey().Select(p => p.UserId);
            ids.SequenceEqual(new[] { 1, 1, 1, 2, 2, 2, 3, 3, 3, 5, 5, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasDistenctMethod()
        {
            var ids = GetUsers().Distinct().TryOrderByEntityKey().Select(p => p.UserId);
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasDistenctByMethod()
        {
            var ids = GetUsers().DistinctBy(p => p.UserId).TryOrderByEntityKey().Select(p => p.UserId);
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasGroupByMethod()
        {
            var ids = GetUsers().GroupBy(p => p.UserId).Select(p => p.First()).TryOrderByEntityKey().Select(p => p.UserId);
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }

        [Fact]
        public void ShouldAppendOrderByIdWhenHasSelect()
        {
            var ids = GetUsers().To<UserDto>().TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 1, 2, 3, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldAppendOrderByIdWhenHasSelectAndOrderByOtherProperty()
        {
            var ids = GetUsers().OrderBy(p => p.Age).To<UserDto>().TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 3, 1, 2, 5 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldNotAppendOrderByIdWhenQueryHasOrderById()
        {
            var ids = GetUsers().OrderByDescending(p => p.UserId).TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 5, 3, 2, 1 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldNotAppendOrderByIdWhenHasOrderByIdAndOrderByOtherProperty()
        {
            var ids = GetUsers().OrderBy(p => p.Age).ThenByDescending(p => p.UserId).TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 3, 5, 2, 1 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldNotAppendOrderByIdWhenHasSelectAndOtherById()
        {
            var ids = GetUsers().OrderByDescending(p => p.UserId).To<UserDto>().TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 5, 3, 2, 1 }).Should().BeTrue();
        }
        [Fact]
        public void ShouldNotAppendOrderByIdWhenHasSelectAndOrderByOtherProperty()
        {
            var ids = GetUsers().OrderBy(p => p.Age).ThenByDescending(p => p.UserId).To<UserDto>().TryOrderByEntityKey().Select(p => p.UserId).ToArray();
            ids.SequenceEqual(new[] { 3, 5, 2, 1 }).Should().BeTrue();
        }

        private IQueryable<User> GetUsers()
        {
            return new User[]
                {
                    new User{ UserId=5, Age=10, Name="wangwu" },
                    new User{ UserId=1, Age=10, Name="zhangsan" },
                    new User{ UserId=3, Age=9, Name="lisi" },
                    new User{ UserId=2, Age=10, Name="lisi" },
                }.AsQueryable();

        }
        internal record User
        {
            public int UserId { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
        internal record UserDto
        {
            public int UserId { get; set; }
            public string Name { get; set; }
        }
    }

}
