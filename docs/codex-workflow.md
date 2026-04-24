# Codex 工作流

这个仓库为 Codex 风格的 agent 采用轻量级工作流。目标是在保持日常工作快速推进的同时，尽量保留 superpowers 的优点。

## 1. 快速熟悉上下文

先从 `HollowKnight/` 里的实际游戏项目开始。

先只阅读最接近的上下文：

- `HollowKnight/README.md`
- `HollowKnight/Scripts/` 下相关的脚本目录
- 当场景绑定很重要时，再看 `HollowKnight/Scenes/` 下对应的 owning scene

除非请求本身带有架构性质，否则不要一上来做大范围仓库巡览。

## 2. 选择合适的流程重量

### 小

当请求局部且明确时使用。

- 把计划留在聊天里
- 只改最少的文件
- 只验证受影响区域

示例：

- 调整玩家移动参数
- 修复 signal 绑定
- 调整敌人伤害逻辑
- 更新 HUD 行为

### 中

当改动跨越多个玩法区域，或跨越 scene/script 边界时使用。

- 在 `docs/plans/YYYY-MM-DD-<topic>.md` 写一个简短计划
- 计划要务实，不要仪式化
- 实现完成后，把每个受影响区域各验证一次

示例：

- 添加一个新的玩家能力，涉及移动、战斗和 HUD
- 重做房间切换行为以及检查点

### 大

当工作有歧义、偏架构，或需要干净交接时使用。

- 先 brainstorm
- 写一份正式的实现计划
- 把执行拆成多个可验证的小任务

示例：

- 新的 combat subsystem
- save/load 架构变更
- 大范围 scene pipeline 重构

## 3. Lightweight Superpowers 映射

有选择地使用这些技能：

- `brainstorming` 用于功能设计和不清晰的请求
- `systematic-debugging` 用于 bug 和回归问题
- `verification-before-completion` 作为默认的收尾纪律
- `writing-plans` 仅用于中/大型工作
- `subagent-driven-development` 仅用于边界清晰、彼此独立的任务拆分

默认并不是“把所有技能都用一遍”。默认是“使用足够轻的流程，同时仍然保护质量”。

## 4. 仓库特定验证

### 代码变更

- 重新阅读被改动的脚本和直接调用点
- 检查 scene 契约，例如 node 名称、导出字段和 signal 签名

### 构建检查

- 如果存在本地 `.csproj`，就在 `HollowKnight/` 下运行一次针对性的 `dotnet build`
- 如果没有提交的 project file，就要说明构建验证必须通过 Godot/editor 生成的产物来完成

### 手动 Smoke Test

优先明确到具体 scene 或具体玩法切片：

- player/combat: `HollowKnight/Scenes/Test/CombatSandbox.tscn`
- movement: `HollowKnight/Scenes/Test/MovementSandbox.tscn`
- room/game flow: `HollowKnight/Scenes/Boot.tscn`、`HollowKnight/Scenes/GameRoot.tscn`，或受影响的房间场景
- UI/menu: `HollowKnight/Scenes/UI/MainMenu.tscn`、`HollowKnight/Scenes/UI/PauseMenu.tscn`、`HollowKnight/Scenes/UI/HUD.tscn`

## 5. 完成定义

只有在以下条件都满足时，变更才算完成：

- 范围保持聚焦
- 相关的 scene/script 契约已经检查
- 已完成验证，或者明确说明了真实限制
- 最终总结写清楚改了哪些文件，以及还剩哪些手动检查

## 6. 计划目录

只有当工作不再是小任务时，才使用 `docs/plans/`。计划应该帮助执行，而不是变成额外的仪式感。
