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
    readonly Dictionary<RegistrationKey,IRegistration> registrations;

    public void Add(IRegistration registration)
    {
      if(registration == null)
        throw new ArgumentNullException(nameof(registration));

      registrations[registration.Key] = registration;
    }

    public IRegistration Get(RegistrationKey key)
    {
      IRegistration output;
      if(registrations.TryGetValue(key, out output))
        return output;

      return null;
    }

    public IReadOnlyCollection<IRegistration> GetAll()
    {
      return registrations.Values.ToArray();
    }

    public bool HasRegistration(RegistrationKey key)
    {
      return registrations.ContainsKey(key);
    }

    public void Remove(RegistrationKey key)
    {
      registrations.Remove(key);
    }

    public Registry()
    {
      registrations = new Dictionary<RegistrationKey, IRegistration>();
    }
  }
}
