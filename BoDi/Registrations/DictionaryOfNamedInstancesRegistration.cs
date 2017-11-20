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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BoDi.Kernel;
using BoDi.Resolution;

namespace BoDi.Registrations
{

  public class DictionaryOfNamedInstancesRegistration : Registration
  {
    public override object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      var typeToResolve = keyToResolve.Type;
      Debug.Assert(typeToResolve.IsGenericType && typeToResolve.GetGenericTypeDefinition() == typeof(IDictionary<,>));

      var genericArguments = typeToResolve.GetGenericArguments();
      var keyType = genericArguments[0];
      var targetType = genericArguments[1];
      var result = (IDictionary)Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(genericArguments));

      foreach (var namedRegistration in container.registrations.Where(r => r.Key.Name != null && r.Key.Type == targetType).Select(r => r.Key).ToList())
      {
        var convertedKey = ChangeType(namedRegistration.Name, keyType);
        Debug.Assert(convertedKey != null);
        result.Add(convertedKey, container.Resolve(namedRegistration.Type, namedRegistration.Name));
      }

      return result;
    }

    private object ChangeType(string name, Type keyType)
    {
      if (keyType.IsEnum)
        return Enum.Parse(keyType, name, true);

      Debug.Assert(keyType == typeof(string));
      return name;
    }

    public DictionaryOfNamedInstancesRegistration(RegistrationKey key) : base(key) {}
  }
}
