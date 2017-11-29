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
using BoDi.Resolution;

namespace BoDi.Registrations
{
  public class TypeRegistration : Registration
  {
    private readonly Type implementationType;

    public TypeRegistration(Type implementationType, RegistrationKey key) : base(key)
    {
      AssertValid(implementationType, key.Type);

      this.implementationType = implementationType;
    }

    public override object Resolve(IObjectContainer container, RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      var typeToConstruct = GetTypeToConstruct(keyToResolve);

      var pooledObjectKey = new RegistrationKey(typeToConstruct, keyToResolve.Name);
      object obj = container.ServicePool.GetOrReturnNull(pooledObjectKey);

      if (obj == null)
      {
        if (typeToConstruct.IsInterface)
          throw new ObjectContainerException("Interface cannot be resolved: " + keyToResolve, resolutionPath.GetTypes());

        obj = container.Resolver.Resolve(typeToConstruct, resolutionPath, keyToResolve);
        container.ServicePool.Add(pooledObjectKey, obj);
      }

      return obj;
    }

    void AssertValid(Type concreteType, Type asType)
    {
      if(!IsValid(concreteType, asType))
        throw new InvalidOperationException("type mapping is not valid");
    }

    private Type GetTypeToConstruct(RegistrationKey keyToResolve)
    {
      var targetType = implementationType;
      if (targetType.IsGenericTypeDefinition)
      {
        var typeArgs = keyToResolve.Type.GetGenericArguments();
        targetType = targetType.MakeGenericType(typeArgs);
      }
      return targetType;
    }

    public override string ToString()
    {
      return "Type: " + implementationType.FullName;
    }

    public static bool IsValid(Type concreteType, Type asType)
    {
      if(asType == null) return false;
      if(concreteType == null) return false;

      if(asType.IsAssignableFrom(concreteType))
        return true;

      if(asType.IsGenericTypeDefinition && concreteType.IsGenericTypeDefinition)
      {
        var baseTypes = GetBaseTypes(concreteType).ToArray();
        return baseTypes.Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == asType);
      }

      return false;
    }

    static IEnumerable<Type> GetBaseTypes(Type type)
    {
      if(type.BaseType == null) return type.GetInterfaces();

      return Enumerable.Repeat(type.BaseType, 1)
                       .Concat(type.GetInterfaces())
                       .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                       .Concat(GetBaseTypes(type.BaseType));
    }

  }
}
