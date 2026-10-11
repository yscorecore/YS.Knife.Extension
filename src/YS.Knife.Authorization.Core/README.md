# YS.Knife.Authorization.Core

授权（Authorization）能力的抽象层，作为鉴权体系的契约定义包存在。

## 说明

本项目为授权体系的 Core（抽象）层。当前包内暂不包含具体源码，仅作为占位/契约程序集，供上层模块引用与后续扩展。按照仓库的命名约定：

- `*.Core`：抽象层，定义接口与模型，不含实现。
- `*.AspnetCore.Mvc`：ASP.NET Core MVC 集成层。
- `*.Impl.Xxx`：某种具体实现。

## 相关项目

- `YS.Knife.Authorization.AspnetCore.Mvc`：提供 `AuthAttribute` 授权过滤器，基于 `IFunctionPermissionService`（来自 `YS.Knife.Function.Core`）进行功能权限校验，是实际可用的鉴权集成层。

## 依赖说明

- 本包无外部依赖，目标框架由仓库统一配置。
