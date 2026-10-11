# YS.Knife.LogicRoles.Core

逻辑角色（Logic Roles）抽象层，用于从多个角色提供者中汇聚当前用户的角色编码。

## 主要功能

- `ILogicRoleProvider`：角色提供者接口，按名称（`Name`）返回当前用户的角色编码（`GetCurrentRoleCodes`）
- `LogicRoleProviderExtensions`：将一个或多个提供者的角色汇聚为 `提供者名::角色编码` 格式的完整角色标识
  - `GetAllRoles()`：汇聚所有提供者
  - `GetAllRoles(string[] activeProviderNames)`：仅汇聚指定提供者
  - `GetMainRole(string activeProviderName)`：获取指定提供者的主角色（最后一个角色）
- `ILogicRoleService`：获取全部角色编码的接口，配套 `FilterByProviders` 扩展按提供者过滤
- `ILogicRoleManagerService` / `ILogicRoleDetailService`：用于管理界面展示角色提供者信息（树状、多选、仅叶子等元数据）
- `ClaimTypes.LogicRole`：逻辑角色 Claim 类型常量（`ys.knife/logic-roles`）

## 使用示例

实现自定义角色提供者：

```csharp
public class DeptRoleProvider : ILogicRoleProvider
{
    public string Name => "dept";

    public Task<string[]> GetCurrentRoleCodes()
    {
        // 返回当前用户所属部门编码
        return Task.FromResult(new[] { "dev-team" });
    }
}
```

汇聚角色：

```csharp
public class MyService
{
    private readonly IEnumerable<ILogicRoleProvider> providers;

    public MyService(IEnumerable<ILogicRoleProvider> providers)
    {
        this.providers = providers;
    }

    public async Task Demo()
    {
        // 汇聚所有提供者，得到 ["dept::dev-team", ...]
        var allRoles = await providers.GetAllRoles();

        // 仅汇聚指定提供者
        var deptRoles = await providers.GetAllRoles("dept");

        // 获取主角色
        var mainRole = await providers.GetMainRole("dept");
    }
}
```

## 依赖说明

本包仅为抽象层，无任何第三方依赖，目标框架 net6.0。
