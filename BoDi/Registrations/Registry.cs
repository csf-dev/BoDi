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
using System.Linq;

namespace BoDi.Registrations
{
  public class Registry : IRegistry
  {
    bool isDisposed;
    readonly Dictionary<RegistrationKey,IRegistration> registrations;

    public void Add(IRegistration registration)
    {
      if(registration == null)
        throw new ArgumentNullException(nameof(registration));
      AssertNotDisposed();

      registrations[registration.Key] = registration;
    }

    public void AddIfNotExists(IRegistration registration)
    {
      if(registration == null)
        throw new ArgumentNullException(nameof(registration));
      AssertNotDisposed();

      if(!HasRegistration(registration.Key))
      {
        registrations[registration.Key] = registration;
      }
    }

    public IRegistration Get(RegistrationKey key)
    {
      AssertNotDisposed();

      IRegistration output;
      if(registrations.TryGetValue(key, out output))
        return output;

      return null;
    }

    public IReadOnlyCollection<IRegistration> GetAll()
    {
      AssertNotDisposed();
      return registrations.Values.ToArray();
    }

    public IReadOnlyCollection<IRegistration> GetAll(Type ofType)
    {
      if(ofType == null)
        throw new ArgumentNullException(nameof(ofType));
      AssertNotDisposed();

      return GetAll().Where(x => x.Key.Type == ofType).ToArray();
    }

    public bool HasRegistration(RegistrationKey key)
    {
      AssertNotDisposed();
      return registrations.ContainsKey(key);
    }

    public void Remove(RegistrationKey key)
    {
      AssertNotDisposed();
      registrations.Remove(key);
    }

    public void RemoveAll()
    {
      AssertNotDisposed();
      registrations.Clear();
    }

    void AssertNotDisposed()
    {
      if(isDisposed)
        throw new InvalidOperationException("The registry must not be disposed");
    }

    protected virtual void Dispose(bool disposing)
    {
      if(!isDisposed)
      {
        if(disposing)
        {
          registrations.Clear();
        }

        isDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }

    public Registry()
    {
      registrations = new Dictionary<RegistrationKey, IRegistration>();
    }
  }
}
