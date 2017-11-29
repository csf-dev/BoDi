// BoDi: A very simple IoC container.
//
// BoDi was created to support SpecFlow (http://www.specflow.org) by Gaspar Nagy (http://gasparnagy.com/)
//
// Project source & unit tests: http://github.com/gasparnagy/BoDi
// License: Apache License 2.0
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BoDi.Registrations;

namespace BoDi.Resolution
{
  public class Resolver : IResolver
  {
    const string REGISTERED_NAME_PARAMETER_NAME = "registeredName";

    bool isDisposed;
    readonly IReadOnlyList<IObjectContainer> containers;
    readonly IRegistrationFactory registrationFactory;
    readonly ICachesResolvedServices serviceCache;
    readonly IPoolsServiceInstances instancePool;

    public event Action<object> ObjectCreated;

    public object Resolve(RegistrationKey key)
    {
      AssertNotDisposed();
      throw new NotImplementedException();
    }

    public object Resolve(Delegate objectFactory, ResolutionPath resolutionPath, RegistrationKey keyToResolve)
    {
      ThrowOnCircularDependency(resolutionPath, keyToResolve, objectFactory.ToString());
      return CreateObject(new DelegateFactoryAdapter(objectFactory), resolutionPath, keyToResolve);
    }

    public object Resolve(Type type, ResolutionPath resolutionPath, RegistrationKey keyToResolve)
    {
      ThrowOnCircularDependency(resolutionPath, keyToResolve, type.FullName);

      var ctor = SelectConstructor(type, resolutionPath);
      return CreateObject(new ConstructorFactoryAdapter(ctor), resolutionPath, keyToResolve, true, type);
    }

    void AssertNotDisposed()
    {
      if(isDisposed)
        throw new InvalidOperationException("The resolver must not be disposed.");
    }

    private object Resolve(Type typeToResolve, ResolutionPath resolutionPath, string name)
    {
      var keyToResolve = new RegistrationKey(typeToResolve, name);

      object resolvedObject;
      if(serviceCache.TryGet(keyToResolve, out resolvedObject))
      {
        Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
        return resolvedObject;
      }

      resolvedObject = ResolveObject(keyToResolve, resolutionPath);
      serviceCache.Add(keyToResolve, resolvedObject);

      Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
      return resolvedObject;
    }

    OwnedRegistration GetOwnedRegistration(RegistrationKey keyToResolve)
    {
      foreach(var container in containers)
      {
        var registration = GetOwnedRegistration(keyToResolve, container);
        if(registration != null)
          return registration;
      }

      if(IsEnumerationBasedDictionaryOfNamedInstancesKey(keyToResolve))
      {
        var convertedKey = ConvertFromEnumerationToStringBasedDictionaryOfNamedInstancesKey(keyToResolve);
        return GetOwnedRegistration(convertedKey);
      }

      if(IsStringBasedDictionaryOfNamedInstancesKey(keyToResolve))
      {
        var registration = new DictionaryOfNamedInstancesRegistration(keyToResolve);
        return new OwnedRegistration(containers.Last(), registration);
      }

      return new OwnedRegistration(containers.First(), new TypeRegistration(keyToResolve.Type, keyToResolve));
    }

    OwnedRegistration GetOwnedRegistration(RegistrationKey key, IObjectContainer container)
    {
      if(container == null)
        throw new ArgumentNullException(nameof(container));

      var registration = container.Registry.Get(key);
      if(registration == null)
        return null;

      return new OwnedRegistration(container, registration);
    }

    RegistrationKey ConvertFromEnumerationToStringBasedDictionaryOfNamedInstancesKey(RegistrationKey input)
    {
      var serviceType = input.Type.GetGenericArguments()[1];
      return registrationFactory.CreateDictionaryOfNamedInstancesRegistrationKey(serviceType);
    }

    private bool IsStringBasedDictionaryOfNamedInstancesKey(RegistrationKey keyToResolve)
    {
      return (IsDictionaryOfNamedInstancesKey(keyToResolve)
              && keyToResolve.Type.GetGenericArguments()[0] == typeof(string));
    }

    private bool IsEnumerationBasedDictionaryOfNamedInstancesKey(RegistrationKey keyToResolve)
    {
      return (IsDictionaryOfNamedInstancesKey(keyToResolve)
              && keyToResolve.Type.GetGenericArguments()[0].IsEnum);
    }

    private bool IsDictionaryOfNamedInstancesKey(RegistrationKey keyToResolve)
    {
      return (keyToResolve.Name == null
              && keyToResolve.Type.IsGenericType
              && keyToResolve.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    object ResolveObject(RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      if(keyToResolve.Type.IsPrimitive || keyToResolve.Type == typeof(string) || keyToResolve.Type.IsValueType)
        throw new ObjectContainerException("Primitive types or structs cannot be resolved: " + keyToResolve.Type.FullName, resolutionPath.GetTypes());

      var ownedRegistration = GetOwnedRegistration(keyToResolve);
      return ResolveObject(keyToResolve, resolutionPath, ownedRegistration);
    }

    object ResolveObject(RegistrationKey keyToResolve, ResolutionPath resolutionPath, OwnedRegistration ownedRegistration)
    {
      var registrationIsOwnedByMyContainer = ownedRegistration.Owner == containers.First();
      var path = registrationIsOwnedByMyContainer? resolutionPath : new ResolutionPath();
      return ownedRegistration.Registration.Resolve(ownedRegistration.Owner, keyToResolve, path);
    }

    object CreateObject(IFactoryAdapter factory,
                        ResolutionPath resolutionPath,
                        RegistrationKey keyToResolve,
                        bool triggerObjectCreated = false,
                        Type resolutionType = null)
    {
      var childResolutionPath = resolutionPath.CreateChild(keyToResolve, resolutionType);
      var args = ResolveMethodParameters(factory.GetParameters(), keyToResolve, childResolutionPath);
      var obj = factory.CreateObject(args);

      if(triggerObjectCreated)
        OnObjectCreated(obj);

      return obj;
    }

    ConstructorInfo SelectConstructor(Type type, ResolutionPath resolutionPath)
    {
      var ctors = type.GetConstructors();
      if(ctors.Length == 0)
        ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

      Debug.Assert(ctors.Length > 0, "Class must have a constructor!");

      int maxParamCount = ctors.Max(ctor => ctor.GetParameters().Length);
      var maxParamCountCtors = ctors.Where(ctor => ctor.GetParameters().Length == maxParamCount).ToArray();

      if(maxParamCountCtors.Length > 1)
        throw new ObjectContainerException("Multiple public constructors with same maximum parameter count are not supported! " + type.FullName, resolutionPath.GetTypes());
      
      return maxParamCountCtors.Single();
    }

    void ThrowOnCircularDependency(ResolutionPath resolutionPath, RegistrationKey keyToResolve, string description)
    {
      if(resolutionPath.Contains(keyToResolve))
        throw new ObjectContainerException("Circular dependency found! " + description, resolutionPath.GetTypes());
    }

    protected virtual void OnObjectCreated(object obj) => ObjectCreated?.Invoke(obj);

    object[] ResolveMethodParameters(IEnumerable<ParameterInfo> parameters,
                                     RegistrationKey keyToResolve,
                                     ResolutionPath resolutionPath)
    {
      return parameters
        .Select(p => {
          if(IsRegisteredNameParameter(p)) return ResolveRegisteredName(keyToResolve);

          return Resolve(p.ParameterType, resolutionPath, null);
        })
        .ToArray();
    }

    object ResolveRegisteredName(RegistrationKey keyToResolve) => keyToResolve.Name;

    bool IsRegisteredNameParameter(ParameterInfo param)
      => param.ParameterType == typeof(string) && param.Name.Equals(REGISTERED_NAME_PARAMETER_NAME);

    protected virtual void Dispose(bool disposing)
    {
      if(!isDisposed)
      {
        if(disposing)
        {
          serviceCache.Dispose();
          instancePool.Dispose();
        }

        isDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }

    public Resolver(IReadOnlyList<IObjectContainer> containers,
                    IRegistrationFactory registrationFactory = null,
                    ICachesResolvedServices serviceCache = null,
                    IPoolsServiceInstances instancePool = null)
    {
      if(containers == null)
        throw new ArgumentNullException(nameof(containers));
      if(!containers.Any())
        throw new ArgumentException("Containers collection must not be empty.", nameof(containers));
      
      this.containers = containers;
      this.registrationFactory = registrationFactory ?? new RegistrationFactory();
      this.serviceCache = serviceCache ?? new InstanceCache();
      this.instancePool = instancePool ?? new InstanceCache();
    }
  }
}
