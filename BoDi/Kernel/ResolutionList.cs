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
using System.Diagnostics;
using System.Linq;

namespace BoDi.Kernel
{

  /// <summary>
  /// A very simple immutable linked list of <see cref="Type"/>.
  /// </summary>
  class ResolutionList
  {
    private readonly RegistrationKey currentRegistrationKey;
    private readonly Type currentResolvedType;
    private readonly ResolutionList nextNode;
    private bool IsLast { get { return nextNode == null; } }

    public ResolutionList()
    {
      Debug.Assert(IsLast);
    }

    private ResolutionList(RegistrationKey currentRegistrationKey, Type currentResolvedType, ResolutionList nextNode)
    {
      if (nextNode == null) throw new ArgumentNullException("nextNode");

      this.currentRegistrationKey = currentRegistrationKey;
      this.currentResolvedType = currentResolvedType;
      this.nextNode = nextNode;
    }

    public ResolutionList AddToEnd(RegistrationKey registrationKey, Type resolvedType)
    {
      return new ResolutionList(registrationKey, resolvedType, this);
    }

    public bool Contains(Type resolvedType)
    {
      if (resolvedType == null) throw new ArgumentNullException("resolvedType");
      return GetReverseEnumerable().Any(i => i.Value == resolvedType);
    }

    public bool Contains(RegistrationKey registrationKey)
    {
      return GetReverseEnumerable().Any(i => i.Key.Equals(registrationKey));
    }

    private IEnumerable<KeyValuePair<RegistrationKey, Type>> GetReverseEnumerable()
    {
      var node = this;
      while (!node.IsLast)
      {
        yield return new KeyValuePair<RegistrationKey, Type>(node.currentRegistrationKey, node.currentResolvedType);
        node = node.nextNode;
      }
    }

    public Type[] ToTypeList()
    {
      return GetReverseEnumerable().Select(i => i.Value ?? i.Key.Type).Reverse().ToArray();
    }

    public override string ToString()
    {
      return string.Join(",", GetReverseEnumerable().Select(n => string.Format("{0}:{1}", n.Key, n.Value)));
    }
  }
}
