# AGENTS.md

这个仓库默认采用轻量级 superpowers 工作流，核心始终围绕 `HollowKnight/` 里的实际游戏项目展开。

## 项目概览

- 工作区根目录：仓库外壳和共享文档
- 实际游戏项目：`HollowKnight/`
- 引擎/运行时：Godot 4.6 .NET
- 主要语言：C#
- 次要语言：GDScript

## 工作位置

大多数源码改动都应尽量留在 `HollowKnight/` 内：

- `HollowKnight/Scripts/Core` 用于启动、场景流转、存档、输入、调试工具
- `HollowKnight/Scripts/Combat` 用于伤害、hitbox/hurtbox、生命值、无敌、击退
- `HollowKnight/Scripts/Player` 用于控制器、移动、战斗、能力、动画桥接
- `HollowKnight/Scripts/Entities` 用于敌人和共享实体逻辑
- `HollowKnight/Scripts/Level` 用于房间、切换、检查点、相机边界、门禁
- `HollowKnight/Scripts/UI` 用于 HUD、菜单、设置
- `HollowKnight/Scenes` 用于可游玩场景和场景组合
- `HollowKnight/Docs` 用于设计、资源或调研笔记

开始熟悉项目时，优先阅读 `HollowKnight/README.md`。

## 默认不要编辑

这些路径通常是生成文件、缓存，或环境本地文件：

- `HollowKnight/.godot/`
- `HollowKnight/.nuget/`
- `HollowKnight/.dotnet_home/`
- `HollowKnight/.appdata/`

如果 Godot 会在本地自动重新生成这些产物，也尽量不要手工编辑它们。

## 仓库特定编码建议

- 游戏玩法和系统工作优先用 C#，除非目标本来就写在 GDScript 里
- 尽量保持周围文件的风格，不要顺手重排无关代码
- 除非任务明确要改场景契约，否则要保留 node 名称、signal 名称、导出属性，以及 `GetNode()` 路径
- 变更范围先锁定在最近的玩法区域，不要在没人要求时顺手扩成清理重构
- 把 `project.godot` 当成最后落地的配置文件，只有任务确实需要项目级设置时才改它

## 轻量工作流

默认走轻量路径：

1. 编辑前先看最近相关的脚本、场景和 README
2. 小任务保持聊天里的短计划即可，尽量只改最少文件
3. 中大型任务如果跨多个玩法区域，就在 `docs/plans/YYYY-MM-DD-<topic>.md` 写一个简短计划
4. 先用成本最低、最相关的检查验证，再说完成
5. 收尾时说明使用了那些技能、改了哪些文件、验证了什么，以及还需要哪些手动 Godot 检查

更详细的说明见 `docs/codex-workflow.md`。

## Lightweight Superpowers 策略
仅在本机已安装Superpowers的时候才允许使用
每次都只用最小必要子集，不要默认上完整重流程：

- `brainstorming`：只用于全新机制、功能设计，或需求明显不清晰的请求
- `systematic-debugging`：用于排查 bug 和回归问题
- `verification-before-completion`：在说工作完成前，始终按这个精神做最后确认
- `writing-plans`：只在工作真正跨多个系统，或需要交接时使用
- `subagent-driven-development`：只在任务彼此独立、文件归属清晰时使用

对于简单的脚本微调，不要强行上完整 TDD 或大篇幅设计文档。

## 验证要求

尽量选择与变更匹配、成本最低的证明方式：

- 仅脚本逻辑改动：检查受影响的调用点，如果仓库里有本地 `.csproj`，就跑一次针对性的构建
- 场景挂载改动：同时确认 node 路径、signal、导出值，以及受影响的 `.tscn`
- Combat/player 改动：补一个简短的手动 smoke test 建议
- UI/menu 改动：补上 pause/menu/HUD 的 smoke test 说明

如果仓库里没有提交 `.csproj`，要明确说明，不要假装已经可以运行 `dotnet build`。

## 搜索与导航

能用快速文本/文件搜索工具时优先使用它们。若环境里没有 `rg`，就用 PowerShell 备用命令，例如：

- `Get-ChildItem -Recurse -File`
- `Select-String -Path ... -Pattern ...`

## 仓库本地技能引用

项目相关参考技能放在这里：

- `.codex/skills/lightweight-superpowers/SKILL.md`
- `.codex/skills/godot-change-checklist/SKILL.md`

如果平台没有自动加载仓库本地技能，就把它们当作本地参考文档阅读，并手动遵循对应工作流。
## skills Creat Rules
- 重复性的流程沉淀出来，使用Skill Creator 创建技能复用

## other rules
- 遇到测试、验证卡死问题超过3次或累计超过60s，及时询问用户