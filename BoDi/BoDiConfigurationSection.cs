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
using System.Configuration;
using BoDi.Config;

namespace BoDi
{

  // TODO: In a future breaking change, move this type into the BoDi.Config (or similar) namespace.
  // At present this type cannot be renamed nor may it be moved into another namespace, because these things
  // form its public API (in the config file).

  public class BoDiConfigurationSection
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
      : ConfigurationSection
#endif
  {
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
    [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
    [ConfigurationCollection(typeof(ContainerRegistrationCollection),
                             AddItemName = ConfigConstants.RegisterElementName)]
#endif
    public ContainerRegistrationCollection Registrations
    {
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
      get { return (ContainerRegistrationCollection) this[String.Empty]; }
      set { this[String.Empty] = value; }
#else
      get; set;
#endif
    }
  }
}