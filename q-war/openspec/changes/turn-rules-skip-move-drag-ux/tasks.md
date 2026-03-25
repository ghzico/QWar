## 1. 回合规则：每棋将 2 次行动

- [x] 1.1 在 FightBoard 中新增「当前回合每棋将剩余行动次数」的存储（如 Dictionary<ChessHero,int> 或等价结构），并在我方回合开始时初始化为在场存活棋将各 2 次
- [x] 1.2 将 ConsumePlayerAction 改为「扣除当前操作棋将的剩余次数 1；若该棋将归零则从轮次中移除；若全场无剩余次数则调用 RunMonsterTurnAsync」
- [x] 1.3 调整「可弹出行动弹窗」的条件：仅当该棋将本回合剩余行动次数 > 0 时才允许 ShowActionPopup；点击无剩余次数的棋将时不弹窗
- [x] 1.4 在 RunMonsterTurnAsync 结束后（我方回合再次开始前）重新收集在场存活棋将并为其各赋予 2 次行动，保证下一回合计数正确

## 2. 放弃移动

- [x] 2.1 在 ChessHero 的 ActionPopup（VBox）中新增「放弃移动」按钮，并在 _Ready 中绑定 Pressed 回调
- [x] 2.2 在 FightBoard 中新增 EndCurrentHeroTurn(ChessHero hero)：将该 hero 的本回合剩余次数置 0 并移出轮次；若我方无任何剩余行动则调用 RunMonsterTurnAsync
- [x] 2.3 在「放弃移动」回调中关闭 ActionPopup 并调用 FightBoard.EndCurrentHeroTurn(this)

## 3. 布阵拖拽：随鼠标移动的预览

- [x] 3.1 在 DeckHeroCard 的 _GetDragData 中通过 SetDragPreview 设置自定义预览节点（如带立绘的 Control/TextureRect），或在不支持时创建跟随鼠标的预览节点并在 _Process 中同步 GetViewport().GetMousePosition()
- [x] 3.2 确保预览节点尺寸与视觉风格与棋将卡片/立绘一致，拖拽过程中预览随鼠标平滑移动、无跳跃

## 4. 布阵阶段棋盘内重排（已放置棋将拖拽）

- [x] 4.1 为布阵阶段已放置在棋盘上的 ChessHero 支持拖拽：在 FightBoard 或 ChessHero 上实现 _GetDragData（仅当 IsDeploymentPhase 时返回可区分的 data，如棋将实例或 (row,col)+hero 标识），使 BoardDropZone 能接收「来自棋盘的棋将」
- [x] 4.2 在 BoardDropZone/FightBoard 的 CanDropDataAt 中区分两类 data：来自 Deck 的 ConfigId（保持现有逻辑）；来自棋盘的已放置棋将——仅当目标格为 col 0 或 1、且目标格为空或是该棋将当前所在格时允许放置
- [x] 4.3 在 DropDataAt 中处理「来自棋盘的棋将」：从原格 RemoveUnitAt，再 PlaceUnit 到新格；不调用 DeckPanel.MarkDeployed/Unmark，保持已上场状态
- [x] 4.4 确保非布阵阶段棋盘上的棋将不触发拖拽（不返回拖拽 data 或不在布阵阶段启用拖拽），避免与战斗中的点击选棋将冲突

## 5. 布阵阶段：将棋将拖入背包使其下场

- [x] 5.1 为 ChessHero 增加 ConfigId 属性（或等效方式），在从棋将包放置上场时写入，供拖入背包时调用 UnmarkDeployed(configId)
- [x] 5.2 在 DeckPanel 或其内部可接收拖放的区域实现 _CanDropData/_DropData：当 data 为「来自棋盘的已放置棋将」且处于布阵阶段时接受放置
- [x] 5.3 在 DeckPanel 的 Drop 处理中：从棋盘 RemoveUnitAt、根据棋将取得 ConfigId 并调用 UnmarkDeployed(configId)、对棋将节点 QueueFree；在 DeckPanel 中新增 UnmarkDeployed(configId) 方法，从已上场列表移除并刷新卡片显示

## 6. 验收与边界

- [ ] 6.1 验证我方回合多棋将依次行动、每棋将最多 2 次，全部用尽或放弃后进入怪物回合
- [ ] 6.2 验证「放弃移动」后该棋将本回合不可再操作，且若还有其他棋将有次数则可继续选其他棋将
- [ ] 6.3 验证从棋将包拖拽时预览跟随鼠标、上场后可在第 0/1 列内拖拽调整位置且不取消已上场状态
- [ ] 6.4 验证布阵阶段将棋盘上的棋将拖入背包后，该棋将从棋盘移除、对应卡片在背包中重新显示并可再次拖出上场
