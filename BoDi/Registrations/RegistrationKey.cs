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
using System.Diagnostics;

namespace BoDi.Registrations
{

  public struct RegistrationKey
  {
    public readonly Type Type;
    public readonly string Name;

    public RegistrationKey(Type type, string name)
    {
      if (type == null) throw new ArgumentNullException("type");

      Type = type;
      Name = name;
    }

    private Type TypeGroup
    {
      get
      {
        if (Type.IsGenericType && !Type.IsGenericTypeDefinition)
          return Type.GetGenericTypeDefinition();
        return Type;
      }
    }

    public override string ToString()
    {
      Debug.Assert(Type.FullName != null);
      if (Name == null)
        return Type.FullName;

      return string.Format("{0}('{1}')", Type.FullName, Name);
    }

    bool Equals(RegistrationKey other)
    {
      var isInvertable = other.TypeGroup == Type || other.Type == TypeGroup || other.Type == Type;
      return isInvertable && String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (obj.GetType() != typeof(RegistrationKey)) return false;
      return Equals((RegistrationKey)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return TypeGroup.GetHashCode();
      }
    }
  }
}
