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

namespace BoDi.Config
{
  public class ContainerRegistrationConfigElement
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
    : ConfigurationElement
#endif
  {
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
    [ConfigurationProperty(ConfigConstants.ServiceTypeProperty, IsRequired = true)]
#endif
    public string Interface
    {
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
      get { return (string) this[ConfigConstants.ServiceTypeProperty]; }
      set { this[ConfigConstants.ServiceTypeProperty] = value; }
#else
      get; set;
#endif
    }

#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
    [ConfigurationProperty(ConfigConstants.ImplementationTypeProperty, IsRequired = true)]
#endif
    public string Implementation
    {
      #if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
      get { return (string) this[ConfigConstants.ImplementationTypeProperty]; }
      set { this[ConfigConstants.ImplementationTypeProperty] = value; }
#else
      get; set;
#endif
    }

#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
    [ConfigurationProperty(ConfigConstants.NameProperty, IsRequired = false, DefaultValue = null)]
#endif
    public string Name
    {
      #if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
      get { return (string) this[ConfigConstants.NameProperty]; }
      set { this[ConfigConstants.NameProperty] = value; }
#else
      get; set;
#endif
    }
  }
}