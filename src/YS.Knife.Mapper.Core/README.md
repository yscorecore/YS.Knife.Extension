# YS.Knife.Mapper.Core

对象映射（Mapper）能力的抽象层，定义查询映射、转换映射与拷贝映射三类核心接口，以及标记映射程序集的特性。

## 主要功能

- `IQuerableMapper`：`MapQuery<From, To>(IQueryable<From>)`，将一个 `IQueryable` 投影映射为另一种类型的 `IQueryable`，适用于数据库查询场景（表达式级别的投影）。
- `IConvertMapper`：`Convert<From, To>(From)`，将一个对象实例转换为新的对象实例。
- `ICopyMapper`：`Copy<From, To>(From, To, onAddItem, onRemoveItem)`，将源对象的值拷贝到已存在的目标对象，并支持集合元素的增删回调。
- `MapperAssemblyAttribute`：程序集级特性（`AllowMultiple = false`），用于标记包含映射实现的程序集，供映射框架扫描识别。

## 使用示例

```csharp
// 在映射实现程序集中标记
[assembly: YS.Knife.Mapper.MapperAssembly]

// 依赖注入后使用（具体实现由 Mapper 实现包提供）
public class UserService
{
    private readonly IQuerableMapper querableMapper;
    private readonly IConvertMapper convertMapper;

    public IQueryable<UserDto> QueryDtos(IQueryable<User> users)
    {
        return querableMapper.MapQuery<User, UserDto>(users);
    }

    public UserDto ToDto(User user)
    {
        return convertMapper.Convert<User, UserDto>(user);
    }
}
```

## 依赖说明

- 本包无任何外部依赖，仅包含接口与特性定义。
- 具体映射能力由对应的实现包提供（如基于源生成或表达式编译的 Mapper 实现）。
