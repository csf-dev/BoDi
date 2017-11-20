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
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BoDi.Kernel;
using BoDi.Registrations;
using BoDi.Config;
using BoDi.Resolution;

namespace BoDi
{

  public class ObjectContainer : IObjectContainer
  {
    private const string REGISTERED_NAME_PARAMETER_NAME = "registeredName";

    bool isDisposed = false;
    readonly ObjectContainer baseContainer;
    readonly IRegistry registry;
    readonly IRegistrationFactory registrationFactory;
    internal readonly Dictionary<RegistrationKey, object> resolvedObjects = new Dictionary<RegistrationKey, object>();
    internal readonly Dictionary<RegistrationKey, object> objectPool = new Dictionary<RegistrationKey, object>();

    public event Action<object> ObjectCreated;

    public ObjectContainer(IObjectContainer baseContainer) : this(baseContainer, null, null) {}

    public ObjectContainer(IObjectContainer baseContainer = null,
                           IRegistry registry = null,
                           IRegistrationFactory registrationFactory = null)
    {
      if(baseContainer != null && !(baseContainer is ObjectContainer))
        throw new ArgumentException("Base container must be an ObjectContainer", nameof(baseContainer));

      this.baseContainer = (ObjectContainer) baseContainer;

      this.registry = registry ?? new NamedInstanceDictionaryRegistryProxy();
      this.registrationFactory = registrationFactory ?? new RegistrationFactory();

      RegisterInstanceAs<IObjectContainer>(this);
    }

    public void RegisterTypeAs<TInterface>(Type implementationType, string name = null) where TInterface : class
    {
      var registration = registrationFactory.CreateType(implementationType, typeof(TInterface), name);
      Add(registration);
    }

    public void RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface
    {
      var registration = registrationFactory.CreateType(typeof(TType), typeof(TInterface), name);
      Add(registration);
    }

    public void RegisterTypeAs(Type implementationType, Type interfaceType)
    {
      var registration = registrationFactory.CreateType(implementationType, interfaceType, null);
      Add(registration);
    }

    public void RegisterTypeAs(Type implementationType, Type interfaceType, string name)
    {
      var registration = registrationFactory.CreateType(implementationType, interfaceType, name);
      Add(registration);
    }

    public void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false)
    {
      if(instance == null)
        throw new ArgumentNullException(nameof(instance));
      var registrationKey = new RegistrationKey(interfaceType, name);
      AssertNotResolved(registrationKey);

      ClearRegistrations(registrationKey);
      Add(new InstanceRegistration(instance, registrationKey));
      objectPool[new RegistrationKey(instance.GetType(), name)] = GetPoolableInstance(instance, dispose);
    }

    private static object GetPoolableInstance(object instance, bool dispose)
    {
      return (instance is IDisposable) && !dispose ? new NonDisposableWrapper(instance) : instance;
    }

    public void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class
    {
      RegisterInstanceAs(instance, typeof(TInterface), name, dispose);
    }

    public void RegisterFactoryAs<TInterface>(Func<TInterface> factoryDelegate, string name = null)
    {
      var registration = registrationFactory.CreateFromFactory(factoryDelegate, name);
      Add(registration);
    }

    public void RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null)
    {
      var registration = registrationFactory.CreateFromFactory(factoryDelegate, name);
      Add(registration);
    }

    public void RegisterFactoryAs<TInterface>(Delegate factoryDelegate, string name = null)
    {
      var registration = registrationFactory.CreateFromFactory<TInterface>(factoryDelegate, name);
      Add(registration);
    }

    public void RegisterFactoryAs(Delegate factoryDelegate, Type interfaceType, string name = null)
    {
      var registration = registrationFactory.CreateFromFactory(factoryDelegate, interfaceType, name);
      Add(registration);
    }

    public bool IsRegistered<T>()
    {
      return IsRegistered<T>(null);
    }

    public bool IsRegistered<T>(string name)
    {
      return registry.HasRegistration(new RegistrationKey(typeof(T), name));
    }

    private void AssertNotResolved(RegistrationKey interfaceType)
    {
      if(resolvedObjects.ContainsKey(interfaceType))
        throw new ObjectContainerException("An object has been resolved for this interface already.", null);
    }

    private void ClearRegistrations(RegistrationKey registrationKey)
    {
      registry.Remove(registrationKey);
    }

    public void RegisterFromConfiguration()
    {
      var provider = GetConfigurationTypeRegistrationProvider();
      var configRegistrations = provider.GetRegistrations();
      Add(configRegistrations);
    }

    public void RegisterFromConfiguration(ContainerRegistrationCollection registrationCollection)
    {
      var provider = GetConfigurationTypeRegistrationProvider();
      var configRegistrations = provider.GetRegistrations(registrationCollection);
      Add(configRegistrations);
    }

    void Add(IReadOnlyList<IRegistration> toAdd)
    {
      if(toAdd == null)
        throw new ArgumentNullException(nameof(toAdd));

      foreach(var reg in toAdd)
      {
        Add(reg);
      }
    }

    void Add(IRegistration toAdd)
    {
      if(toAdd == null)
        throw new ArgumentNullException(nameof(toAdd));

      AssertNotResolved(toAdd.Key);
      ClearRegistrations(toAdd.Key);

      registry.Add(toAdd);
    }

    IProvidesTypeRegistrationsFromConfiguration GetConfigurationTypeRegistrationProvider()
      => new ConfigurationTypeRegistrationProvider();

    public T Resolve<T>()
    {
      return (T) Resolve(typeof(T), null);
    }

    public T Resolve<T>(string name)
    {
      return (T) Resolve(typeof(T), name);
    }

    public object Resolve(Type typeToResolve, string name = null)
    {
      var key = new RegistrationKey(typeToResolve, name);
      return Resolve(key);
    }

    protected virtual object Resolve(RegistrationKey key)
    {
      var resolver = CreateResolver();
      return resolver.Resolve(key);
    }

    IResolver CreateResolver()
    {
      return new Resolver();
    }

    public IEnumerable<T> ResolveAll<T>() where T : class
    {
      return registry.GetAll(typeof(T))
          .Select(x => Resolve(x.Key))
          .Cast<T>();
    }

    private object Resolve(Type typeToResolve, ResolutionPath resolutionPath, string name)
    {
      AssertNotDisposed();

      var keyToResolve = new RegistrationKey(typeToResolve, name);
      object resolvedObject;
      if(!resolvedObjects.TryGetValue(keyToResolve, out resolvedObject))
      {
        resolvedObject = ResolveObject(keyToResolve, resolutionPath);
        resolvedObjects.Add(keyToResolve, resolvedObject);
      }
      Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
      return resolvedObject;
    }

    private KeyValuePair<ObjectContainer, IRegistration>? GetRegistrationResult(RegistrationKey keyToResolve)
    {
      var registration = registry.Get(keyToResolve);
      if(registration != null)
        return new KeyValuePair<ObjectContainer, IRegistration>(this, registration);

      if(baseContainer != null)
        return baseContainer.GetRegistrationResult(keyToResolve);

      if(IsSpecialNamedInstanceDictionaryKey(keyToResolve))
      {
        var targetType = keyToResolve.Type.GetGenericArguments()[1];
        var namedInstanceKey = registrationFactory.CreateDictionaryOfNamedInstancesRegistrationKey(targetType);
        return GetRegistrationResult(namedInstanceKey);
      }

      // if there was no named registration, we still return an empty dictionary
      if(IsDefaultNamedInstanceDictionaryKey(keyToResolve))
      {
        return new KeyValuePair<ObjectContainer, IRegistration>(this, new DictionaryOfNamedInstancesRegistration(keyToResolve));
      }

      return null;
    }

    private bool IsDefaultNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
    {
      return IsNamedInstanceDictionaryKey(keyToResolve) &&
             keyToResolve.Type.GetGenericArguments()[0] == typeof(string);
    }

    private bool IsSpecialNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
    {
      return IsNamedInstanceDictionaryKey(keyToResolve) &&
             keyToResolve.Type.GetGenericArguments()[0].IsEnum;
    }

    private bool IsNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
    {
      return keyToResolve.Name == null && keyToResolve.Type.IsGenericType && keyToResolve.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
    }

    internal object GetPooledObject(RegistrationKey pooledObjectKey)
    {
      object obj;
      if(GetObjectFromPool(pooledObjectKey, out obj))
        return obj;

      return null;
    }

    private bool GetObjectFromPool(RegistrationKey pooledObjectKey, out object obj)
    {
      if(!objectPool.TryGetValue(pooledObjectKey, out obj))
        return false;

      var nonDisposableWrapper = obj as NonDisposableWrapper;
      if(nonDisposableWrapper != null)
        obj = nonDisposableWrapper.Object;

      return true;
    }

    private object ResolveObject(RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      if(keyToResolve.Type.IsPrimitive || keyToResolve.Type == typeof(string) || keyToResolve.Type.IsValueType)
        throw new ObjectContainerException("Primitive types or structs cannot be resolved: " + keyToResolve.Type.FullName, resolutionPath.GetTypes());

      var registrationResult = GetRegistrationResult(keyToResolve) ??
  new KeyValuePair<ObjectContainer, IRegistration>(this, new TypeRegistration(keyToResolve.Type, keyToResolve));

      var resolutionPathForResolve = registrationResult.Key == this ?
                                                       resolutionPath : new ResolutionPath();
      return registrationResult.Value.Resolve(registrationResult.Key, keyToResolve, resolutionPathForResolve);
    }

    internal object CreateObject(Type type, ResolutionPath resolutionPath, RegistrationKey keyToResolve)
    {
      var ctors = type.GetConstructors();
      if(ctors.Length == 0)
        ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

      Debug.Assert(ctors.Length > 0, "Class must have a constructor!");

      int maxParamCount = ctors.Max(ctor => ctor.GetParameters().Length);
      var maxParamCountCtors = ctors.Where(ctor => ctor.GetParameters().Length == maxParamCount).ToArray();

      object obj;
      if(maxParamCountCtors.Length == 1)
      {
        ConstructorInfo ctor = maxParamCountCtors[0];
        if(resolutionPath.Contains(keyToResolve))
          throw new ObjectContainerException("Circular dependency found! " + type.FullName, resolutionPath.GetTypes());

        var args = ResolveArguments(ctor.GetParameters(), keyToResolve, resolutionPath.CreateChild(keyToResolve, type));
        obj = ctor.Invoke(args);
      }
      else
      {
        throw new ObjectContainerException("Multiple public constructors with same maximum parameter count are not supported! " + type.FullName, resolutionPath.GetTypes());
      }

      OnObjectCreated(obj);

      return obj;
    }

    protected virtual void OnObjectCreated(object obj)
    {
      var eventHandler = ObjectCreated;
      if(eventHandler != null)
        eventHandler(obj);
    }

    internal object InvokeFactoryDelegate(Delegate factoryDelegate, ResolutionPath resolutionPath, RegistrationKey keyToResolve)
    {
      if(resolutionPath.Contains(keyToResolve))
        throw new ObjectContainerException("Circular dependency found! " + factoryDelegate, resolutionPath.GetTypes());

      var args = ResolveArguments(factoryDelegate.Method.GetParameters(), keyToResolve, resolutionPath.CreateChild(keyToResolve, null));
      return factoryDelegate.DynamicInvoke(args);
    }

    private object[] ResolveArguments(IEnumerable<ParameterInfo> parameters, RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      return parameters.Select(p => IsRegisteredNameParameter(p) ? ResolveRegisteredName(keyToResolve) : Resolve(p.ParameterType, resolutionPath, null)).ToArray();
    }

    private object ResolveRegisteredName(RegistrationKey keyToResolve)
    {
      return keyToResolve.Name;
    }

    private bool IsRegisteredNameParameter(ParameterInfo parameterInfo)
    {
      return parameterInfo.ParameterType == typeof(string) &&
             parameterInfo.Name.Equals(REGISTERED_NAME_PARAMETER_NAME);
    }

    public override string ToString()
    {
      var formattedRegistrations = registry
        .GetAll()
        .Where(r => !(r is DictionaryOfNamedInstancesRegistration))
        .Select(r => Format(r));

      return string.Join(Environment.NewLine, formattedRegistrations);
    }

    string Format(IRegistration registration)
    {
      string registrationName;

      if(registration.Key.Type == typeof(IObjectContainer)
         && registration.Key.Name == null)
      {
        registrationName = "<self>";
      }
      else
      {
        registrationName = registration.ToString();
      }

      return $"{registration.Key} -> {registrationName}";
    }

    private void AssertNotDisposed()
    {
      if(isDisposed)
        throw new ObjectContainerException("Object container disposed", null);
    }

    public void Dispose()
    {
      isDisposed = true;

      foreach(var obj in objectPool.Values.OfType<IDisposable>().Where(o => !ReferenceEquals(o, this)))
        obj.Dispose();

      objectPool.Clear();
      registry.RemoveAll();
      resolvedObjects.Clear();
    }
  }
}