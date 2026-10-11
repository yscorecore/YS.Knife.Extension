# YS.Knife.Extensions.HttpClient

`HttpClient` 的 REST 调用与文件下载扩展方法库，简化 JSON API 请求与带缓存的资源下载。

## 主要功能

### REST 请求（HttpClientExtensions）

- 通用发送：`SendAsString` / `SendAsObject<T>` / `SendAsResponse`，支持任意 `HttpMethod`
- 便捷动词方法：`GetAsString` / `GetAsObject<T>`、`PostAs*`、`PutAs*`、`PatchAs*`、`DeleteAs*`
- header / query 参数支持匿名对象或字典，body 自动序列化为 JSON（camelCase、属性名不区分大小写）
- 非成功状态码抛出包含请求、状态码、响应头与截断响应体的详细 `HttpRequestException`
- `businessExceptionFactory` 支持将业务错误转换为自定义异常
- 表单提交：`PostFormAsString` / `PostFormAsObject<T>`（multipart/form-data，支持 `Stream`、`byte[]`、`StreamBody` 文件上传与集合字段）

### 下载缓存（HttpClientDownloadExtensions）

- `DownloadWithCache`：按域名分目录缓存下载文件，返回本地文件路径
- 支持 ETag 与 `If-Modified-Since` 条件请求，未修改（304）时直接使用缓存
- `refreshCache = false` 时命中缓存不发请求
- `DownloadStreamWithCache`：返回缓存文件的只读流

## 使用示例

```csharp
HttpClient client = httpClientFactory.CreateClient();

// GET JSON API
var user = await client.GetAsObject<UserDto>(
    "https://api.example.com", "users/1",
    header: new { Authorization = "Bearer xxx" },
    param: new { includeDetail = true });

// POST JSON
var created = await client.PostAsObject<CreateResult>(
    "https://api.example.com", "users",
    body: new { name = "test", age = 20 });

// multipart 表单上传文件
var result = await client.PostFormAsObject<UploadResult>(
    "https://api.example.com", "files",
    form: new { file = StreamBody.FromLocalFile("a.png"), remark = "头像" });

// 带缓存下载
string localFile = await client.DownloadWithCache(
    "https://example.com/res/data.json", "./cache");
```

所有扩展方法定义在 `System.Net.Http` 命名空间下。

## 依赖说明

- 项目引用 `YS.Knife.Extensions`（提供 `JoinUrl`、`ToJsonText`、`AsJsonObject`、`StreamBody` 等）
- 无第三方包依赖

目标框架：.NET 6.0。
