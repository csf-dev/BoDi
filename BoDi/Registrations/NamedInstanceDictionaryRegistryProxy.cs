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

namespace BoDi.Registrations
{
  public class NamedInstanceDictionaryRegistryProxy : IRegistry
  {
    bool isDisposed;
    readonly IRegistry proxiedInstance;
    readonly IRegistrationFactory registrationFactory;

    public void Add(IRegistration registration)
    {
      proxiedInstance.Add(registration);

      if(registration.Key.Name != null)
      {
        AddNamedInstanceDictionaryRegistration(registration.Key);
      }
    }

    public void AddIfNotExists(IRegistration registration)
    {
      if(registration == null)
        throw new ArgumentNullException(nameof(registration));

      if(!HasRegistration(registration.Key))
      {
        Add(registration);
      }
    }

    void AddNamedInstanceDictionaryRegistration(RegistrationKey key)
    {
      var registration = registrationFactory.CreateDictionaryOfNamesToImplementationTypes(key);
      proxiedInstance.AddIfNotExists(registration);
    }

    public IRegistration Get(RegistrationKey key)
    {
      return proxiedInstance.Get(key);
    }

    public IReadOnlyCollection<IRegistration> GetAll()
    {
      return proxiedInstance.GetAll();
    }

    public IReadOnlyCollection<IRegistration> GetAll(Type ofType)
    {
      return proxiedInstance.GetAll(ofType);
    }

    public bool HasRegistration(RegistrationKey key)
    {
      return proxiedInstance.HasRegistration(key);
    }

    public void Remove(RegistrationKey key)
    {
      proxiedInstance.Remove(key);
    }

    public void RemoveAll()
    {
      proxiedInstance.RemoveAll();
    }

    protected virtual void Dispose(bool disposing)
    {
      if(!isDisposed)
      {
        if(disposing)
        {
          proxiedInstance.Dispose();
        }

        isDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }

    public NamedInstanceDictionaryRegistryProxy(IRegistry proxiedInstance = null,
                                                IRegistrationFactory registrationFactory = null)
    {
      this.proxiedInstance = proxiedInstance?? new Registry();
      this.registrationFactory = registrationFactory?? new RegistrationFactory();
    }
  }
}
