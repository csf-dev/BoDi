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
using BoDi.Registrations;
using BoDi.Config;
using BoDi.Resolution;

namespace BoDi
{

  public class ObjectContainer : IObjectContainer
  {
    bool isDisposed;
    readonly IObjectContainer baseContainer;
    readonly IRegistry registry;
    readonly IRegistrationFactory registrationFactory;
    readonly IResolver resolver;
    readonly IPoolsServiceInstances servicePool;
    readonly ICachesResolvedServices serviceCache;

    public event Action<object> ObjectCreated;

    public IRegistry Registry => registry;

    public IResolver Resolver => resolver;

    public IObjectContainer BaseContainer => baseContainer;

    public IPoolsServiceInstances ServicePool => servicePool;

    public ObjectContainer(IObjectContainer baseContainer) : this(baseContainer, null, null) {}

    public ObjectContainer(IObjectContainer baseContainer = null,
                           IRegistry registry = null,
                           IRegistrationFactory registrationFactory = null,
                           IResolver resolver = null,
                           IPoolsServiceInstances servicePool = null,
                           ICachesResolvedServices serviceCache = null)
    {
      this.registry = registry ?? new NamedInstanceDictionaryRegistryProxy();
      this.registrationFactory = registrationFactory ?? new RegistrationFactory();

      this.baseContainer = baseContainer;

      var containers = GetContainerStack();

      this.servicePool = servicePool ?? new InstanceCache();
      this.serviceCache = serviceCache ?? new InstanceCache();
      this.resolver = resolver?? new Resolver(containers, serviceCache: this.serviceCache);

      this.resolver.ObjectCreated += OnObjectCreated;

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

      ClearRegistrations(registrationKey);
      Add(new InstanceRegistration(instance, registrationKey));

      AddToServicePool(instance, name, !dispose);
    }

    void AddToServicePool(object instance, string name, bool suppressDisposal)
    {
      var instanceToPool = GetInstanceForPooling(instance, suppressDisposal);
      var registrationKey = new RegistrationKey(instance.GetType(), name);
      servicePool.Add(registrationKey, instanceToPool);
    }

    object GetInstanceForPooling(object instance, bool suppressDisposal)
    {
      var instanceToPool = instance;
      if((instance is IDisposable) && suppressDisposal)
      {
        instanceToPool = new NonDisposableWrapper(instance);
      }
      return instanceToPool;
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
      if(serviceCache.Contains(interfaceType))
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
      AssertNotDisposed();
      return resolver.Resolve(key);
    }

    public IEnumerable<T> ResolveAll<T>() where T : class
    {
      return registry.GetAll(typeof(T))
          .Select(x => Resolve(x.Key))
          .Cast<T>();
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

    public IReadOnlyList<IObjectContainer> GetContainerStack()
    {
      IEnumerable<IObjectContainer> output = new [] { this };

      if(baseContainer != null)
      {
        var baseStack = baseContainer.GetContainerStack();
        if(baseStack != null)
          output = output.Union(baseStack);
      }

      return output.ToArray();
    }

    void OnObjectCreated(object obj)
    {
      ObjectCreated?.Invoke(obj);
    }

    protected virtual void Dispose(bool disposing)
    {
      if(!isDisposed)
      {
        if(disposing)
        {
          registry.Dispose();
          resolver.Dispose();
          servicePool.Dispose();
        }

        isDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }
}