namespace Nogue.Gameplay.World
{
    public static class EventResolution
    {
        // タグ変化や契約失敗から実損の係数に寄せる最小版
        public static float InferSeverityFromOutcome(Director.IEventCandidate _c, ResolutionOutcome outcome)
        {
            return outcome switch
            {
                ResolutionOutcome.SuccessPartial => 0.6f,
                ResolutionOutcome.Fail => 1.0f,
                _ => 0.3f
            };
        }
    }

    public enum ResolutionOutcome { Success, SuccessPartial, Fail }
}

