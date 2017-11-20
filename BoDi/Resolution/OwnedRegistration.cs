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
using BoDi.Registrations;

namespace BoDi.Resolution
{
  public class OwnedRegistration
  {
    public IObjectContainer Owner { get; private set; }

    public IRegistration Registration { get; private set; }

    public OwnedRegistration(IObjectContainer owner, IRegistration registration)
    {
      if(owner == null)
        throw new ArgumentNullException(nameof(owner));
      if(registration == null)
        throw new ArgumentNullException(nameof(registration));
      
      Owner = owner;
      Registration = registration;
    }
  }
}
