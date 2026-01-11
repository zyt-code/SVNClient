# Svns - SVN Client Application Development Plan

## 项目概述

一个基于 .NET 9 和 Avalonia UI 的现代化 SVN 客户端应用，类似 IntelliJ IDEA 的工作方式，一次打开一个 SVN 工作副本。

## 核心设计理念

- **单工作副本模式**：一次打开一个 SVN 工作副本（类似 IDEA）
- **完整功能覆盖**：支持 SVN 命令行所有功能
- **现代化 UI**：使用 Avalonia 构建跨平台桌面应用
- **高性能**：异步操作，响应式界面
- **用户友好**：直观的可视化操作

## 技术栈

- **.NET 9** - 最新的 .NET 框架
- **Avalonia UI 11.3** - 跨平台 UI 框架
- **CommunityToolkit.Mvvm** - MVVM 框架
- **AvaloniaEdit** - 代码/差异编辑器
- **System.CommandLine** - 命令行参数解析

## 技术架构

### 1. 核心层 (Core Layer)

#### SvnCommandService
- 执行 SVN 命令行工具（svn.exe）
- 异步命令执行
- 进度跟踪和取消支持
- 错误处理和重试机制

#### SvnOutputParser
- 解析 SVN XML 输出
- 解析 SVN 文本输出
- 转换为强类型模型

### 2. 数据层 (Data Layer)

**Models 目录:**

- `SvnStatus` - 文件状态模型
- `SvnInfo` - 工作副本信息
- `SvnLogEntry` - 提交历史记录
- `SvnDiffResult` - 差异结果
- `SvnConflict` - 冲突文件
- `SvnProperty` - 文件属性
- `SvnRepositoryInfo` - 仓库信息
- `SvnBlameLine` - 追溯作者行

### 3. 服务层 (Service Layer)

**Services/Svn 目录:**

- `WorkingCopyService` - 工作副本管理
- `FileOperationService` - 文件操作（增删改）
- `HistoryService` - 历史记录查询
- `DiffService` - 差异对比
- `ConflictService` - 冲突解决
- `BranchTagService` - 分支标签管理
- `RemoteService` - 远程仓库操作
- `PropertyService` - 属性管理

**Services/UI 目录:**

- `DialogService` - 对话框服务
- `NotificationService` - 通知服务
- `ClipboardService` - 剪贴板服务

### 4. 业务逻辑层 (Business Logic Layer)

**ViewModels:**

- `MainWindowViewModel` - 主窗口
- `WorkingCopyViewModel` - 工作副本视图
- `ChangesViewModel` - 变更列表
- `CommitViewModel` - 提交对话框
- `UpdateViewModel` - 更新对话框
- `HistoryViewModel` - 历史记录
- `DiffViewModel` - 差异视图
- `ConflictViewModel` - 冲突解决
- `SettingsViewModel` - 设置
- `RemoteBrowserViewModel` - 远程仓库浏览器

### 5. UI 层 (View Layer)

**Views:**

- `MainWindow.axaml` - 主窗口
- `Controls/FileTreeView.axaml` - 文件树控件
- `Controls/ChangesPanel.axaml` - 变更面板
- `Controls/StatusPanel.axaml` - 状态面板
- `Controls/DiffViewer.axaml` - 差异查看器
- `Controls/HistoryView.axaml` - 历史记录视图

**Dialogs:**

- `CommitDialog.axaml` - 提交对话框
- `UpdateDialog.axaml` - 更新对话框
- `BranchTagDialog.axaml` - 分支标签对话框
- `MergeDialog.axaml` - 合并对话框
- `SettingsDialog.axaml` - 设置对话框
- `ConflictResolveDialog.axaml` - 冲突解决对话框

## 功能模块详细说明

### 阶段 1: 基础架构 (Foundation)

#### 1.1 项目结构搭建
- [x] 创建基础 Avalonia 项目
- [x] 创建目录结构
- [x] 添加必要的 NuGet 包
- [x] 配置项目设置

#### 1.2 SVN 命令执行引擎
```csharp
public class SvnCommandService
{
    public async Task<SvnResult> ExecuteAsync(
        string workingCopy,
        SvnCommand command,
        CancellationToken cancellationToken = default);

    public async Task<string> ExecuteRawAsync(
        string workingCopy,
        string[] arguments,
        CancellationToken cancellationToken = default);
}
```

#### 1.3 输出解析器
- [x] XML 输出解析器
- [x] 状态解析器
- [x] 日志解析器
- [x] 差异解析器
- [x] 信息解析器

#### 1.4 工作副本检测
- [x] 检测 .svn 目录
- [x] 验证工作副本有效性
- [x] 获取工作副本根目录
- [x] 工作副本信息缓存

#### 1.5 基础 UI 框架
- [x] 主窗口布局
- [x] 菜单栏
- [x] 工具栏
- [x] 状态栏
- [x] 侧边栏容器

### 阶段 2: 核心功能 (Core Features)

#### 2.1 文件状态管理

**功能:**
- `svn status` - 查看文件状态
- 文件树显示（带状态图标和颜色）
- 状态筛选
- 刷新状态

**状态图标映射:**
- ✓ 正常 (Normal) - 绿色
- M 修改 (Modified) - 蓝色
- A 添加 (Added) - 青色
- D 删除 (Deleted) - 红色
- C 冲突 (Conflicted) - 橙色
- I 忽略 (Ignored) - 灰色
- ? 未版本化 (Unversioned) - 浅灰色
- ! 缺失 (Missing) - 红色警告

**实现步骤:**
1. 实现 `SvnStatusService.GetStatusAsync()`
2. 创建 `FileTreeView` 控件
3. 创建 `SvnStatusToIconConverter`
4. 实现状态筛选逻辑
5. 添加刷新功能

#### 2.2 更新功能

**功能:**
- `svn update` - 更新到最新版本
- `svn update -r N` - 更新到指定版本
- 更新进度显示
- 更新冲突处理
- 更新日志显示

**实现步骤:**
1. 创建 `UpdateDialog`
2. 实现 `UpdateViewModel`
3. 添加进度条
4. 实时显示更新日志
5. 处理更新后的状态刷新

#### 2.3 提交功能

**功能:**
- `svn commit` - 提交变更
- 选择要提交的文件
- 提交消息输入
- 提交历史记录
- 提交后自动刷新

**实现步骤:**
1. 创建 `CommitDialog`
2. 实现 `CommitViewModel`
3. 文件选择列表
4. 提交消息编辑器
5. 执行提交并显示结果

#### 2.4 基础文件操作

**功能:**
- `svn add` - 添加文件到版本控制
- `svn delete` - 删除文件
- `svn revert` - 恢复文件
- 右键菜单集成
- 批量操作支持

**实现步骤:**
1. 实现文件右键菜单
2. 添加操作确认对话框
3. 执行操作并刷新状态
4. 撤销支持（Undo）

### 阶段 3: 高级功能 (Advanced Features)

#### 3.1 差异对比

**功能:**
- `svn diff` - 查看文件变更
- `svn diff -r N:M` - 查看版本间差异
- 统一差异视图
- 并排差异视图
- 代码语法高亮
- 差异导航

**实现步骤:**
1. 创建 `DiffViewer` 控件（使用 AvaloniaEdit）
2. 实现 `DiffService`
3. 解析 unified diff 格式
4. 高亮显示变更行
5. 添加导航功能（上一个/下一个差异）

#### 3.2 历史记录

**功能:**
- `svn log` - 查看提交历史
- `svn log -l N` - 限制显示数量
- `svn log --stop-on-copy` - 查看分支历史
- 按日期筛选
- 按作者筛选
- 按路径筛选
- 查看提交详情
- 查看提交变更的文件

**实现步骤:**
1. 创建 `HistoryView` 控件
2. 实现 `HistoryService.GetLogAsync()`
3. 日志列表显示
4. 日志筛选控件
5. 提交详情面板
6. 查看指定版本的差异

#### 3.3 分支与标签

**功能:**
- `svn copy` - 创建分支/标签
- `svn switch` - 切换工作副本
- `svn merge` - 合并分支
- `svn merge --dry-run` - 合并预览
- 合并冲突解决
- 分支图可视化（可选）

**实现步骤:**
1. 创建 `BranchTagDialog`
2. 创建 `MergeDialog`
3. 实现 `BranchTagService`
4. 源/目标路径选择
5. 合并预览功能
6. 冲突处理流程

#### 3.4 冲突解决

**功能:**
- 冲突文件检测
- 冲突标记可视化
- 冲突解决工具：
  - 使用我的（ResolveMine）
  - 使用他们的（ResolveTheirs）
  - 合并编辑器（Merge Editor）
  - 手动编辑
- 批量解决冲突

**实现步骤:**
1. 创建 `ConflictResolveDialog`
2. 实现三路合并编辑器
3. 高亮显示冲突区域
4. 添加解决按钮（使用我的/使用他们的）
5. 标记冲突为已解决

### 阶段 4: 远程操作 (Remote Operations)

#### 4.1 仓库浏览

**功能:**
- `svn list` - 浏览仓库结构
- `svn cat` - 查看远程文件
- `svn info` - 查看仓库信息
- 仓库树视图
- 文件内容预览

**实现步骤:**
1. 创建 `RemoteBrowserDialog`
2. 实现 `RemoteService`
3. 树形结构显示
4. 异步加载节点
5. 文件查看功能

#### 4.2 检出与导出

**功能:**
- `svn checkout` - 检出仓库
- `svn export` - 导出干净副本
- `svn import` - 导入到仓库
- 检出进度显示
- 仓库 URL 历史记录

**实现步骤:**
1. 创建 `CheckoutDialog`
2. 创建 `ExportDialog`
3. URL 输入和验证
4. 本地路径选择
5. 进度显示和日志

### 阶段 5: 增强功能 (Enhanced Features)

#### 5.1 清理与修复

**功能:**
- `svn cleanup` - 清理工作副本
- `svn cleanup --remove-unversioned` - 删除未版本化文件
- `svn cleanup --remove-ignored` - 删除忽略文件
- 解锁锁定文件
- 修复中断的操作

#### 5.2 信息查看

**功能:**
- `svn info` - 查看详细信息
- `svn blame` - 追溯文件作者
- `svn cat` - 查看文件内容
- 多版本文件对比

**实现步骤:**
1. 创建 `InfoDialog`
2. 创建 `BlameView`
3. 逐行显示作者和修订版
4. 点击行查看详情

#### 5.3 锁定管理

**功能:**
- `svn lock` - 锁定文件
- `svn unlock` - 解锁文件
- 查看锁定状态
- 锁定窃取
- 锁断破除

#### 5.4 属性管理

**功能:**
- `svn propget` - 获取属性
- `svn propset` - 设置属性
- `svn proplist` - 列出属性
- `svn propdel` - 删除属性
- 常用属性管理：
  - `svn:ignore`
  - `svn:eol-style`
  - `svn:mime-type`
  - `svn:keywords`
  - `svn:executable`

**实现步骤:**
1. 创建 `PropertyDialog`
2. 实现 `PropertyService`
3. 属性列表显示
4. 属性编辑器
5. 常用属性快捷设置

### 阶段 6: 用户体验优化 (UX Enhancement)

#### 6.1 快捷键支持

**常用快捷键:**
- `Ctrl+T` - 更新
- `Ctrl+K` - 提交
- `Ctrl+D` - 差异
- `Ctrl+Z` - 恢复
- `Ctrl+L` - 查看日志
- `F5` - 刷新
- `Delete` - 删除
- `Ctrl+A` - 全选

**实现步骤:**
1. 创建快捷键管理器
2. 在 Axaml 中定义 HotKey
3. 实现快捷键命令绑定
4. 添加快捷键设置界面

#### 6.2 右键菜单

**文件/文件夹右键菜单:**
- 更新
- 提交
- 差异
- 恢复
- 添加
- 删除
- 重命名
- 复制路径
- 在资源管理器中打开
- 查看日志
- 属性
- 锁/解锁

#### 6.3 搜索和筛选

**搜索功能:**
- 文件名搜索
- 文件内容搜索（使用 grep）
- 历史记录搜索

**筛选功能:**
- 按状态筛选
- 按路径筛选
- 按作者筛选
- 按日期筛选
- 保存筛选配置

#### 6.4 主题和外观

**主题支持:**
- 明亮主题
- 暗黑主题
- 自定义主题颜色
- 字体设置
- 界面缩放

#### 6.5 持久化设置

**设置项:**
- 最近打开的工作副本列表
- 仓库 URL 历史
- 窗口大小和位置
- 主题选择
- 快捷键配置
- 筛选器配置

## 项目目录结构

```
Svns/
├── docs/
│   ├── development-plan.md         # 开发计划（本文档）
│   ├── api-reference.md            # API 参考
│   └── ui-design.md                # UI 设计文档
│
├── Services/
│   ├── Svn/
│   │   ├── Core/
│   │   │   ├── SvnCommandService.cs        # SVN 命令执行服务
│   │   │   ├── SvnResult.cs                # 命令执行结果
│   │   │   ├── SvnCommand.cs               # SVN 命令定义
│   │   │   └── SvnException.cs             # SVN 异常
│   │   │
│   │   ├── Parsers/
│   │   │   ├── ISvnOutputParser.cs         # 输出解析器接口
│   │   │   ├── SvnStatusParser.cs          # 状态解析器
│   │   │   ├── SvnLogParser.cs             # 日志解析器
│   │   │   ├── SvnInfoParser.cs            # 信息解析器
│   │   │   ├── SvnDiffParser.cs            # 差异解析器
│   │   │   └── SvnListParser.cs            # 列表解析器
│   │   │
│   │   └── Operations/
│   │       ├── WorkingCopyService.cs       # 工作副本管理
│   │       ├── FileOperationService.cs     # 文件操作
│   │       ├── UpdateCommitService.cs      # 更新和提交
│   │       ├── HistoryService.cs           # 历史记录
│   │       ├── DiffService.cs              # 差异对比
│   │       ├── ConflictService.cs          # 冲突解决
│   │       ├── BranchTagService.cs         # 分支标签
│   │       ├── RemoteService.cs            # 远程操作
│   │       ├── PropertyService.cs          # 属性管理
│   │       └── LockService.cs              # 锁定管理
│   │
│   └── UI/
│       ├── DialogService.cs                # 对话框服务
│       ├── NotificationService.cs          # 通知服务
│       ├── ClipboardService.cs             # 剪贴板服务
│       └── SettingsService.cs              # 设置服务
│
├── Models/
│   ├── SvnStatus.cs                        # 文件状态模型
│   ├── SvnInfo.cs                          # 工作副本信息
│   ├── SvnLogEntry.cs                      # 提交历史
│   ├── SvnDiffResult.cs                    # 差异结果
│   ├── SvnConflict.cs                      # 冲突信息
│   ├── SvnProperty.cs                      # 文件属性
│   ├── SvnRepositoryInfo.cs                # 仓库信息
│   ├── SvnBlameLine.cs                     # Blame 行
│   ├── SvnChangeList.cs                    # 变更列表
│   └── WorkingCopyInfo.cs                  # 工作副本信息
│
├── ViewModels/
│   ├── MainWindowViewModel.cs              # 主窗口 ViewModel
│   ├── WorkingCopyViewModel.cs             # 工作副本 ViewModel
│   ├── ChangesViewModel.cs                 # 变更列表 ViewModel
│   ├── CommitViewModel.cs                  # 提交 ViewModel
│   ├── UpdateViewModel.cs                  # 更新 ViewModel
│   ├── HistoryViewModel.cs                 # 历史记录 ViewModel
│   ├── DiffViewModel.cs                    # 差异视图 ViewModel
│   ├── ConflictViewModel.cs                # 冲突解决 ViewModel
│   ├── SettingsViewModel.cs                # 设置 ViewModel
│   ├── RemoteBrowserViewModel.cs           # 远程浏览 ViewModel
│   ├── InfoViewModel.cs                    # 信息 ViewModel
│   └── BlameViewModel.cs                   # Blame ViewModel
│
├── Views/
│   ├── MainWindow.axaml                    # 主窗口
│   ├── MainWindow.axaml.cs
│   │
│   ├── Controls/
│   │   ├── FileTreeView.axaml              # 文件树控件
│   │   ├── FileTreeView.axaml.cs
│   │   ├── ChangesPanel.axaml              # 变更面板
│   │   ├── ChangesPanel.axaml.cs
│   │   ├── StatusPanel.axaml               # 状态面板
│   │   ├── StatusPanel.axaml.cs
│   │   ├── DiffViewer.axaml                # 差异查看器
│   │   ├── DiffViewer.axaml.cs
│   │   ├── HistoryView.axaml               # 历史记录视图
│   │   ├── HistoryView.axaml.cs
│   │   ├── BlameView.axaml                 # Blame 视图
│   │   ├── BlameView.axaml.cs
│   │   └── PropertyEditor.axaml            # 属性编辑器
│   │       └── PropertyEditor.axaml.cs
│   │
│   └── Dialogs/
│       ├── CommitDialog.axaml              # 提交对话框
│       ├── CommitDialog.axaml.cs
│       ├── UpdateDialog.axaml              # 更新对话框
│       ├── UpdateDialog.axaml.cs
│       ├── BranchTagDialog.axaml           # 分支标签对话框
│       ├── BranchTagDialog.axaml.cs
│       ├── MergeDialog.axaml               # 合并对话框
│       ├── MergeDialog.axaml.cs
│       ├── CheckoutDialog.axaml            # 检出对话框
│       ├── CheckoutDialog.axaml.cs
│       ├── ConflictResolveDialog.axaml     # 冲突解决对话框
│       ├── ConflictResolveDialog.axaml.cs
│       ├── SettingsDialog.axaml            # 设置对话框
│       ├── SettingsDialog.axaml.cs
│       └── InfoDialog.axaml                # 信息对话框
│           └── InfoDialog.axaml.cs
│
├── Converters/
│   ├── SvnStatusToIconConverter.cs         # 状态转图标
│   ├── SvnStatusToColorConverter.cs        # 状态转颜色
│   ├── SvnStatusToStringConverter.cs       # 状态转字符串
│   └── BoolToVisibilityConverter.cs        # 布尔转可见性
│
├── Resources/
│   ├── Icons/                              # 图标资源
│   │   ├── normal.ico
│   │   ├── modified.ico
│   │   ├── added.ico
│   │   ├── deleted.ico
│   │   ├── conflicted.ico
│   │   └── ignored.ico
│   └── Styles/                             # 样式资源
│       └── ThemeStyles.axaml               # 主题样式
│
├── Utils/
│   ├── PathHelper.cs                       # 路径工具
│   ├── SvnPathHelper.cs                    # SVN 路径工具
│   └── ProcessHelper.cs                    # 进程工具
│
├── Constants/
│   └── SvnConstants.cs                     # SVN 常量
│
├── App.axaml                               # 应用程序入口
├── App.axaml.cs
├── Program.cs
├── Svns.csproj
└── README.md
```

## SVN 命令功能清单

### 基础命令
- [x] `svn help` - 帮助信息
- [x] `svn status` - 查看文件状态
- [x] `svn update` - 更新工作副本
- [x] `svn commit` - 提交变更
- [x] `svn add` - 添加文件
- [x] `svn delete` (del, remove, rm) - 删除文件
- [x] `svn revert` - 恢复文件
- [x] `svn copy` (cp) - 复制文件/目录 (BranchTagDialog)
- [x] `svn move` (mv, rename, ren) - 移动/重命名 (RenameDialog)
- [x] `svn mkdir` - 创建目录并加入版本控制 (NewFolderCommand)
- [x] `svn checkout` (co) - 检出仓库 (CheckoutDialog)
- [x] `svn export` - 导出干净副本 (CheckoutDialog)
- [x] `svn import` - 导入到仓库 (ImportDialog)
- [x] `svn info` - 查看信息
- [x] `svn cat` - 查看文件内容 (CatAsync)
- [x] `svn list` (ls) - 列出目录内容 (RepositoryBrowserDialog)

### 信息查询
- [x] `svn log` - 查看提交历史
- [x] `svn diff` - 显示差异
- [x] `svn blame` (praise, annotate, ann) - 追溯作者 (BlameDialog)
- [x] `svn propget` (pget, pg) - 获取属性 (PropertyDialog)
- [x] `svn proplist` (plist, pl) - 列出属性 (PropertyDialog)
- [x] `svn propset` (pset, ps) - 设置属性 (PropertyDialog)
- [x] `svn propdel` (pdel, pd) - 删除属性 (PropertyDialog)

### 分支与标签
- [x] `svn switch` (sw) - 切换工作副本 (SwitchDialog)
- [x] `svn merge` - 合并差异 (MergeDialog)

### 维护命令
- [x] `svn cleanup` - 清理工作副本 (CleanupDialog)
- [x] `svn relocate` - 重新定位仓库 URL (RelocateDialog)

### 锁定
- [x] `svn lock` - 锁定文件 (LockDialog)
- [x] `svn unlock` - 解锁文件 (LockDialog)

### 冲突解决
- [x] `svn resolve` - 解决冲突 (ConflictResolveDialog)
- [x] `svn merge --accept` - 合并时接受策略 (MergeDialog)

## 开发阶段划分

### Phase 1: 基础架构 (Week 1-2)
**目标**: 建立项目基础，实现核心 SVN 命令执行引擎

**任务**:
1. 项目结构搭建
2. SVN 命令执行引擎
3. XML 输出解析器
4. 数据模型定义
5. 工作副本检测和管理
6. 基础 UI 框架

**验收标准**:
- 能够执行基本 SVN 命令
- 正确解析命令输出
- 能够检测和打开工作副本
- 基础 UI 框架可用

### Phase 2: 核心功能 (Week 3-4)
**目标**: 实现最基本的 SVN 操作

**任务**:
1. 文件状态查看
2. 文件树显示
3. 更新功能
4. 提交功能
5. 基础文件操作（Add, Delete, Revert）

**验收标准**:
- 能够查看文件状态
- 能够执行 Update 操作
- 能够执行 Commit 操作
- 能够执行基础文件操作

### Phase 3: 高级功能 (Week 5-6)
**目标**: 实现差异对比和历史记录

**任务**:
1. 差异对比功能
2. 历史记录查看
3. 分支与标签管理
4. 冲突解决

**验收标准**:
- 能够查看文件差异
- 能够查看提交历史
- 能够创建分支/标签
- 能够解决冲突

### Phase 4: 远程操作 (Week 7)
**目标**: 实现远程仓库操作

**任务**:
1. 仓库浏览器
2. 检出功能
3. 导出功能
4. 远程文件查看

**验收标准**:
- 能够浏览远程仓库
- 能够检出仓库
- 能够导出文件

### Phase 5: 增强功能 (Week 8)
**目标**: 实现辅助功能

**任务**:
1. 清理与修复
2. 信息查看
3. 锁定管理
4. 属性管理

**验收标准**:
- 能够清理工作副本
- 能够查看详细信息
- 能够管理锁定
- 能够管理属性

### Phase 6: 用户体验优化 (Week 9)
**目标**: 优化用户体验

**任务**:
1. 快捷键支持
2. 右键菜单
3. 搜索和筛选
4. 主题支持
5. 设置持久化

**验收标准**:
- 快捷键工作正常
- 右键菜单完整
- 搜索筛选功能完善
- 主题切换正常

### Phase 7: 测试与优化 (Week 10)
**目标**: 测试和性能优化

**任务**:
1. 单元测试
2. 集成测试
3. 性能优化
4. Bug 修复
5. 文档完善

**验收标准**:
- 测试覆盖率达到 70%
- 无严重 Bug
- 性能满足要求
- 文档完整

## 依赖项 NuGet 包

### 必需
- `Avalonia` (11.3.10) - UI 框架 ✅
- `Avalonia.Desktop` (11.3.10) - 桌面平台 ✅
- `Avalonia.Themes.Fluent` (11.3.10) - Fluent 主题 ✅
- `Avalonia.Fonts.Inter` (11.3.10) - 字体 ✅
- `CommunityToolkit.Mvvm` (8.2.1) - MVVM 框架 ✅

### 推荐添加
- `AvaloniaEdit` - 代码/差异编辑器
- `Material.Styles` - Material Design 主题
- `Serilog` - 日志记录
- `Splat` - 依赖注入
- `ReactiveUI` - 响应式扩展
- `System.CommandLine` - 命令行参数解析

### 可选
- `Avalonia.Visualizer` - UI 可视化调试
- `Avalonia.Diagnostics` (已有) - 开发工具 ✅

## UI 设计参考

### 主窗口布局

```
+--------------------------------------------------+
| Menu Bar: File Edit View History Tools Help      |
+--------------------------------------------------+
| Tool Bar: [Update] [Commit] [Diff] [Refresh]     |
+--------------------------------------------------+
|                                                  |
|  +----------------+  +-------------------------+ |
|  |                |  |                         | |
|  |   File Tree    |  |   Changes Panel         | |
|  |                |  |                         | |
|  |  - src/        |  |  Modified Files:        | |
|  |    - App.cs    |  |  ✓ App.cs              | |
|  |    - Utils.cs  |  |  ✓ Utils.cs            | |
|  |  - test/       |  |                         | |
|  |                |  +-------------------------+ |
|  |                |  |                         | |
|  |                |  |   Details Panel         | |
|  |                |  |                         | |
|  |                |  +-------------------------+ |
|  |                |                            | |
|  +----------------+                            | |
|                                                  |
+--------------------------------------------------+
| Status Bar: Ready | Branch: main | Rev: 123      |
+--------------------------------------------------+
```

### 文件状态图标

| 状态 | 图标 | 颜色 | 说明 |
|------|------|------|------|
| Normal | ✓ | 绿色 | 无变更 |
| Modified | ✎ | 蓝色 | 已修改 |
| Added | + | 青色 | 已添加 |
| Deleted | - | 红色 | 已删除 |
| Conflicted | ⚠ | 橙色 | 有冲突 |
| Ignored | ⊘ | 灰色 | 已忽略 |
| Unversioned | ? | 浅灰 | 未版本控制 |
| Missing | ! | 红色 | 文件缺失 |
| Replaced | ↻ | 紫色 | 已替换 |

### 快捷键列表

| 快捷键 | 功能 |
|--------|------|
| Ctrl+O | 打开工作副本 |
| Ctrl+T | 更新 |
| Ctrl+K | 提交 |
| Ctrl+D | 差异 |
| Ctrl+Z | 恢复 |
| Ctrl+L | 查看日志 |
| Ctrl+A | 全选 |
| Delete | 删除 |
| F5 | 刷新 |
| Ctrl+Shift+C | 打开提交对话框 |
| Ctrl+Shift+U | 打开更新对话框 |

## 性能优化策略

### 1. 异步操作
- 所有 SVN 命令执行都使用 `async/await`
- 避免阻塞 UI 线程
- 使用 `CancellationToken` 支持取消

### 2. 数据缓存
- 缓存工作副本状态
- 缓存日志记录
- 按需加载（Lazy Loading）

### 3. 虚拟化
- 使用 `VirtualizingStackPanel` 处理大列表
- 延迟加载树节点

### 4. 增量更新
- 只刷新变更的文件
- 使用文件系统监听器

### 5. 并行处理
- 并行执行多个独立的 SVN 命令
- 使用 `Task.WhenAll` 批量操作

## 安全考虑

### 1. 密码管理
- 使用系统密钥链存储密码
- 支持 SSH 密钥认证
- 不在日志中记录敏感信息

### 2. 命令注入防护
- 参数化所有命令行参数
- 验证用户输入
- 转义特殊字符

### 3. 权限控制
- 遵循文件系统权限
- 不执行未授权的操作

## 测试策略

### 1. 单元测试
- 测试所有服务类
- 测试数据模型
- 测试解析器

### 2. 集成测试
- 测试 SVN 命令执行
- 测试 UI 交互
- 端到端测试

### 3. 手动测试
- 真实工作副本测试
- 不同场景测试
- 边界情况测试

## 部署和发布

### 1. 构建配置
- Debug 模式 - 开发调试
- Release 模式 - 生产发布

### 2. 发布方式
- 单文件发布
- 依赖框架发布
- 自包含发布

### 3. 安装程序
- Windows: MSI / InnoSetup
- macOS: DMG / PKG
- Linux: AppImage / DEB / RPM

## 未来扩展功能

### 可选功能 (V2.0)
- Git SVN 互操作
- 图形化分支合并工具
- 代码审查集成
- 持续集成集成
- 云存储集成
- 多语言支持（i18n）
- 插件系统

## 参考资料

- [SVN Book (官方文档)](https://svnbook.red-bean.com/)
- [Avalonia UI 文档](https://docs.avaloniaui.net/)
- [CommunityToolkit.Mvvm 文档](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [SharpSvn 文档](https://sharpsvn.open.collab.net/)

## 更新日志

### 2026-01-10 (Phase 3 + Phase 4 完成)
- 完成 Phase 3 高级功能：
  - 分支/标签对话框 (BranchTagDialog) - svn copy
  - 切换对话框 (SwitchDialog) - svn switch
  - 合并对话框 (MergeDialog) - svn merge
  - 冲突解决对话框 (ConflictResolveDialog) - svn resolve

- 完成 Phase 4 远程操作：
  - 仓库浏览器对话框 (RepositoryBrowserDialog) - svn list
  - 检出/导出对话框 (CheckoutDialog) - svn checkout/export

- 新增服务方法 (WorkingCopyService):
  - CopyAsync - 创建分支/标签
  - SwitchAsync - 切换工作副本
  - MergeAsync - 合并变更
  - ListAsync - 列出仓库内容
  - CheckoutAsync - 检出仓库
  - ExportAsync - 导出仓库
  - BlameAsync - 追溯文件作者
  - LockAsync/UnlockAsync - 锁定/解锁文件

- 新增 ViewModel:
  - BranchTagViewModel - 分支标签管理
  - SwitchViewModel - 切换工作副本
  - MergeViewModel - 合并功能
  - ConflictResolveViewModel - 冲突解决
  - RepositoryBrowserViewModel - 仓库浏览
  - CheckoutViewModel - 检出/导出

- 新增 Converters:
  - BoolToStringConverter - 布尔值转字符串
  - BoolToBrushConverter - 布尔值转画刷

- 主窗口集成：
  - MainWindowViewModel 添加新命令:
    - BranchTagCommand - 打开分支/标签对话框
    - SwitchCommand - 打开切换对话框
    - MergeCommand - 打开合并对话框
    - ResolveConflictsCommand - 打开冲突解决对话框
    - BrowseRepositoryCommand - 打开仓库浏览器
    - CheckoutCommand - 打开检出/导出对话框
  - MainWindow 新增菜单:
    - SVN 菜单增加 "Resolve Conflicts..."
    - 新增 "Branch" 菜单 (Create Branch/Tag, Switch, Merge)
    - 新增 "Repository" 菜单 (Checkout, Browse Repository)

### 2026-01-10 (Phase 2 完成 + 单元测试)
- 完成 Phase 2 核心功能：
  - 实现 CommitDialog 提交对话框
  - 实现 svn add/delete/revert/diff 命令
  - 添加文件树右键菜单
  - 完善 MainWindowViewModel 功能
  - 实现状态筛选功能 (StatusFilterType)
  - 完善 DiffViewer 差异查看器窗口 (DiffDialog)
  - 实现确认对话框 (ConfirmDialog - 删除/恢复操作)
  - 实现文件历史对话框 (HistoryDialog)

- 添加单元测试项目 (Svns.Tests):
  - Models 测试：
    - SvnStatusTests - 文件状态模型测试
    - SvnLogEntryTests - 日志条目模型测试
    - SvnInfoTests - 工作副本信息测试
    - SvnConflictTests - 冲突模型测试
    - SvnBlameTests - Blame 模型测试
    - StatusFilterTypeTests - 状态筛选类型测试
  - ViewModels 测试：
    - DiffViewModelTests - 差异视图测试
    - CommitViewModelTests - 提交视图测试 (CommitFileItem)
  - Services 测试：
    - SvnCommandTests - SVN 命令构建测试
    - SvnResultTests - 命令结果测试
    - SvnExceptionTests - 异常类测试
    - SvnStatusParserTests - 状态解析器测试
    - SvnLogParserTests - 日志解析器测试
  - Converters 测试：
    - DiffLineConvertersTests - 差异行转换器测试

- 测试覆盖：184 个测试全部通过

### 2025-01-10
- 创建开发计划文档
- 定义项目架构
- 规划功能模块
- 制定开发阶段

---

**当前状态**: 🎯 Phase 8 - 功能完善完成 (100%) ✅

**已完成**:
- Phase 1 基础架构 ✅
- Phase 2 核心功能 ✅
  - 文件状态管理 ✅
  - 状态筛选功能 ✅
  - 更新功能 ✅
  - 提交功能 (CommitDialog) ✅
  - 基础文件操作 (Add/Delete/Revert) ✅
  - 差异查看 (DiffDialog) ✅
  - 文件历史 (HistoryDialog) ✅
  - 确认对话框 ✅
  - 右键菜单 ✅
- Phase 3 高级功能 ✅
  - 分支/标签管理 (BranchTagDialog) ✅
  - 切换工作副本 (SwitchDialog) ✅
  - 合并功能 (MergeDialog) ✅
  - 冲突解决 (ConflictResolveDialog) ✅
- Phase 4 远程操作 ✅
  - 仓库浏览器 (RepositoryBrowserDialog) ✅
  - 检出/导出 (CheckoutDialog) ✅
- Phase 5 增强功能 ✅
  - 清理与修复 (CleanupDialog) ✅
  - 信息查看 (InfoDialog) ✅
  - Blame 视图 (BlameDialog) ✅
  - 锁定管理 (LockDialog) ✅
  - 属性管理 (PropertyDialog) ✅
- Phase 6 用户体验优化 ✅
  - 重命名功能 (RenameDialog) ✅
  - 日志筛选 (Author/Message/Date) ✅
  - 窗口位置/大小持久化 ✅
  - 最近项目功能 ✅
  - 主题支持 ✅
  - 右键菜单完善 ✅
- Phase 7 测试与优化 ✅
  - 单元测试 (245 tests) ✅
  - 性能优化 (虚拟化) ✅
  - 内存缓存工具 ✅
  - Bug 修复 ✅
- Phase 8 功能完善 ✅
  - svn mkdir/cat/import/relocate/export ✅
  - svn merge --accept 冲突处理策略 ✅
  - 单元测试 (280 tests) ✅
- 主窗口菜单集成 ✅
  - Branch 菜单 (Branch/Tag, Switch, Merge) ✅
  - Repository 菜单 (Checkout, Browse Repository, Import, Relocate) ✅
  - SVN 菜单 (Resolve Conflicts) ✅

**SVN 命令完成度**: 29/29 (100%) 🎉

**项目完成状态**: 🎉 所有 SVN 命令已实现，可进入 Beta 测试阶段


