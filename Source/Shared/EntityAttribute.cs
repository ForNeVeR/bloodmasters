using JetBrains.Annotations;

namespace Bloodmasters;

/// <summary>
/// Classes marked with this attribute are instantiated in runtime via reflection, and thus should be excluded from the
/// IDE usage analysis.
/// </summary>
[MeansImplicitUse]
public abstract class EntityAttribute : Attribute
{
}
