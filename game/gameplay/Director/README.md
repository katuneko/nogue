# Director (Scoring & Selection)

Implements the scoring and selection logic described in `requirements.md`:

- Score = 0.45*danger + 0.25*pedagogy + 0.15*novelty + 0.10*diversity + 0.05*contract
- `solvableNow` gates via multiplier (1.0 if true, 0.4 otherwise)
- Exceeding damage budget applies penalty (default 0.3x)
- Diversity boost prefers different `EventType`
- ε-greedy novelty injection occasionally swaps in an unseen event
- Reserved slots: `contract_critical: 1` for Tier >= 4 (pass via `IWorldState.GetReservedSlots`)

`IWorldState`/`IEventCandidate` are small interfaces that the game layer should implement.

Config loading (externalized knobs):

```csharp
using Nogue.Core;
using Nogue.Gameplay.Director;
using Nogue.Gameplay.World;

// 1) Load defaults
var cfg = DirectorConfigLoader.LoadOrDefault(ContentPaths.DirectorScoringPath());
var director = Director.FromConfig(cfg);

// 2) Load tier overrides (Tier N)
int tier = 4; // example
var ov = TierConfigLoader.LoadDirectorForTier(ContentPaths.TiersJsonPath(), tier);
int k = ov.K ?? cfg.K;
double eps = ov.Epsilon ?? cfg.Epsilon;
int reserved = System.Math.Max(cfg.ReservedSlots.ContractCritical, ov.ReservedContract ?? 0);

// 3) Build world
var world = new WorldState(tier: tier, k: k, apRemaining: 10, epsilon: eps, reservedContract: reserved);
```
```
