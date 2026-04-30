# Design: Weight-based Spam Calculation

## Formula
To ensure difficulty scales well from very small (0.1kg) to very large (1000kg) fish, we use a square root formula:
`spamClicks = 5 + sqrt(weight * 10) + rankBonus`

- **Base**: 5 clicks.
- **Scaling**: A 10kg fish adds ~10 clicks. A 100kg fish adds ~31 clicks.
- **Rank Bonus**: Star rank length * 2 (adds 2-10 extra clicks).
- **Clamp**: Min 5, Max 100.

## Implementation
- **Model**: Add `float weight` to `FishDataSO`.
- **Editor**: In `ExcelDataConverter`, use `(size^2 / 500) * rankMultiplier` to generate default weights.
- **Logic**: Update `FishingPlayer.GetRequiredSpam(FishDataSO fish)`.
