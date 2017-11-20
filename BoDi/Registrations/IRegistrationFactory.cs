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
namespace BoDi.Registrations
{
  public interface IRegistrationFactory
  {
    IRegistration CreateType<TConcrete,TAs>(string name = null);

    IRegistration CreateType(Type concreteType, Type asType, string name = null);

    IRegistration CreateType(string concreteTypeName, string asTypeName, string name = null);

    IRegistration CreateInstance(object instance, Type asType, string name = null);

    IRegistration CreateInstance<TAs>(object instance, string name = null);

    IRegistration CreateFromFactory<TAs>(Func<TAs> factory, string name = null);

    IRegistration CreateFromFactory<TAs>(Func<IObjectContainer,TAs> factory, string name = null);

    IRegistration CreateFromFactory<TAs>(Delegate factory, string name = null);

    IRegistration CreateFromFactory(Delegate factory, Type asType, string name = null);

    IRegistration CreateDictionaryOfNamesToImplementationTypes(Delegate factory, Type asType, string name = null);
  }
}
