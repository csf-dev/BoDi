﻿using System;
using CSF.MicroDi.Kernel;

namespace CSF.MicroDi.Registration
{
  public class RegistrationBuilder
    : IAsBuilder, IAsBuilderWithMultiplicity, IRegistrationOptionsBuilder, IRegistrationOptionsBuilderWithMultiplicity
  {
    ServiceRegistration registration;

    public IRegistrationOptionsBuilder As(Type serviceType)
    {
      if(serviceType == null)
        throw new ArgumentNullException(nameof(serviceType));
      registration.ServiceType = serviceType;
      return this;
    }

    public IRegistrationOptionsBuilder As<T>()
      where T : class
      => As(typeof(T));

    public IRegistrationOptionsBuilderWithMultiplicity AsOwnType()
    {
      var typedRegistration = registration as TypedRegistration;
      if(typedRegistration == null)
        throw new InvalidOperationException($"This operation is only suitable for {nameof(TypedRegistration)} instances.");

      registration.ServiceType = typedRegistration.ImplementationType;
      return this;
    }

    public IRegistrationOptionsBuilderWithMultiplicity SeparateInstancePerResolution()
    {
      registration.Multiplicity = Multiplicity.InstancePerResolution;
      return this;
    }

    public IRegistrationOptionsBuilderWithMultiplicity SingleSharedInstance()
    {
      registration.Multiplicity = Multiplicity.Shared;
      return this;
    }

    public IRegistrationOptionsBuilder WithName(string name)
    {
      registration.Name = name;
      return this;
    }

    IRegistrationOptionsBuilderWithMultiplicity IAsBuilderWithMultiplicity.As(Type serviceType)
    {
      if(serviceType == null)
        throw new ArgumentNullException(nameof(serviceType));
      registration.ServiceType = serviceType;
      return this;
    }

    IRegistrationOptionsBuilderWithMultiplicity IAsBuilderWithMultiplicity.As<T>()
    {
      registration.ServiceType = typeof(T);
      return this;
    }

    IRegistrationOptionsBuilderWithMultiplicity IRegistrationOptionsBuilderWithMultiplicity.WithName(string name)
    {
      registration.Name = name;
      return this;
    }

    public RegistrationBuilder(ServiceRegistration registration)
    {
      if(registration == null)
        throw new ArgumentNullException(nameof(registration));

      this.registration = registration;
    }
  }
}
