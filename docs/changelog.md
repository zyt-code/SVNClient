# Svns - 开发进度更新

## 2026-01-11 - Bug修复与消息通知中心

### ✅ Bug修复

#### 提交后刷新问题修复
- ✅ 修复 SVN 提交后左侧文件树和右侧提交历史不更新的问题
- ✅ `CommitViewModel` 添加 `WasCommitSuccessful` 属性追踪提交状态
- ✅ `MainWindowViewModel.CommitAsync()` 在对话框关闭后检查提交状态并刷新
- ✅ 确保文件状态和提交历史正确同步

### ✅ 新增功能

#### 消息通知中心
- ✅ 创建 `Models/NotificationMessage.cs` - 通知消息模型
  - 支持 4 种通知类型：Info、Success、Warning、Error
  - 每种类型有独特的图标和颜色
  - 支持时间戳和已读状态
- ✅ 创建 `Services/NotificationService.cs` - 通知服务（单例模式）
  - 管理所有应用通知
  - 自动限制保存最近 100 条通知
  - 未读计数追踪
  - 支持标记已读/清除所有
- ✅ 状态栏右下角添加铃铛图标（Material Icons BellOutline）
  - 未读消息红色徽章显示
  - 点击打开/关闭通知面板
- ✅ 右侧通知侧边栏（320px 宽）
  - 通知列表展示（图标、标题、消息、时间）
  - "Mark all read" 按钮标记所有为已读
  - "Clear all" 按钮清除所有通知
  - 关闭按钮
- ✅ 创建 `Converters/IntToVisibilityConverter.cs` - 整数转可见性转换器
  - 当值 > 0 时返回 true（用于未读徽章）

### 📁 新增/修改文件

```
Models/
└── NotificationMessage.cs           # 新增：通知消息模型

Services/
└── NotificationService.cs           # 新增：通知服务（单例）

Converters/
└── IntToVisibilityConverter.cs      # 新增：整数转可见性转换器

ViewModels/
├── CommitViewModel.cs               # 修改：添加 WasCommitSuccessful 属性
└── MainWindowViewModel.cs           # 修改：添加通知服务和相关命令

Views/
└── MainWindow.axaml                 # 修改：添加通知侧边栏和铃铛图标
```

### 🎨 UI 改进

#### 通知侧边栏布局
```
┌───────────────────────────────────┐
│ Notifications          5 messages [×]│
├───────────────────────────────────┤
│ [✓ Mark all read] [🗑 Clear all] │
├───────────────────────────────────┤
│ ℹ Title              14:23        │
│   Message content...             │
├───────────────────────────────────┤
│ ✓ Success            14:20        │
│   Operation completed            │
├───────────────────────────────────┤
│ ⚠ Warning            14:15        │
│   Something needs attention       │
├───────────────────────────────────┤
│ ⛔ Error              14:10        │
│   An error occurred               │
└───────────────────────────────────┘
```

#### 状态栏铃铛图标
```
┌────────────────────────────────────────────────────────┐
│ Working copy loaded                         🔔(5) v1.0│
└────────────────────────────────────────────────────────┘
                                                  ↑
                                            未读徽章（红色）
```

### 🔧 技术细节

#### NotificationService 使用示例
```csharp
// 获取服务实例
var notificationService = NotificationService.Instance;

// 添加通知
notificationService.Info("标题", "信息消息");
notificationService.Success("成功", "操作完成");
notificationService.Warning("警告", "需要注意的事项");
notificationService.Error("错误", "操作失败");

// 批量操作
notificationService.MarkAllAsRead();
notificationService.ClearAll();

// 切换面板
notificationService.TogglePanel();
```

#### 通知类型颜色和图标
| 类型 | 图标 | 颜色 | 描述 |
|-----|------|------|------|
| Info | InformationOutline | #2196F3 (蓝) | 信息提示 |
| Success | CheckCircleOutline | #4CAF50 (绿) | 操作成功 |
| Warning | AlertOutline | #FF9800 (橙) | 警告提示 |
| Error | AlertCircleOutline | #F44336 (红) | 错误信息 |

---

## 2026-01-10 - 功能完善 Phase 8

### ✅ 新增 SVN 命令支持

#### 1. svn mkdir - 创建目录
- ✅ 实现 `SvnCommand.Mkdir()` 方法
- ✅ 支持 `--parents` 选项
- ✅ 支持远程仓库创建 (带 -m 消息)
- ✅ 右键菜单添加 "New Folder..." 选项
- ✅ 实现 `WorkingCopyService.MkdirAsync()`

#### 2. svn cat - 查看远程文件内容
- ✅ 实现 `SvnCommand.Cat()` 方法
- ✅ 支持指定版本 (-r)
- ✅ 实现 `WorkingCopyService.CatAsync()`

#### 3. svn import - 导入文件到仓库
- ✅ 创建 `ImportViewModel` 和 `ImportDialog`
- ✅ 支持选择本地文件夹
- ✅ 支持 `--no-ignore` 选项
- ✅ Repository 菜单添加 "Import..." 选项
- ✅ 实现 `WorkingCopyService.ImportAsync()`

#### 4. svn relocate - 重定位仓库 URL
- ✅ 创建 `RelocateViewModel` 和 `RelocateDialog`
- ✅ 自动加载当前仓库 URL
- ✅ Repository 菜单添加 "Relocate..." 选项

#### 5. svn export - 导出增强
- ✅ 实现 `SvnCommand.Export()` 方法
- ✅ 支持 `--force` 选项

#### 6. svn merge --accept - 合并冲突处理策略
- ✅ 创建 `MergeAcceptType` 枚举 (8种策略)
  - Postpone - 延迟处理冲突
  - Base - 使用基础版本
  - MineConflict - 使用我的版本（仅冲突部分）
  - TheirsConflict - 使用他们的版本（仅冲突部分）
  - MineFull - 完全使用我的版本
  - TheirsFull - 完全使用他们的版本
  - Edit - 手动编辑
  - Launch - 启动外部工具
- ✅ 更新 `SvnCommand.Merge()` 支持 `--accept` 参数
- ✅ 更新 `WorkingCopyService.MergeAsync()` 支持 accept 参数
- ✅ 更新 `MergeViewModel` 添加冲突策略选择
- ✅ 更新 `MergeDialog` UI 添加策略下拉框
- ✅ 创建 `MergeAcceptTypeConverter` 转换器

### ✅ 单元测试 (23 新增，总计 280)
- ✅ `Mkdir_CreatesCorrectCommand`
- ✅ `Mkdir_WithMessage_AddsMessageFlag`
- ✅ `Mkdir_WithParents_AddsParentsFlag`
- ✅ `Cat_CreatesCorrectCommand`
- ✅ `Cat_WithRevision_AddsRevisionFlag`
- ✅ `Import_CreatesCorrectCommand`
- ✅ `Import_WithNoIgnore_AddsNoIgnoreFlag`
- ✅ `Relocate_CreatesCorrectCommand`
- ✅ `Relocate_WithPath_AddsPath`
- ✅ `Export_CreatesCorrectCommand`
- ✅ `Export_WithRevision_AddsRevisionFlag`
- ✅ `Export_WithForce_AddsForceFlag`
- ✅ `Merge_WithAccept_AddsAcceptFlag`
- ✅ `Merge_WithAcceptMineConflict_AddsAcceptFlag`
- ✅ `Merge_WithAcceptTheirsConflict_AddsAcceptFlag`
- ✅ `Merge_WithAcceptMineFull_AddsAcceptFlag`
- ✅ `Merge_WithAcceptTheirsFull_AddsAcceptFlag`
- ✅ `Merge_WithDryRunAndAccept_AddsBothFlags`
- ✅ `MergeAcceptType_ToSvnArgument` (8 个测试)
- ✅ `MergeAcceptType_GetDescription` (8 个测试)

### 📁 新增/修改文件

```
Models/
└── MergeAcceptType.cs              # 新增：合并冲突处理策略枚举

Converters/
└── MergeAcceptTypeConverter.cs     # 新增：策略枚举转换器

ViewModels/
├── RelocateViewModel.cs            # 新增：重定位 ViewModel
├── ImportViewModel.cs              # 新增：导入 ViewModel
├── MergeViewModel.cs               # 修改：添加 accept 策略
└── MainWindowViewModel.cs          # 添加 Relocate/Import/NewFolder 命令

Views/Dialogs/
├── RelocateDialog.axaml(.cs)       # 新增：重定位对话框
├── ImportDialog.axaml(.cs)         # 新增：导入对话框
└── MergeDialog.axaml               # 修改：添加策略选择 UI

Views/
└── MainWindow.axaml                # Repository 菜单添加项

Services/Svn/Core/
└── SvnCommand.cs                   # 添加 Mkdir/Cat/Import/Export/Relocate，更新 Merge

Services/Svn/Operations/
└── WorkingCopyService.cs           # 添加 MkdirAsync/CatAsync/ImportAsync，更新 MergeAsync

Svns.Tests/Services/
└── SvnCommandTests.cs              # 添加 18 个新测试

Svns.Tests/Models/
└── MergeAcceptTypeTests.cs         # 新增：策略枚举测试
```

### 🎯 SVN 命令完成度
- 已实现: 29/29 (100%) 🎉
- 所有 SVN 命令已完成实现！

---

## 2026-01-10 - 测试与性能优化 Phase 7

### ✅ 单元测试增强

#### 新增测试文件
- ✅ `Services/SvnCommandTests.cs` - Move/Lock/Unlock/Property 命令测试 (18 新测试)
- ✅ `ViewModels/RenameViewModelTests.cs` - 文件名验证和 HasChanges 逻辑测试
- ✅ `ViewModels/PropertyViewModelTests.cs` - PropertyItem 修改跟踪测试
- ✅ `Services/AppSettingsServiceTests.cs` - 窗口/应用/最近项目设置测试
- ✅ `ViewModels/StartPageViewModelTests.cs` - RecentProjectItem/RoadmapItem/相对日期测试
- ✅ `ViewModels/BlameViewModelTests.cs` - BlameLineItem/AuthorStatItem 测试

#### 测试覆盖率
- ✅ 总测试数: 245 个测试全部通过
- ✅ 修复重复 InlineData 警告 (SvnStatusTests)
- ✅ 修复 PropertyItem 初始化顺序问题

### ✅ 性能优化

#### 1. UI 虚拟化
- ✅ BlameDialog - ListBox 使用 VirtualizingStackPanel
- ✅ MainWindow - LogEntries 使用 ListBox + VirtualizingStackPanel
- ✅ 大列表渲染性能显著提升

#### 2. 内存缓存工具类
- ✅ 创建 `Utils/MemoryCache.cs` - 线程安全的内存缓存
- ✅ 支持时间过期策略
- ✅ 支持 GetOrAddAsync 模式
- ✅ 自动清理过期条目

### 📁 新增/修改文件

```
Svns.Tests/
├── ViewModels/
│   ├── RenameViewModelTests.cs         # 新增
│   ├── PropertyViewModelTests.cs       # 新增
│   ├── StartPageViewModelTests.cs      # 新增
│   └── BlameViewModelTests.cs          # 新增
├── Services/
│   ├── SvnCommandTests.cs              # 扩展：18 个新测试
│   └── AppSettingsServiceTests.cs      # 新增
└── Models/
    └── SvnStatusTests.cs               # 修复重复 InlineData

Utils/
└── MemoryCache.cs                      # 新增：通用内存缓存

Views/
├── MainWindow.axaml                    # 优化：LogEntries 虚拟化
└── Dialogs/
    └── BlameDialog.axaml               # 优化：Lines 虚拟化
```

### 🔧 技术细节

#### MemoryCache 使用示例
```csharp
var cache = new MemoryCache<string, SvnInfo>(TimeSpan.FromMinutes(5));

// 获取或添加缓存
var info = await cache.GetOrAddAsync("key", async () =>
{
    return await _workingCopyService.GetInfoAsync(path);
});

// 手动设置
cache.Set("key", value, TimeSpan.FromMinutes(10));

// 按条件清除
cache.Invalidate(key => key.StartsWith("/path"));
```

#### 虚拟化配置
```xml
<ListBox ItemsSource="{Binding Items}">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

---

## 2026-01-10 - UX 增强 Phase 6

### ✅ 新增功能

#### 1. 重命名对话框 (RenameDialog)
- ✅ 创建 `RenameViewModel` 和 `RenameDialog`
- ✅ 支持 SVN 文件/文件夹重命名 (`svn move`)
- ✅ 文件名验证（无效字符检测）
- ✅ 目标已存在检测
- ✅ 右键菜单集成
- ✅ F2 快捷键支持

#### 2. 日志筛选增强
- ✅ 作者筛选（ComboBox 下拉选择）
- ✅ 提交消息搜索
- ✅ 日期范围筛选（From/To）
- ✅ 一键清除筛选按钮
- ✅ 筛选状态消息提示

#### 3. 窗口位置/大小持久化
- ✅ 窗口大小自动保存
- ✅ 窗口位置自动保存
- ✅ 最大化状态保存
- ✅ 多显示器支持（验证位置有效性）
- ✅ 关闭时自动保存

#### 4. 最近项目功能
- ✅ 最近打开项目列表显示
- ✅ 点击快速打开项目
- ✅ 移除单个项目按钮
- ✅ 清除全部按钮
- ✅ 相对时间显示（x hours ago）
- ✅ 无效路径自动移除

### 🎨 UI 改进

#### 日志筛选区域布局
```
┌─────────────────────────────────────────────────────┐
│ Commit History                                       │
├─────────────────────────────────────────────────────┤
│ [Author...    ▼] [Message search...           ]     │
│ [From date...   ] - [To date...   ] [🗑 Clear]     │
├─────────────────────────────────────────────────────┤
│ A Added  M Modified  D Deleted  R Replaced          │
├─────────────────────────────────────────────────────┤
│ [r123] author      2 changes                        │
│ 3 hours ago                                         │
│ ...                                                 │
└─────────────────────────────────────────────────────┘
```

#### 最近项目区域布局
```
┌─────────────────────────────────────────────────────┐
│ Recent Projects                               [🗑]  │
├─────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────┬──┐ │
│ │ MyProject                                   │ ✕│ │
│ │ C:\path\to\working\copy                     │  │ │
│ │ 2 hours ago                                 │  │ │
│ └─────────────────────────────────────────────┴──┘ │
│ ┌─────────────────────────────────────────────┬──┐ │
│ │ AnotherProject                              │ ✕│ │
│ │ D:\another\path                             │  │ │
│ │ 3 days ago                                  │  │ │
│ └─────────────────────────────────────────────┴──┘ │
└─────────────────────────────────────────────────────┘
```

### 📁 新增/修改文件

```
ViewModels/
├── RenameViewModel.cs              # 重命名对话框 ViewModel
├── MainWindowViewModel.cs          # 添加日志筛选/重命名命令
└── StartPageViewModel.cs           # 添加最近项目管理

Views/Dialogs/
└── RenameDialog.axaml(.cs)         # 重命名对话框

Views/
├── MainWindow.axaml                # 日志筛选 UI / 重命名菜单项
├── MainWindow.axaml.cs             # 窗口位置持久化
├── StartPageWindow.axaml           # 最近项目 UI
└── StartPageWindow.axaml.cs        # 最近项目事件处理

Services/
├── AppSettingsService.cs           # 窗口设置保存/加载方法

Services/Svn/Core/
└── SvnCommand.cs                   # 添加 Move 命令

Services/Svn/Operations/
└── WorkingCopyService.cs           # 添加 MoveAsync 方法
```

### 🔧 技术细节

#### MainWindowViewModel 新增属性
```csharp
// 日志筛选
[ObservableProperty] private string _logAuthorFilter;
[ObservableProperty] private string _logMessageFilter;
[ObservableProperty] private DateTime? _logDateFrom;
[ObservableProperty] private DateTime? _logDateTo;
[ObservableProperty] private ObservableCollection<string> _availableAuthors;

// 筛选方法
private void ApplyLogFilter();
private bool MatchesLogFilter(SvnLogEntry log);
[RelayCommand] private void ClearLogFilters();
```

#### AppSettingsService 新增方法
```csharp
Task SaveWindowSettingsAsync(WindowSettings windowSettings);
Task<WindowSettings> GetWindowSettingsAsync();
```

#### 右键菜单新增项
```xml
<MenuItem Header="Rename..." Command="{Binding RenameCommand}" InputGesture="F2"/>
```

#### 快捷键
- `F2` - 重命名选中文件/文件夹

---

## 2026-01-10 - 增强功能 Phase 5 v6

### ✅ 新增功能

#### 1. 清理对话框 (CleanupDialog)
- ✅ 创建 `CleanupViewModel` 和 `CleanupDialog`
- ✅ 支持标准清理操作 (`svn cleanup`)
- ✅ 支持删除未版本化文件 (`--remove-unversioned`)
- ✅ 支持删除忽略文件 (`--remove-ignored`)
- ✅ 操作完成状态显示

#### 2. 信息对话框 (InfoDialog)
- ✅ 创建 `InfoViewModel` 和 `InfoDialog`
- ✅ 显示工作副本详细信息
- ✅ 显示仓库信息 (URL, Root, UUID)
- ✅ 显示版本信息 (Revision, Last Changed)
- ✅ 显示本地更改统计 (Modified/Added/Deleted/Conflicted)

#### 3. Blame 视图 (BlameDialog)
- ✅ 创建 `BlameViewModel` 和 `BlameDialog`
- ✅ 显示文件每行的修改者和修订号
- ✅ 作者颜色编码
- ✅ 作者统计侧边栏
- ✅ 行号、日期、内容显示

#### 4. 锁定对话框 (LockDialog)
- ✅ 创建 `LockViewModel` 和 `LockDialog`
- ✅ 支持锁定文件 (`svn lock`)
- ✅ 支持解锁文件 (`svn unlock`)
- ✅ 支持强制锁定 (steal lock)
- ✅ 锁定消息输入

#### 5. 属性对话框 (PropertyDialog)
- ✅ 创建 `PropertyViewModel` 和 `PropertyDialog`
- ✅ 显示文件/文件夹的 SVN 属性
- ✅ 支持添加新属性
- ✅ 支持编辑属性值
- ✅ 支持删除属性
- ✅ 常用属性快捷选择 (svn:ignore, svn:eol-style 等)

### 🎨 UI 改进

#### Blame 视图布局
```
┌──────────────────────────────────────────────────────────────────────────┐
│ 🔍 Blame (Annotate)                                  100 lines │ 5 rev │ 3 authors │
├──────────────────────────────────────────────────────────────────────────┤
│ Line │  Rev  │ Author      │ Date       │ Content                        │
├──────────────────────────────────────────────────────────────────────────┤
│   1  │ r123  │ ● alice     │ 2025-01-10 │ using System;                  │
│   2  │ r120  │ ● bob       │ 2025-01-08 │ namespace App;                 │
│   3  │ r123  │ ● alice     │ 2025-01-10 │                                │
│ ...                                                                       │
└──────────────────────────────────────────────────────────────────────────┘
```

#### 属性对话框布局
```
┌─────────────────────────────────────────────────────┐
│ 🏷 SVN Properties                                   │
│ filename.cs                                         │
├─────────────────────────────────────────────────────┤
│ Property        │ Value                    │ Actions│
├─────────────────────────────────────────────────────┤
│ 🏷 svn:eol-style │ [native          ]       │ [🗑]   │
│ 🏷 svn:keywords  │ [Id Author Date  ]  ●    │ [🗑]   │
├─────────────────────────────────────────────────────┤
│ Add New Property                                    │
│ Name: [svn:ignore    ▼]  Value: [*.log    ] [Add]  │
└─────────────────────────────────────────────────────┘
```

### 📁 新增/修改文件

```
ViewModels/
├── CleanupViewModel.cs             # 清理对话框 ViewModel
├── InfoViewModel.cs                # 信息对话框 ViewModel
├── BlameViewModel.cs               # Blame 视图 ViewModel
├── LockViewModel.cs                # 锁定对话框 ViewModel
├── PropertyViewModel.cs            # 属性对话框 ViewModel
└── MainWindowViewModel.cs          # 添加 Blame/Lock/Property 命令

Views/Dialogs/
├── CleanupDialog.axaml(.cs)        # 清理对话框
├── InfoDialog.axaml(.cs)           # 信息对话框
├── BlameDialog.axaml(.cs)          # Blame 视图
├── LockDialog.axaml(.cs)           # 锁定对话框
└── PropertyDialog.axaml(.cs)       # 属性对话框

Services/Svn/Operations/
└── WorkingCopyService.cs           # 添加属性管理方法

Views/
└── MainWindow.axaml                # 右键菜单添加 Blame/Lock/Properties
```

### 🔧 技术细节

#### WorkingCopyService 新增方法
```csharp
// 属性管理
Task<Dictionary<string, string>> GetPropertiesAsync(string path);
Task<string?> GetPropertyAsync(string path, string propertyName);
Task<SvnResult> SetPropertyAsync(string path, string propertyName, string value);
Task<SvnResult> DeletePropertyAsync(string path, string propertyName);
```

#### 右键菜单新增项
```xml
<MenuItem Header="Blame" Command="{Binding BlameCommand}"/>
<MenuItem Header="Lock/Unlock..." Command="{Binding LockFileCommand}"/>
<MenuItem Header="Properties..." Command="{Binding ShowPropertiesCommand}"/>
```

---

## 2026-01-10 - UI 细节优化与多选提交 v5

### ✅ 新增功能

#### 1. 文件树多选提交支持
- ✅ 文件树每个项目添加 CheckBox
- ✅ `SvnStatus` 模型添加 `IsSelected` 属性（默认选中）
- ✅ 使用 `ObservableObject` 支持双向绑定
- ✅ 支持选择性提交文件

#### 2. 右侧面板可调整大小
- ✅ 添加 GridSplitter 在中间区域和右侧面板之间
- ✅ 右侧面板宽度可通过拖动调整
- ✅ 修复布局问题（5列布局：左面板/分隔/中心/分隔/右面板）

#### 3. UI 细节优化
- ✅ 修复右侧图例（Legend）被截断的问题
- ✅ 使用 WrapPanel 替代 StackPanel，图例自动换行
- ✅ 调整右侧面板内边距，统一为 16px
- ✅ Material Icons 样式正确注册（MaterialIconStyles）

### 🎨 UI 改进

#### 文件树布局（含 CheckBox）
```
┌─────────────────────────────────────────────────────────┐
│ ☑ 📄 ✓ README.txt                                       │
│ ☑ 📁 ? .svns                              [Unversioned] │
│   ☑ 📄   config.json                                    │
│ ☐ 📄 M  modified-file.cs                    [Modified]  │
└─────────────────────────────────────────────────────────┘
```

#### 右侧面板布局（可拖动调整宽度）
```
┌─────────────────────────────────────┐
│ Commit History                      │
│ SVN revision logs                   │
├─────────────────────────────────────┤
│ A Added  M Modified                 │
│ D Deleted  R Replaced               │  ← 自动换行
├─────────────────────────────────────┤
│ [r1] zhangyutong      1 changes     │
│ 9 hours ago                         │
│ ...                                 │
└─────────────────────────────────────┘
```

### 📁 修改文件

```
Models/
├── SvnStatus.cs                    # 添加 IsSelected 属性，继承 ObservableObject

Views/
├── MainWindow.axaml                # 布局修复：5列布局、右侧 GridSplitter
                                    # 文件树添加 CheckBox
                                    # 右侧图例使用 WrapPanel

App.axaml                           # 添加 MaterialIconStyles 注册

Styles/
├── GlassStyles.axaml               # 添加 Material Icon 默认样式
```

### 🔧 技术细节

#### SvnStatus 模型更新
```csharp
public partial class SvnStatus : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected = true;  // 默认选中

    // ... 其他属性
}
```

#### 布局修复
```xml
<!-- 5列布局 -->
<Grid ColumnDefinitions="280,8,*,8,300">
    <Border Grid.Column="0">...</Border>           <!-- 左侧面板 -->
    <GridSplitter Grid.Column="1"/>                <!-- 左侧分隔 -->
    <Grid Grid.Column="2">...</Grid>               <!-- 中间文件树 -->
    <GridSplitter Grid.Column="3"/>                <!-- 右侧分隔 -->
    <Border Grid.Column="4">...</Border>           <!-- 右侧面板 -->
</Grid>
```

---

## 2025-01-10 - UI 优化与设置功能完善 v4

### ✅ 新增功能

#### 1. Material Icons 矢量图标
- ✅ 使用 Material.Icons.Avalonia 替代 Emoji 图标
- ✅ 所有菜单项使用矢量图标
- ✅ 工具栏按钮使用矢量图标
- ✅ 右键菜单使用矢量图标
- ✅ StartPage 欢迎页面使用矢量图标
- ✅ 设置页面使用矢量图标

#### 2. 设置页面完善
- ✅ 主题切换功能（浅色/深色/跟随系统）
- ✅ 主题设置持久化保存
- ✅ 应用启动时自动加载保存的主题
- ✅ SVN 可执行文件路径配置
- ✅ SVN 路径浏览选择功能
- ✅ 默认仓库 URL 配置
- ✅ 设置自动保存（无需点击保存按钮）
- ✅ 选中主题按钮高亮显示

#### 3. 应用设置服务增强
- ✅ 支持主题设置保存和加载
- ✅ 支持 SVN 路径设置
- ✅ 支持默认仓库 URL 设置
- ✅ 支持最近项目列表（最多 10 个）

### 🎨 UI 改进

#### Material Icons 图标映射
| 功能 | 图标 Kind | 描述 |
|-----|-----------|------|
| 更新 | Download | SVN 更新 |
| 提交 | Upload | SVN 提交 |
| 刷新 | Refresh | 刷新文件列表 |
| 差异 | FileCompare | 比较文件 |
| 历史 | History | 查看历史 |
| 设置 | Cog | 打开设置 |
| 分支 | SourceBranch | 分支管理 |
| 切换 | SwapHorizontal | 切换分支 |
| 合并 | SourceMerge | 合并分支 |
| 浏览 | FolderOpen | 浏览文件夹 |
| 添加 | Plus | 添加文件 |
| 删除 | Delete | 删除文件 |
| 还原 | Undo | 还原修改 |
| 主页 | Home | 返回欢迎页 |

#### 设置页面布局
```
┌─────────────────────────────────────────────────────────────┐
│ ⚙ Settings                                                   │
│ Configure your SVN client preferences                       │
├─────────────────────────────────────────────────────────────┤
│ 🎨 Appearance                                                │
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│ │ ☀ Light    │ │ 🌙 Dark    │ │ 🖥 System   │            │
│ └─────────────┘ └─────────────┘ └─────────────┘            │
├─────────────────────────────────────────────────────────────┤
│ 🔀 SVN Configuration                                         │
│ SVN Executable Path: [svn                    ] [Browse...] │
│ Default Repository URL: [https://...]                       │
├─────────────────────────────────────────────────────────────┤
│ ℹ About Svns                                                │
│ Version: 1.0.0                                              │
│ Framework: Avalonia UI 11.3 / .NET 9                       │
│ License: MIT                                                │
└─────────────────────────────────────────────────────────────┘
```

### 📁 修改文件

```
Services/
├── AppSettingsService.cs           # 增强：主题/SVN路径/最近项目支持

ViewModels/
├── SettingsViewModel.cs            # 重写：完整的设置管理功能

Views/
├── MainWindow.axaml                # 修改：Material Icons 替代 Emoji
├── StartPageWindow.axaml           # 修改：Material Icons 替代 Emoji
├── SettingsWindow.axaml            # 重写：可编辑的设置界面
└── SettingsWindow.axaml.cs         # 重写：主题切换/SVN路径浏览

App.axaml.cs                        # 修改：启动时加载保存的主题
```

### 🔧 技术细节

#### 设置文件结构
```json
{
  "LastWorkingCopy": "C:\\path\\to\\working\\copy",
  "LastOpened": "2025-01-10T12:00:00",
  "Theme": "Dark",
  "SvnPath": "C:\\Program Files\\TortoiseSVN\\bin\\svn.exe",
  "DefaultRepositoryUrl": "https://svn.example.com/repos",
  "RecentProjects": [
    { "Name": "MyProject", "Path": "C:\\path", "LastOpened": "..." }
  ],
  "Window": {
    "Width": 1200,
    "Height": 700
  }
}
```

#### 主题切换
```csharp
// App.xaml.cs - 启动时加载主题
var theme = await _settingsService.GetThemeAsync();
Current.RequestedThemeVariant = theme switch
{
    "Light" => ThemeVariant.Light,
    "Dark" => ThemeVariant.Dark,
    _ => ThemeVariant.Default
};
```

---

## 2025-01-10 - UI 优化与功能增强 v3

### ✅ 新增功能

#### 1. 应用设置持久化
- ✅ 创建 `Services/AppSettingsService.cs` - 应用级设置服务
- ✅ 设置保存到 `%LocalAppData%\Svns\svns-settings.json`
- ✅ 支持记住上次打开的工作副本路径
- ✅ 应用启动时自动打开上次的工作副本
- ✅ 如果上次工作副本不存在，显示欢迎页面

#### 2. SVN 日志缓存
- ✅ 创建 `Services/SvnLogCacheService.cs` - 日志缓存服务
- ✅ 缓存保存到工作副本下的 `.svns/svnlog-cache.json`
- ✅ `.svns` 目录通过 `.svnignore` 排除 SVN 版本控制
- ✅ 智能合并：只拉取新版本号，追加到已有缓存
- ✅ 加载时先显示缓存，后台更新最新日志

#### 3. 文件类型图标（Emoji）
- ✅ 使用 Emoji 图标替代 IconPacks（更可靠、无需外部依赖）
- ✅ 创建 `Converters/FileTypeToIconConverter.cs` - 文件类型图标转换器
- ✅ 支持 50+ 文件类型的 Emoji 图标：
  - **编程语言**: Java (☕), C#, Python, JavaScript, TypeScript, Go, Rust, C/C++, PHP, Ruby, Swift, Kotlin
  - **Web技术**: HTML, CSS, SCSS, Sass
  - **配置文件**: XML, YAML, JSON, TOML, INI
  - **文档**: Markdown, TXT, PDF
  - **数据库**: SQL, SQLite
  - **图片**: PNG, JPG, SVG, ICO
  - **压缩包**: ZIP, TAR, GZ, RAR, 7Z
  - **Office**: Word, Excel, PowerPoint
  - **其他**: Docker, Git, Node.js, Shell 脚本

#### 2. 文件树视图优化
- ✅ 将文件列表改为 TreeView 层级结构
- ✅ 在 `SvnStatus` 模型中添加 `Children` 属性
- ✅ 在 `WorkingCopyService` 中添加 `GetStatusTreeAsync()` 方法
- ✅ 文件树显示：
  - 文件类型图标（矢量图标）
  - SVN 状态图标（✓ ? M A D）
  - 文件/文件夹名称
  - 状态徽章（仅显示有意义的修改）

#### 3. SVN 提交历史面板
- ✅ 创建 `Models/SvnLogEntry.cs` - SVN 日志条目模型
- ✅ 创建 `Models/SvnChangedPath.cs` - 变更路径模型
- ✅ 创建 `Services/Svn/Parsers/SvnLogParser.cs` - 日志解析器
- ✅ 在 `WorkingCopyService` 添加 `GetLogAsync()` 方法
- ✅ 右侧面板显示提交历史：
  - 版本号徽章
  - 作者
  - 相对时间
  - 提交消息
  - 变更文件列表（带 A/M/D/R 标识）

### 🎨 UI 改进

#### 主窗口布局
```
┌─────────────────────────────────────────────────────────────┐
│ Menu Bar                                                      │
├──────────────┬────────────────────────┬──────────────────────┤
│ Working Copy │ File Tree              │ Commit History       │
│ Info         │                        │                      │
│              │ 📄 ✓ pom.xml           │ r123 user 2h ago    │
│ Branch: trunk│ 📁 docs/               │ M Updated README     │
│ Revision: 456│   📄 README.md         │ A 2 files            │
│              │ 📁 src/                │                      │
│ Changes:     │   ☕ Main.java         │ r122 user 1d ago     │
│ ✎ 5 Modified  │   📘 Program.cs       │ D Old file           │
│ + 3 Added     │                        │                      │
│ − 1 Deleted   │                        │ ...                 │
└──────────────┴────────────────────────┴──────────────────────┘
```

#### 图标映射表
| 文件类型 | 图标 | 描述 |
|---------|------|------|
| 文件夹 | 📁 | Folder |
| Java | ☕ | Coffee |
| C# | 📘 | FileCode |
| Python | 🐍 | FileCode |
| JavaScript | 📜 | Script |
| HTML | 🌐 | Web |
| CSS | 🎨 | Palette |
| XML | 📄 | FileCode |
| Markdown | 📝 | TextBox |
| 图片 | 🖼️ | Image |
| 压缩包 | 🗜️ | ZipBox |
| PDF | 📕 | FilePdfBox |
| 数据库 | 🗃️ | Database |
| 默认 | 📄 | File |

### 📁 新增/修改文件

```
Converters/
├── FileTypeToIconConverter.cs      # 文件类型图标转换器（Emoji 版本）
├── SvnPathActionConverters.cs      # SVN 路径动作转换器

Models/
├── SvnLogEntry.cs                  # SVN 日志条目
├── SvnChangedPath.cs               # 变更路径
└── SvnStatus.cs                    # 添加 Children/Name/IsFile 属性

Services/
├── AppSettingsService.cs           # 应用设置服务（新增）
└── SvnLogCacheService.cs           # SVN 日志缓存服务（新增）

Services/Svn/
├── Operations/
│   └── WorkingCopyService.cs       # 添加 GetStatusTreeAsync/GetLogAsync
└── Parsers/
    └── SvnLogParser.cs             # 日志解析器

Views/
└── MainWindow.axaml               # TreeView 文件树 + 提交历史面板

ViewModels/
└── MainWindowViewModel.cs         # 添加 LoadLogsAsync/LogEntries

App.axaml.cs                        # 启动时自动打开上次工作副本
```

### 🔧 技术细节

#### Emoji 图标方案
```xml
<!-- 无需外部依赖，直接使用 Emoji -->
<TextBlock Text="{Binding Path, Converter={StaticResource FileTypeToIconConverter}}"
           FontSize="14" />
```

#### 应用设置
```csharp
// 设置保存路径
%LocalAppData%\Svns\svns-settings.json

// 设置内容
{
  "LastWorkingCopy": "C:\\path\\to\\working\\copy",
  "LastOpened": "2025-01-10T12:00:00",
  "Window": {
    "Width": 1200,
    "Height": 700,
    "X": 100,
    "Y": 50,
    "IsMaximized": false
  }
}
```

#### SVN 日志解析与缓存
```csharp
// 获取最近 50 条提交记录（带缓存）
var cachedLogs = await _logCacheService.LoadCachedLogsAsync(workingCopyPath);
var newLogs = await _workingCopyService.GetLogAsync(workingCopyPath, limit: 50);
var mergedLogs = _logCacheService.MergeLogs(cachedLogs, newLogs);

// SVN 命令
svn log -l 50 -v --xml

// 缓存文件路径
<working-copy>/.svns/svnlog-cache.json

// 返回 SvnLogEntry 列表
// - Revision: 版本号
// - Author: 作者
// - Date: 提交时间
// - Message: 提交消息
// - ChangedPaths: 变更文件列表
```

### 🚧 待完成功能

#### 短期
- [ ] Checkout 对话框实现
- [ ] Commit 对话框
- [ ] Diff 查看器
- [ ] 深色主题配色资源

#### 中期
- [ ] 右键菜单
- [ ] 搜索和筛选功能
- [ ] 日志分页加载
- [ ] 窗口位置和大小记忆

---

**最后更新**: 2026-01-11
**版本**: 1.0.0-beta
**状态**: ✅ 编译成功，Bug修复和消息通知中心完成 (提交刷新修复 / 通知中心新增 / 29/29 SVN 命令实现)
