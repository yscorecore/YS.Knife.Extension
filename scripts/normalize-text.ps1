<#
.SYNOPSIS
  统一文本文件的 BOM、换行符和编码。

.DESCRIPTION
  按 .editorconfig 风格规则：
    - *.cs, *.csproj, *.props, *.targets, *.sln, *.config, *.html, *.razor
        → UTF-8 **with** BOM，LF 换行
    - *.json, *.md, *.txt, *.yml, *.yaml, *.css, *.js, *.ts, *.sql, *.xml, *.resx
        → UTF-8 **no** BOM，LF 换行
    - *.ps1, *.psm1, *.psd1, *.bat, *.cmd
        → 保留原编码（通常 ANSI/UTF-8），但换行统一 **CRLF**（Windows 脚本需要）

  额外处理：
    - 去除文件开头叠层的 U+FEFF（双重/三重 BOM 问题）
    - 去除文件中间残留的 \r（除了 .ps1/.bat）
    - 二进制文件（png/zip/dll/exe/pdb/nupkg）自动跳过，不修改
#>

[CmdletBinding()]
param(
    # 处理指定路径；不传则处理 git tracked 文本文件
    [string[]]$Paths = @(),

    # 只显示，不实际修改
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# —— 规则定义 ——
$bomExtensions     = @('.cs','.vb','.csproj','.props','.targets','.sln','.config',
                        '.html','.razor','.vbproj','.resx')
$nobomExtensions   = @('.json','.md','.txt','.yml','.yaml','.css','.js','.ts',
                        '.sql','.xml','.editorconfig','.gitattributes','.dockerfile')
$crlfExtensions     = @('.ps1','.psm1','.psd1','.bat','.cmd')
$binaryExtensions   = @('.png','.jpg','.jpeg','.gif','.ico','.pdf','.zip','.7z',
                        '.dll','.exe','.pdb','.nupkg','.woff','.woff2','.ttf','.eot','.svg')

function Should-UseBom([string]$ext) {
    return $bomExtensions -contains $ext
}

function Should-UseCrlf([string]$ext) {
    return $crlfExtensions -contains $ext
}

function Should-SkipBinary([string]$ext) {
    return $binaryExtensions -contains $ext
}

function Is-ProbablyBinary([byte[]]$bytes) {
    # 简单启发式：检查 NUL 字节（文本文件一般不会有）
    $checkLen = [Math]::Min($bytes.Length, 8192)
    for ($i = 0; $i -lt $checkLen; $i++) {
        if ($bytes[$i] -eq 0) { return $true }
    }
    return $false
}

function Normalize-File([string]$path) {
    $ext = [System.IO.Path]::GetExtension($path).ToLower()
    if ([string]::IsNullOrEmpty($ext)) { return }

    if (Should-SkipBinary $ext) { return }

    $bytes = [System.IO.File]::ReadAllBytes($path)
    if ($bytes.Length -eq 0) { return }

    if (Is-ProbablyBinary $bytes) { return }

    # 1. 检查开头是否有 BOM（可能叠了多层）
    $hasAnyBom = $bytes[0] -eq 0xEF -and $bytes.Length -ge 3 -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF

    # 2. 用 UTF-8 解码（带 BOM 也能正确解，BOM 会变成 U+FEFF 前缀字符）
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)

    # 3. 去除所有开头的 U+FEFF（叠层 BOM）
    $start = 0
    while ($start -lt $text.Length -and [int]$text[$start] -eq 0xFEFF) { $start++ }
    if ($start -gt 0) { $text = $text.Substring($start) }

    # 4. 统一换行：先把 \r\n → \n，再把孤立的 \r → \n
    $text = $text -replace "`r`n", "`n"
    $text = $text -replace "`r", "`n"

    # 5. 根据扩展名决定最终换行
    if (Should-UseCrlf $ext) {
        $text = $text -replace "`n", "`r`n"
    }

    # 6. 决定是否加 BOM
    $wantBom = Should-UseBom $ext

    # —— 只在内容真的变了时才写回 ——
    $enc = [System.Text.UTF8Encoding]::new($wantBom)
    $newBytes = $enc.GetBytes($text)

    if ($newBytes.Length -eq $bytes.Length) {
        $same = $true
        for ($i = 0; $i -lt $bytes.Length; $i++) {
            if ($newBytes[$i] -ne $bytes[$i]) { $same = $false; break }
        }
        if ($same) { return }  # 没变，跳过
    }

    if ($DryRun) {
        Write-Output "  WOULD FIX: $path"
        return
    }

    [System.IO.File]::WriteAllBytes($path, $newBytes)
    Write-Output "  FIXED:     $path"
}

# —— 主流程 ——
$repoRoot = (git rev-parse --show-toplevel 2>$null)
if (-not $repoRoot) { $repoRoot = (Get-Location).Path }

if ($Paths.Count -eq 0) {
    # 处理所有 git tracked 文件（排除 bin/obj/.git）
    $allFiles = git -C $repoRoot ls-files 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $allFiles) {
        # fallback：遍历目录
        $files = Get-ChildItem -Path $repoRoot -Recurse -File | Where-Object {
            $_.FullName -notmatch '\\bin\\|\\obj\\|\\\.git\\|\\node_modules\\'
        }
    } else {
        $files = $allFiles | ForEach-Object {
            Join-Path $repoRoot $_
        } | Where-Object { Test-Path $_ }
    }
} else {
    $files = $Paths
}

Write-Output "Normalizing text files in: $repoRoot"
Write-Output ""

$count = 0
foreach ($f in $files) {
    Normalize-File $f
    $count++
}

Write-Output ""
Write-Output "Processed $count files."
