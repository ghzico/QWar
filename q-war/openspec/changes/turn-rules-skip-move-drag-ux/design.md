## Context

- **当前回合**：FightBoard 使用 `_actionsRemainingThisTurn`（全局 2 次）表示我方本回合剩余行为次数；任意棋将执行一次移动/攻击/技能后调用 `ConsumePlayerAction()` 减 1，归零后进入怪物回合。无「按棋将」维度的行动次数，也无「放弃移动」入口。
- **当前布阵拖拽**：DeckHeroCard 通过 `_GetDragData` 返回 ConfigId，由 Godot 默认拖拽逻辑处理；无自定义预览，拖拽视觉易跳跃。BoardDropZone 仅处理「从棋将包拖入」的 data（ConfigId），不支持「从棋盘格拖出已放置棋将」在同一棋盘内重排。
- **约束**：保持与现有 ChessHero/ChessEnemy、PlaceUnit/RemoveUnitAt、DeckPanel 的兼容；不改变怪物回合逻辑（仍为每怪 2 次行为）。

## Goals / Non-Goals

**Goals:**

- 实现「我方每棋将 2 次行动、我方全部结束后敌方回合」的回合模型，并支持在行动菜单中「放弃移动」结束当前棋将本回合。
- 布阵阶段：拖拽时棋将视觉随鼠标平滑移动、抓取感清晰；上场后可在己方列内通过拖拽调整位置；支持将棋将拖入背包使其下场。

**Non-Goals:**

- 不改变敌方回合的 AI 与行为次数；不引入网络/存档的回合序列化；不做战斗外的「布阵存档」或预设阵型。

## Decisions

1. **行动次数数据结构（每棋将 2 次）**  
   - 在 FightBoard 内维护「当前回合每棋将剩余行动次数」：可用 `Dictionary<ChessHero, int>` 或「回合开始时从当前在场棋将快照 + 每棋将 2」构建列表/字典」。  
   - 回合开始（我方阶段开始）时：收集当前存活且在场所有 ChessHero，为每个赋予 2 次行动；当前行动者可为「任意还有次数的棋将」（玩家点击谁就谁弹窗），不强制顺序。  
   - **备选**：按固定顺序（如按 GridRow/GridCol 排序）依次只能选「当前棋将」——未采用，以保留「自由选棋将」的体验。

2. **ConsumePlayerAction 语义变更**  
   - 由「全局减 1，归零则怪物回合」改为「当前操作棋将剩余次数减 1；若该棋将归零则从轮次表移除；若全场无剩余次数则进入怪物回合」。  
   - 新增「结束当前棋将本回合」（放弃移动）：将该棋将剩余次数置 0 并移出轮次，不执行任何移动/攻击；若仍有其他棋将有次数则不变更当前焦点，若无则进入怪物回合。

3. **放弃移动的 UI 与调用链**  
   - 在 ActionPopup 的 VBox 中增加按钮「放弃移动」。点击后关闭弹窗，调用 FightBoard 的新方法（如 `EndCurrentHeroTurn(ChessHero hero)`），内部：将该 hero 的剩余次数置 0、若我方无剩余行动则 `RunMonsterTurnAsync()`。

4. **布阵拖拽：随鼠标移动的预览**  
   - Godot 的 Control 拖拽可通过 `SetDragPreview(Control preview)` 提供自定义预览；预览在拖拽期间由引擎移动。若默认仍跳跃，可改为：在拖拽开始时创建一个跟随鼠标的 Control（或复用一张立绘 TextureRect），在 `_Process` 或 `_Input` 中每帧将预览位置设为 `GetViewport().GetMousePosition()` 相对目标父节点的坐标，实现「棋将随鼠标」的平滑感。  
   - **备选**：仅用 SetDragPreview 且预览节点在 _Process 里跟鼠标——若引擎已每帧更新预览位置则不必；否则在预览节点 _Process 中同步鼠标位置。

5. **上场后棋盘内重排（仅布阵阶段）**  
   - 已放置在棋盘上的 ChessHero（在 UnitsContainer 内）在布阵阶段需要支持「拖出再放下」。实现方式：  
     - 为 ChessHero 或其父容器启用拖拽：在布阵阶段，当鼠标在己方棋将上按下并拖动时，以「拖拽数据」形式提供该棋将的引用或唯一标识（例如 ChessHero 实例或其 GridRow/GridCol + 标识）；BoardDropZone 的 `_CanDropData` / `_DropData` 需识别两类 data：一是来自 Deck 的 ConfigId（新建棋将并 PlaceUnit），二是来自棋盘的「已放置棋将」引用（从原格 RemoveUnitAt，再 PlaceUnit 到新格，不调用 DeckPanel.MarkDeployed/Unmark）。  
   - 合法目标格：仍为布阵阶段、col 0 或 1、目标格为空或为当前拖拽的同一棋将（即原格）；不允许拖到已有其他棋将的格。

6. **将棋将拖入背包使其下场**  
   - DeckPanel 需作为拖放目标：实现 `_CanDropData` / `_DropData`（或在其内部子 Control 上实现），当 data 为「来自棋盘的已放置棋将」且处于布阵阶段时，接受放置。  
   - 放置时：从棋盘 RemoveUnitAt(hero.GridRow, hero.GridCol)，取得该棋将的 ConfigId（需在 ChessHero 或拖拽 payload 中携带），调用 DeckPanel.UnmarkDeployed(configId)，然后对棋将节点 QueueFree。UnmarkDeployed 将 configId 从已上场列表移除并刷新卡片显示（使该 ID 的卡片重新可见、可再次拖出上场）。

## Risks / Trade-offs

- **每棋将 2 次的状态持久化**：若未来要做存档/读档，需保存「当前回合每棋将剩余次数」；本次不实现存档，仅内存状态。  
- **拖拽预览与输入**：自定义预览若在顶层显示，需注意 ZIndex 与点击层顺序，避免拖拽时误触棋盘点击。  
- **棋盘内拖拽与点击**：布阵阶段点击己方棋将若开始拖拽，则不再触发「选中并弹出操作」；需区分「点击」与「拖拽开始」，避免冲突（例如拖拽阈值或按下后移动一定像素再认定为拖拽）。

## Migration Plan

- 代码内变更，无数据迁移。部署：完成 FightBoard/ChessHero/DeckHeroCard/BoardDropZone 等修改后，进入战斗场景验证回合顺序与放弃移动、布阵拖拽与重排即可。回滚：还原上述文件的改动。

## Open Questions

- 无。若后续需要「按固定顺序只能当前棋将行动」，可在 FightBoard 增加「当前行动棋将」指针并在 UI 上仅对当前棋将高亮可点击。
