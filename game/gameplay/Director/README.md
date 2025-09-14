# Director (Scoring & Selection)

Implements the scoring and selection logic described in `requirements.md`:

- Score = 0.45*danger + 0.25*pedagogy + 0.15*novelty + 0.10*diversity + 0.05*contract
- `solvableNow` gates via multiplier (1.0 if true, 0.4 otherwise)
- Exceeding damage budget applies penalty (default 0.3x)
- Diversity boost prefers different `EventType`
- ε-greedy novelty injection occasionally swaps in an unseen event
- Reserved slots: `contract_critical: 1` for Tier >= 4 (pass via `IWorldState.GetReservedSlots`)

`IWorldState`/`IEventCandidate` are small interfaces that the game layer should implement.
