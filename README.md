# 2D Roguelike Game

A Unity-based 2D roguelike game featuring progressive difficulty scaling, intelligent AI enemies, and dynamic camera systems. Built with Unity 6.

## 🎮 Game Features

### Core Gameplay
- **Grid-based movement** with smooth animations
- **Turn-based combat** system
- **Progressive difficulty scaling** across 5 distinct phases
- **Food collection** and resource management
- **Destructible walls** and environmental interaction
- **Trap systems** with audio feedback
- **Enemy AI** with Manhattan distance pathfinding

### Dynamic Systems
- **Adaptive camera system** using Unity 6 Cinemachine 3.x
- **Data-driven level configuration** for scalable difficulty
- **Intelligent enemy behavior** with predictable pathfinding
- **Procedural board generation** with balanced resource distribution

## 🏗️ Technical Architecture

### Design Patterns
- **Data-Driven Configuration**: LevelConfig system eliminates hardcoded values
- **Component-Based Architecture**: Modular CellObject inheritance hierarchy
- **Event-Driven Turn System**: Clean separation of game logic and presentation

### Progressive Difficulty System
The game features a 5-phase progression system:

| Phase | Levels | Board Size | Enemies | Camera Lens | Focus |
|-------|--------|------------|---------|-------------|-------|
| Learning | 1-2 | 8x8 | 0 | 3.0f | Movement basics |
| Skill Building | 3-5 | 10x10 | 1 (fixed) | 3.0f | Combat introduction |
| Challenge | 6-8 | 12x12 | 2-4 | 4.0f | Tactical gameplay |
| Mastery | 9-12 | 14x14 | 3-6 | 5.0f | Advanced strategies |
| Endgame | 13+ | 16x16 | 5-9 | 6.0f | Maximum difficulty |

### Camera System
- **Unity 6 Cinemachine 3.x** integration
- **Progressive lens scaling** (3.0f → 6.0f)

## 🤖 AI System

### Enemy Pathfinding

Enemies use **Manhattan distance pathfinding** with intelligent movement prioritization.

**Benefits:**
- **O(1) performance** per enemy per turn
- **Predictable behavior** for strategic gameplay
- **Fallback movement** when primary direction is blocked
- **Adjacent attack detection** for combat engagement

### Setup
1. Clone the repository
2. Open project in Unity 6
3. Ensure Cinemachine and Input System packages are installed
4. Assign CinemachineCameraController reference in BoardManager

### Controls
- **Arrow Keys**: Movement
- **Space**: Skip turn
- **Enter**: Restart game (when game over)

## 🔧 Customization

### Adding New Difficulty Phases
Extend the `levelConfigs` array in BoardManager with new LevelConfig entries.

### Modifying Enemy Behavior
Adjust pathfinding logic in `EnemyObject.TurnHappened()` method.

### Camera Progression Tuning
Update `CameraLensSize` values in LevelConfig entries.

### Resource Scaling
Modify `FoodDivisor` and `WallDivisor` parameters for different resource densities.

## 🧮 Mathematical Divisor System

The game uses an elegant **mathematical divisor system** for automatic resource scaling that ensures balanced gameplay across all board sizes.

### Core Formula
```csharp
resourceAmount = boardArea / divisor
```

Where:
- **boardArea** = `(Width - 2) * (Height - 2)` (interior cells only)
- **divisor** = scaling factor controlling resource density

### How It Works

#### Food Generation Example
```csharp
// Level 1-2: 8x8 board, FoodDivisor=8, MinFood=4
boardArea = (8-2) * (8-2) = 36
scaledFood = 36 / 8 = 4.5 → 4
finalFood = Max(4, 4) = 4
Result: 4-5 food items

// Level 6-8: 12x12 board, FoodDivisor=16, MinFood=2  
boardArea = (12-2) * (12-2) = 100
scaledFood = 100 / 16 = 6.25 → 6
finalFood = Max(6, 2) = 6
Result: 6-7 food items
```

### Divisor Impact on 12x12 Board (100 cells)

| Divisor | Formula | Base Amount | Density | Description |
|---------|---------|-------------|---------|-------------|
| 8 | `100/8 = 12.5` | **12** | 12% | Very generous |
| 12 | `100/12 = 8.3` | **8** | 8% | Balanced |
| 16 | `100/16 = 6.25` | **6** | 6% | Moderate |
| 20 | `100/20 = 5` | **5** | 5% | Sparse |

### System Benefits

1. **Automatic Scaling**: Larger boards get proportionally more resources
2. **Density Control**: Higher divisor = fewer resources per area = harder difficulty
3. **Consistent Balance**: Same divisor creates similar gameplay feel regardless of board size
4. **Designer-Friendly**: Intuitive tuning - one number controls entire resource category
5. **Fail-Safe**: MinFood/MinWalls prevent edge cases on small boards

This mathematical approach eliminates guesswork and ensures every level feels appropriately challenging while maintaining consistent resource distribution patterns.

---

Built with ❤️ using Unity 6.