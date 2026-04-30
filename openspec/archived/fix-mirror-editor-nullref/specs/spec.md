# Spec: Mirror Editor NullReferenceException

## Scenario: Inspecting a GameObject with Mirror components
- **Given** the Unity Editor is open
- **When** I select a GameObject that has a `NetworkIdentity` or `NetworkBehaviour`
- **And** the `NetworkInformationPreview` is active
- **Then** no `NullReferenceException` should be thrown from `Mirror.NetworkInformationPreview+Styles..ctor`
