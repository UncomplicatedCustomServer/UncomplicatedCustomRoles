using Exiled.API.Features;
using Placeholders.API.Features;
using UncomplicatedCustomRoles.Placeholders.Placeholders;

namespace UncomplicatedCustomRoles.PlaceHolders
{
    public class Plugin : Plugin<Config>
    {
        public override void OnEnabled()
        {
            PlaceholdersAPI.Register(new RolePlaceholders());

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PlaceholdersAPI.Unregister(new RolePlaceholders());

            base.OnDisabled();
        }
    }
}
