# Design: Resource Cleanup for Mirror Editor Preview

## Approach
We will add an override for the `Cleanup` method to ensure the base class cleanup is performed. Additionally, we will null out the `styles` and `title` references to assist the garbage collector.

### Proposed Code Change
```csharp
public override void Cleanup()
{
    base.Cleanup();
    styles = null;
    title = null;
}
```

By overriding `Cleanup` and calling the base implementation, we satisfy the requirement mentioned in the Unity warning.

## Decisions
1. **Override Cleanup**: Explicitly override `Cleanup()` to call `base.Cleanup()`.
2. **Nullify References**: Clear internal state during cleanup.

## Code Changes
- **File**: `Assets/Mirror/Editor/NetworkInformationPreview.cs`
- **Class**: `NetworkInformationPreview`
- **Change**: Add `public override void Cleanup()` method.
