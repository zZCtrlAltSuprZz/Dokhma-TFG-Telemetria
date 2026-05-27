using System;
using UnityEngine;

public interface IRitualEnemy
{
    event Action<IRitualEnemy> OnRitualEnemyDied;
    Transform Transform { get; }
}