﻿using System;
using System.Reflection;
using CSF.MicroDi.Resolution;

namespace CSF.MicroDi.Registration
{
  public class TypeRegistration : TypedRegistration
  {
    readonly Type implementationType;
    readonly ISelectsConstructor constructorSelector;

    public override Type ImplementationType => implementationType;

    public override IFactoryAdapter GetFactoryAdapter()
      => new ConstructorFactory(constructorSelector.SelectConstructor(implementationType));

    public override string ToString()
    {
      return $"[Type registration for `{ServiceType.FullName}', using type `{ImplementationType.FullName}']";
    }

    public TypeRegistration(Type implementationType) : this(implementationType, null) {}

    public TypeRegistration(Type implementationType, ISelectsConstructor constructorSelector)
    {
      if(implementationType == null)
        throw new ArgumentNullException(nameof(implementationType));

      this.implementationType = implementationType;
      this.constructorSelector = constructorSelector ?? new ConstructorWithMostParametersSelector();
    }
  }
}
