namespace SpriteAnimatorEditor
{
    internal static class Enums
    {
        public enum DragState
        {
            Start,
            Update,
            End,
            Clear,
            Use
        }

        public enum DragResult
        {
            EqualContext,
            ElementIsParentOfTarget,
            Success,
            TargetIsNotFolder,
        }
    }
}
