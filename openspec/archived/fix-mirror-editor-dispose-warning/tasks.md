# Tasks: Fix Mirror Editor Disposal Warning

## 1. Implementation
- [x] 1.1 Add `public override void Cleanup()` to `Assets/Mirror/Editor/NetworkInformationPreview.cs`.
- [x] 1.2 Call `base.Cleanup()` inside the override.
- [x] 1.3 Nullify `styles` and `title` in the `Cleanup()` method.

## 2. Validation
- [x] 2.1 Verify that the "was not disposed properly" warning no longer appears when closing inspectors or selecting different objects.
