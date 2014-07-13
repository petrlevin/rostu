using System;

namespace Platform.Utils.Extensions
{
    [Flags]
    public enum TypeOptions
    {
        All = 0,
        Public = 1,
        NotAbstract = 2,
        WithPublicParameterLessConstructor = 4,
        IsClass = 8,
        AutoInvokable =  NotAbstract | WithPublicParameterLessConstructor

    }
}