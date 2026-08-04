using System;

namespace Dack;

public enum DackSimulationState
{
    Running,
    Frozen,
    Stopped
}

public enum DackAuthoringMode
{
    Play,
    Build,
    Understand
}

public enum DackOwnedSurface
{
    Canvas,
    Cockpit,
    SpriteStudio,
    Modal
}

public enum DackSafetyState
{
    Normal,
    Boss
}

/// <summary>
/// Shared shell state for input ownership and safe mode transitions.
/// Detailed simulation and level state remain in their owning systems.
/// </summary>
public sealed class DackUiState
{
    public DackSimulationState Simulation { get; private set; } = DackSimulationState.Frozen;
    public DackAuthoringMode Authoring { get; private set; } = DackAuthoringMode.Build;
    public DackOwnedSurface Surface { get; private set; } = DackOwnedSurface.Canvas;
    public DackSafetyState Safety { get; private set; } = DackSafetyState.Normal;

    public event Action? Changed;

    public void Set(
        DackSimulationState simulation,
        DackAuthoringMode authoring,
        DackOwnedSurface surface,
        DackSafetyState safety)
    {
        if (Simulation == simulation
            && Authoring == authoring
            && Surface == surface
            && Safety == safety)
        {
            return;
        }

        Simulation = simulation;
        Authoring = authoring;
        Surface = surface;
        Safety = safety;
        Changed?.Invoke();
    }
}
