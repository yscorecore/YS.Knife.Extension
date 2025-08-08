using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Extensions.UnitTest
{
    public class QueryExtensionTest
    {
        [Fact]
        public void ShouldFilteItemsOr()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var source = new[] { 2, 3 };
            var res = array.AsQueryable().WhereItemsOr(source, (p, t) => p % t == 0).ToList();
            res.Should().BeEquivalentTo(new[] { 2, 3, 4, 6, 8 }, o => o.WithoutStrictOrdering());
        }
        [Fact]
        public void ShouldFilterItemsAnd()
        {
            var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var source = new[] { 2, 3 };
            var res = array.AsQueryable().WhereItemsAnd(source, (p, t) => p % t == 0).ToList();
            res.Should().BeEquivalentTo(new[] { 6 }, o => o.WithoutStrictOrdering());
        }
    }
}
