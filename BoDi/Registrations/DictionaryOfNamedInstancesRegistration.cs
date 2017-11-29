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
using BoDi.Resolution;

namespace BoDi.Registrations
{

  public class DictionaryOfNamedInstancesRegistration : Registration
  {
    public override object Resolve(IObjectContainer container, RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      var typeToResolve = keyToResolve.Type;
      Debug.Assert(typeToResolve.IsGenericType && typeToResolve.GetGenericTypeDefinition() == typeof(IDictionary<,>));

      var genericArguments = typeToResolve.GetGenericArguments();
      var output = CreateGenericDictionary(genericArguments);
      var keyType = genericArguments[0];
      var targetType = genericArguments[1];
      var matchingRegistrationKeys = GetMatchingRegistrationKeys(targetType, container);

      foreach (var key in matchingRegistrationKeys)
      {
        var convertedKey = ChangeType(key.Name, keyType);
        Debug.Assert(convertedKey != null);
        output.Add(convertedKey, container.Resolve(key.Type, key.Name));
      }

      return output;
    }

    object ChangeType(string name, Type keyType)
    {
      if (keyType.IsEnum)
        return Enum.Parse(keyType, name, true);

      Debug.Assert(keyType == typeof(string));
      return name;
    }

    IDictionary CreateGenericDictionary(Type[] genericArguments)
    {
      return (IDictionary) Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(genericArguments));
    }

    IReadOnlyCollection<RegistrationKey> GetMatchingRegistrationKeys(Type targetType, IObjectContainer container)
    {
      return container.Registry
        .GetAll(targetType)
        .Where(x => x.Key.Name != null)
        .Select(x => x.Key)
        .ToArray();
    }

    public DictionaryOfNamedInstancesRegistration(RegistrationKey key) : base(key) {}
  }
}
