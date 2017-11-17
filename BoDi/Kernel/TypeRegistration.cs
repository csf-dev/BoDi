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
namespace BoDi.Kernel
{
  class TypeRegistration : IRegistration
  {
    private readonly Type implementationType;

    public TypeRegistration(Type implementationType)
    {
      this.implementationType = implementationType;
    }

    public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
    {
      var typeToConstruct = GetTypeToConstruct(keyToResolve);

      var pooledObjectKey = new RegistrationKey(typeToConstruct, keyToResolve.Name);
      object obj = container.GetPooledObject(pooledObjectKey);

      if (obj == null)
      {
        if (typeToConstruct.IsInterface)
          throw new ObjectContainerException("Interface cannot be resolved: " + keyToResolve, resolutionPath.ToTypeList());

        obj = container.CreateObject(typeToConstruct, resolutionPath, keyToResolve);
        container.objectPool.Add(pooledObjectKey, obj);
      }

      return obj;
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
  }
}
