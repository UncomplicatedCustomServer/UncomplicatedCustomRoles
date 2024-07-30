using UncomplicatedCustomRoles.API.Helpers.Imports.EXILED.YAML;
using YamlDotNet.Serialization;

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
    }
}
