using Godot;

public partial class Player : CharacterBody3D
{
    private const float Speed = 5.0f * 5f;
    private const float JumpVelocity = 6.5f;

    private float lookSensetivity = 0.002f;

    [NodePath("Camera3D")]
    private Camera3D camera3D;

    [NodePath("Camera3D/RayCast3D")]
    private RayCast3D rayCast3D;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            Rotation = new Vector3(Rotation.X, Rotation.Y - motion.Relative.X * lookSensetivity, Rotation.Z);
            camera3D.Rotation = new Vector3(Mathf.Clamp(camera3D.Rotation.X - motion.Relative.Y * lookSensetivity, Mathf.DegToRad(-90.0f), Mathf.DegToRad(90.0f)), camera3D.Rotation.Y, camera3D.Rotation.Z);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
        }

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
        }

        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backwards");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
        }
        else
        {
            Velocity = new Vector3(Mathf.MoveToward(Velocity.X, 0, Speed), Velocity.Y, Mathf.MoveToward(Velocity.Z, 0, Speed));
        }

        if (Input.IsActionJustPressed("left_click"))
        {
            if (rayCast3D.IsColliding() && rayCast3D.GetCollider() is Node collider && collider.HasMethod("destroy_block"))
            {
                collider.Call("destroy", rayCast3D.GetCollisionPoint() - rayCast3D.GetCollisionNormal() / 2);
            }
        }
        if (Input.IsActionJustPressed("right_click"))
        {
            if (rayCast3D.IsColliding() && rayCast3D.GetCollider() is Node collider && collider.HasMethod("place_block"))
            {
                collider.Call("place", rayCast3D.GetCollisionPoint() + rayCast3D.GetCollisionNormal() / 2, 1);
            }
        }

        MoveAndSlide();
    }
}
