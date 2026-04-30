# Spec: Mirror Editor Disposal Warning

## Scenario: Object Selection and Inspector Refresh
- **Given** a GameObject with Mirror components is selected in the Inspector
- **When** the selection changes to another object or the Inspector is closed
- **Then** the `NetworkInformationPreview` should be cleaned up properly
- **And** no disposal warning should appear in the Console
