using YS.Knife.Function;
using YS.Knife.Function.Impl.EFCore;

namespace YS.Knife.Function.Core.UnitTest;

public class FunctionPermissionServiceTest
{
    [Fact]
    public void HasPermission_Merge_ShouldUnionPermissionsAcrossRoleCodes()
    {
        var values = new List<LayerValueInfo>
        {
            Value("app", "dept::a", AssignType.AllowInherrites),
            Value("read", "dept::a", AssignType.Deny),
            Value("app", "dept::b", AssignType.AllowInherrites),
            Value("read", "dept::b", AssignType.Allow)
        };

        FunctionPermissionService.HasPermission(
            "app", new[] { "read", "app" }, new[] { "dept" },
            LayerValuePolicy.Merge, values).Should().BeTrue();
    }

    [Fact]
    public void HasPermission_Merge_ShouldKeepRoleLocalInheritanceLogic()
    {
        var values = new List<LayerValueInfo>
        {
            Value("app", "dept::a", AssignType.Allow),
            Value("read", "dept::b", AssignType.Deny)
        };

        FunctionPermissionService.HasPermission(
            "app", new[] { "read", "app" }, new[] { "dept" },
            LayerValuePolicy.Merge, values).Should().BeFalse();
    }

    [Fact]
    public void HasPermission_WithNoFunctionChain_ShouldDeny()
    {
        FunctionPermissionService.HasPermission(
            "app", Array.Empty<string>(), new[] { "dept" },
            LayerValuePolicy.Cover, new List<LayerValueInfo>()).Should().BeFalse();
    }

    [Fact]
    public void GetRoleProviders_ShouldKeepLegacyAndNewFormats()
    {
        var legacy = new FunctionTreeInfo
        {
            Code = "app",
            Config = new Dictionary<string, object> { ["roleProviders"] = new[] { "dept", "-", "user" } }
        };
        var modern = new FunctionTreeInfo
        {
            Code = "app",
            Config = new Dictionary<string, object>
            {
                ["roles"] = new[]
                {
                    new RoleProviderInfo { Providers = new[] { "dept" }, Policy = LayerValuePolicy.Merge }
                }
            }
        };

        FunctionPermissionService.GetRoleProviders(legacy).Select(p => p.Providers)
            .Should().BeEquivalentTo(new[] { new[] { "dept" }, new[] { "user" } });
        FunctionPermissionService.GetRoleProviders(modern)[0].Policy.Should().Be(LayerValuePolicy.Merge);
    }

    [Fact]
    public void GetRoleProviders_WithNoConfig_ShouldReturnEmpty()
    {
        FunctionPermissionService.GetRoleProviders(new FunctionInfo()).Should().BeEmpty();
    }

    private static LayerValueInfo Value(string key, string roleCode, AssignType type)
    {
        return new LayerValueInfo
        {
            Key = key,
            RoleCode = roleCode,
            Value = $"{{\"type\":{(int)type}}}"
        };
    }
}
