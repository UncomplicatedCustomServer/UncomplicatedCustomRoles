using Exiled.API.Features;
using Exiled.API.Features.Roles;

namespace UncomplicatedCustomRoles.API.Features
{
    public class MovementBehaviour
    {
        public float WalkingSpeed { get; set; } = 3.9f;

        public float JumpingSpeed { get; set; } = 4.9f;

        public float CrouchingSpeed { get; set; } = 0f;

        public float SprintingSpeed { get; set; } = 5.4f;

        public void Apply(Player player)
        {
            if (player.Role is FpcRole Fpc)
            {
                Fpc.WalkingSpeed = WalkingSpeed;
                Fpc.JumpingSpeed = JumpingSpeed;
                Fpc.CrouchingSpeed = CrouchingSpeed;
                Fpc.SprintingSpeed = SprintingSpeed;
            }
        }
    }
}
