using YS.Knife.Function;

namespace YS.Knife.Function.Core.UnitTest;

public class FunctionTreeExtensionsTest
{
    [Fact]
    public void MergeTree_ShouldUnionNodesWithTheSameCode()
    {
        var first = Tree("app", Tree("read"));
        var second = Tree("app", Tree("write"));

        var result = new[] { first, second }.MergeTree("app");

        result.Should().NotBeNull();
        result!.Code.Should().Be("app");
        result.SubItems!.Select(p => p.Code).Should().BeEquivalentTo("read", "write");
    }

    [Fact]
    public void MergeTree_ShouldMergeNestedNodesAndKeepFirstMetadata()
    {
        var first = Tree("app", Tree("module", new FunctionTreeInfo
        {
            Code = "read",
            Name = "Read from first tree",
            Sequence = 1
        }));
        var second = Tree("app", Tree("module", new FunctionTreeInfo
        {
            Code = "write",
            Name = "Write from second tree",
            Sequence = 2
        }));

        var result = new[] { first, second }.MergeTree("app");

        result!.SubItems.Should().ContainSingle();
        result.SubItems[0].SubItems!.Select(p => p.Code)
            .Should().BeEquivalentTo("read", "write");
        result.SubItems[0].SubItems[0].Name.Should().Be("Read from first tree");
    }

    [Fact]
    public void MergeTree_WithNoTrees_ShouldReturnNull()
    {
        Array.Empty<FunctionTreeInfo>().MergeTree("app").Should().BeNull();
    }

    private static FunctionTreeInfo Tree(string code, params FunctionTreeInfo[] children)
    {
        var tree = new FunctionTreeInfo
        {
            Code = code,
            Name = code,
            SubItems = children.Length == 0 ? null : children.ToList()
        };
        foreach (var child in children)
        {
            child.ParentCode = code;
        }
        return tree;
    }
}
