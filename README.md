# 2D Weapons System - Unity

The **2D Weapons System** is a highly modular and extensible framework for managing weapons in Unity-based 2D games.
This system is designed to provide scalable and efficient weapon handling, supporting projectiles, explosions, environmental interactions, and event-driven mechanics.

## Features

- **Fully Modular Architecture** - Easily add and modify weapons, projectiles, and damage types.
- **ScriptableObject-Based Weapon System** - Define weapon attributes (damage, fire rate, magazine size, effects) in the Unity Editor.
- **Projectile System** - Supports gravity, acceleration, bouncing, and in-flight behavior changes.
- **Explosion & Area Damage** - Handles shockwaves, destruction, and environmental effects.
- **Ammo & Reload System** - Implements fire rate control, magazine size, and reload mechanics.
- **Environmental Interactions** - Dig into terrain, create new obstacles, or modify landscapes dynamically.
- **Event-Driven Architecture** - Uses UnityEvents to ensure decoupled and scalable interactions.
- **Optimized for Performance** - Implements object pooling and coroutine-based logic to enhance efficiency.

## System Overview

The system is structured to be highly **scalable and flexible**, utilizing **Object-Oriented Programming (OOP)**, **event-driven design**, and **data-driven configurations** to enable seamless customization. The key components include:

- **WeaponBase** - The central weapon controller that integrates all weapon mechanics.
- **WeaponSO (ScriptableObject)** - Stores weapon properties such as damage, projectile type, and explosion effects.
- **ProjectileManager** - Handles projectile behavior, including movement, collisions, and physics-based effects.
- **ExplosionManager** - Manages explosions, area damage, and environmental destruction.
- **AmmoManager** - Controls shooting logic, reloads, and cooldown management.
- **EnvironmentalManager** - Handles terrain deformation and object interactions.
- **GravityManager** - Applies gravity effects on projectiles and explosions.
- **Event System** - Uses UnityEvents to allow modular expansion and interaction with other game components.

## Getting Started

### Installation
1. Clone this repository into your Unity project.
2. Ensure your project is using **Unity 2021 or later**.
3. Assign weapons through the **WeaponSO** assets in the Unity Editor.

### Basic Usage
- Attach `WeaponBase` to any entity that can fire weapons.
- Assign a `WeaponSO` to configure the weapon properties.
- Call `FireWeapon()` in `WeaponBase` to shoot projectiles.
- Listen for UnityEvents to trigger additional effects when weapons are used.

## Extending the System

This system is built for **customization and scalability**. You can extend it by:
- **Adding new projectile types** - Inherit from `Projectile` and override collision behavior.
- **Creating unique explosion effects** - Modify `ExplosionManager` to implement custom area-of-effect damage.
- **Integrating new environmental interactions** - Extend `EnvironmentalManager` to handle different terrain types.
- **Using UnityEvents to trigger game-wide effects** - Connect events for sound, animations, or UI updates.

## About the Developer

This project was created as part of an advanced Unity development initiative to recreate the game Liero.
