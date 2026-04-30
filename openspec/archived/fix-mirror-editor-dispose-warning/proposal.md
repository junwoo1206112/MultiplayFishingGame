# Proposal: Fix Mirror Editor Disposal Warning

## Problem
The Unity Editor logs a warning: `Mirror.NetworkInformationPreview was not disposed properly. Make sure that base.Cleanup is called...`. This occurs because `NetworkInformationPreview` (inheriting from `ObjectPreview`) does not explicitly handle its cleanup, leading to the garbage collector finalizer catching unreleased resources.

## Solution
Implement lifecycle management in `Assets/Mirror/Editor/NetworkInformationPreview.cs`:
1. Override the `Cleanup()` method and call `base.Cleanup()`.
2. Although `ObjectPreview` doesn't typically have `OnDisable`, adding a check or ensuring that any resources used (like `Styles`) are cleared can help.
3. Most importantly, ensure the `ObjectPreview` state is correctly finalized.

## Impact
- **Fixes**: "Not disposed properly" warnings in the console.
- **Scope**: Mirror editor script.

## Constraints
- Stay within Mirror's namespace.
- Follow OpenSpec-driven development.
