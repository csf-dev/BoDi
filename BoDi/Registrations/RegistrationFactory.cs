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
  public class RegistrationFactory : IRegistrationFactory
  {
    static readonly Type OpenGenericDictionary = typeof(IDictionary<,>);

    public IRegistration CreateDictionaryOfNamesToImplementationTypes(RegistrationKey originalRegistrationKey)
    {
      var newKey = CreateDictionaryOfNamedInstancesRegistrationKey(originalRegistrationKey.Type);
      return new DictionaryOfNamedInstancesRegistration(newKey);
    }

    public IRegistration CreateFromFactory(Delegate factory, Type asType, string name = null)
    {
      if(factory == null)
        throw new ArgumentNullException(nameof(factory));
      if(asType == null)
        throw new ArgumentNullException(nameof(asType));
      
      var key = new RegistrationKey(asType, name);
      return new FactoryRegistration(factory, key);
    }

    public IRegistration CreateFromFactory<TAs>(Func<IObjectContainer, TAs> factory, string name = null)
    {
      return CreateFromFactory(factory, typeof(TAs), name);
    }

    public IRegistration CreateFromFactory<TAs>(Delegate factory, string name = null)
    {
      return CreateFromFactory(factory, typeof(TAs), name);
    }

    public IRegistration CreateFromFactory<TAs>(Func<TAs> factory, string name = null)
    {
      return CreateFromFactory(factory, typeof(TAs), name);
    }

    public IRegistration CreateInstance(object instance, Type asType, string name = null)
    {
      throw new NotImplementedException();
    }

    public IRegistration CreateInstance<TAs>(object instance, string name = null)
    {
      throw new NotImplementedException();
    }

    public IRegistration CreateType(string concreteTypeName, string asTypeName, string name = null)
    {
      var asType = Type.GetType(asTypeName, true);
      var concreteType = Type.GetType(concreteTypeName, true);

      return CreateType(concreteType, asType, name);
    }

    public IRegistration CreateType(Type concreteType, Type asType, string name = null)
    {
      var key = new RegistrationKey(asType, name);
      return new TypeRegistration(concreteType, key);
    }

    public IRegistration CreateType<TConcrete, TAs>(string name = null)
    {
      return CreateType(typeof(TConcrete), typeof(TAs), name);
    }

    public RegistrationKey CreateDictionaryOfNamedInstancesRegistrationKey(Type asType)
    {
      var newKeyType = GetNamedInstanceDictionaryType(asType);
      return new RegistrationKey(newKeyType, null);
    }

    Type GetNamedInstanceDictionaryType(Type instanceType)
    {
      return OpenGenericDictionary.MakeGenericType(typeof(string), instanceType);
    }
  }
}
