using UncomplicatedCustomRoles.API.Helpers.Imports.EXILED.YAML;
using UncomplicatedCustomRoles.API.Helpers.Imports.EXILED.YAML.Configs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace UncomplicatedCustomRoles.Manager
{
    internal class YamlHelper
    {
        public static ISerializer Serializer { get; set; } = new SerializerBuilder()
            .WithTypeConverter(new VectorsConverter())
            .WithTypeConverter(new ColorConverter())
            .WithEventEmitter(eventEmitter => new TypeAssigningEventEmitter(eventEmitter))
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreFields()
            .DisableAliases()
            .Build();

        //
        // Riepilogo:
        //     Gets or sets the deserializer for configs and translations.
        public static IDeserializer Deserializer { get; set; } = new DeserializerBuilder()
            .WithTypeConverter(new VectorsConverter())
            .WithTypeConverter(new ColorConverter())
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithNodeDeserializer((INodeDeserializer inner) => new ValidatingNodeDeserializer(inner), delegate (ITrackingRegistrationLocationSelectionSyntax<INodeDeserializer> deserializer)
            {
                deserializer.InsteadOf<ObjectNodeDeserializer>();
            })
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();
    }
}
