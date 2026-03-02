# Tasks: 怪物动画（Slime1/2/3，attack/death/hurt/idle/run/walk）

## 1. 资源与精灵表约定

- [x] 1.1 在文档或 README 中明确：资源位于 `Res/Enemy/Slime1/`、`Slime2/`、`Slime3/`，文件名约定为 `怪物名序号_动作.png`（如 Slime1_Attack.png、Slime1_Death.png、Slime1_Hurt.png、Slime1_Idle.png、Slime1_Run.png、Slime1_Walk.png）；精灵表网格为 Attack 4×10、Death 4×10、Hurt 4×5、Idle 4×6、Run 4×8、Walk 4×8，4 行分别为向玩家面/背对/左/右，本期仅使用第一行（向玩家面）

## 2. ChessEnemy 场景与动画组件

- [x] 2.1 在 ChessEnemy 场景中增加 AnimatedSprite2D（或等价节点），与现有 TextureRect 并存；无动画时隐藏动画节点、仅显示 TextureRect
- [x] 2.2 实现从 `Res/Enemy/SlimeN/` 按约定文件名（SlimeN_Attack.png、SlimeN_Death.png 等）加载六种动作精灵表并按网格切帧（仅使用第 0 行），失败时回退到仅立绘、不报错
- [x] 2.3 支持为怪物实例指定动画集（Slime1/Slime2/Slime3），如通过导出属性或放置时传入文件夹名；与 TestScene/现有敌将放置逻辑对接

## 3. 怪物 idle、walk、run

- [x] 3.1 怪物待机时循环播放 idle（第一行，4×6 帧）
- [x] 3.2 怪物移动时播放 walk（第一行，4×8）；在 FightBoard 怪物移动逻辑中触发，并用 Tween 或动画驱动位置，结束后完成网格更新；无资源时直接更新位置
- [x] 3.3 可选：如需区分走/跑，预留或接入 run（4×8）；本期可仅用 walk 表现移动

## 4. 怪物 attack、hurt、death

- [x] 4.1 怪物攻击棋将时播放 attack（第一行，4×10），在造成伤害前或后播放，播完再继续；无资源时不播，逻辑照常
- [x] 4.2 在 ChessEnemy.TakeDamage 中播放 hurt（第一行，4×5）；若本次导致死亡，播完 hurt 后进入死亡流程
- [x] 4.3 怪物死亡时播放 death（第一行，4×10），动画结束或超时后执行 RemoveUnitAt + QueueFree；无 death 时短延时或立即移除，设置最大等待避免卡住

## 5. 集成与测试

- [x] 5.1 确保所有怪物动画触发点（idle、walk、attack、hurt、death）在缺资源时回退到当前行为（仅逻辑、无动画），不破坏现有流程
- [x] 5.2 在测试场景中挂 Slime1、Slime2、Slime3 至少各一，验证 attack、death、hurt、idle、walk（及可选 run）的播放与回退
