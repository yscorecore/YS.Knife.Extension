# YS.Knife.Extensions.Crypt

国密 SM4 对称加密算法实现，继承 `System.Security.Cryptography.SymmetricAlgorithm`，可直接用于 .NET 加密体系。

## 快速开始

### 1. 注册算法

在应用启动时注册一次：

```csharp
CryptoConfig.AddAlgorithm(typeof(Sm4SymmetricAlgorithm), "SM4", "Sm4");
```

### 2. 配合 EF Core 加密属性使用

```csharp
[Encrypted(typeof(SymmetricEncryptionProvider), "SM4", "my-passphrase")]
public string IdCard { get; set; }
```

### 3. 通过工厂方法创建

注册后可通过 `SymmetricAlgorithm.Create` 获取实例，与 AES、DES 等内置算法使用方式一致：

```csharp
// 注册后才能使用
CryptoConfig.AddAlgorithm(typeof(Sm4SymmetricAlgorithm), "SM4", "Sm4");

// 通过工厂方法创建
using var sm4 = SymmetricAlgorithm.Create("SM4");
sm4.Key = key;   // 16 字节
sm4.GenerateIV();

using var encryptor = sm4.CreateEncryptor();
var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
```

### 4. 直接使用

```csharp
using var sm4 = new Sm4SymmetricAlgorithm();
sm4.Key = key;   // 16 字节
sm4.GenerateIV();

using var encryptor = sm4.CreateEncryptor();
var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
```

## 算法参数

| 参数 | 值 |
|------|-----|
| 密钥长度 | 128 位（16 字节） |
| 分组长度 | 128 位（16 字节） |
| 轮数 | 32 |
