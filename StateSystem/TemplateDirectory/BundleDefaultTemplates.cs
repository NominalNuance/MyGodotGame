
using System;
using System.Collections.Generic;

namespace EroJRPG.StateSystem.TemplateDirectory;

public interface IBundleDefaultTemplate
{
    public Type BundleType { get; }
    public IReadOnlyDictionary<IStateKey, object> DefaultValues { get;}
}

public class HealthBundleWoundedDefault : IBundleDefaultTemplate
{
    public Type BundleType { get; } = typeof(HealthBundle);
    public IReadOnlyDictionary<IStateKey, object> DefaultValues { get;} = 
        new Dictionary<IStateKey, object>
        {
            [HealthBundle.CurrentHealth] = 50.0d
        };
}