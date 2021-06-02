using Engine;

namespace Game
{
    public class FppCamera : BasePerspectiveCamera
    {
        public override bool UsesMovementControls => false;

        public override bool IsEntityControlEnabled => true;

        public FppCamera(GameWidget gameWidget)
            : base(gameWidget)
        {
        }

        public override void Activate(Camera previousCamera)
        {
            SetupPerspectiveCamera(previousCamera.ViewPosition, previousCamera.ViewDirection, previousCamera.ViewUp);
        }

        public override void Update(float dt)
        {
            if (base.GameWidget.Target != null)
            {
                Matrix matrix = Matrix.CreateFromQuaternion(base.GameWidget.Target.ComponentCreatureModel.EyeRotation);
                matrix.Translation = base.GameWidget.Target.ComponentCreatureModel.EyePosition;
                SetupPerspectiveCamera(matrix.Translation, matrix.Forward, matrix.Up);
            }
        }
    }
}
