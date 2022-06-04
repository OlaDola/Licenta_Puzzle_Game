// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using Baracuda.Reflection;

[assembly: DisableAssemblyReflection]
namespace Baracuda.Reflection
{
    /// <summary>
    /// Disable reflection for the target assembly or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class  | AttributeTargets.Struct)]
    public class DisableAssemblyReflectionAttribute : Attribute
    {
    }
}
