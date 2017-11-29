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
using BoDi.Registrations;

namespace BoDi.Resolution
{
  public class ResolutionPath
  {
    readonly Stack<RegistrationKeyAndResolvedType> items;

    public ResolutionPath CreateChild(RegistrationKey key, Type resolvedType)
    {
      var newCollection = new Stack<RegistrationKeyAndResolvedType>(items);
      newCollection.Push(new RegistrationKeyAndResolvedType(key, resolvedType));
      return new ResolutionPath(newCollection);
    }

    public bool Contains(Type resolvedType)
    {
      if(resolvedType == null)
        throw new ArgumentNullException(nameof(resolvedType));
      
      return items.Any(x => x.ResolvedType == resolvedType);
    }

    public bool Contains(RegistrationKey key)
    {
      return items.Any(x => Equals(x.Key, key));
    }

    public Type[] GetTypes()
    {
      return items.Select(x => x.ResolvedType?? x.Key.Type).ToArray();
    }

    public override string ToString()
    {
      var reversedItems = items.ToArray().Reverse();
      return String.Join(",", reversedItems);

    }

    public ResolutionPath()
    {
      items = new Stack<RegistrationKeyAndResolvedType>();
    }

    ResolutionPath(Stack<RegistrationKeyAndResolvedType> items)
    {
      if(items == null)
        throw new ArgumentNullException(nameof(items));

      this.items = items;
    }

    class RegistrationKeyAndResolvedType
    {
      public RegistrationKey Key { get; private set; }
      public Type ResolvedType { get; private set; }

      public override string ToString() => $"{Key}:{ResolvedType}";

      public RegistrationKeyAndResolvedType(RegistrationKey key, Type resolvedType)
      {
        Key = key;
        ResolvedType = resolvedType;
      }
    }
  }
}
