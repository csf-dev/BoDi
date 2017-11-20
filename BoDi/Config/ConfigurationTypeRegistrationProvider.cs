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
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using BoDi.Registrations;

namespace BoDi.Config
{
  public class ConfigurationTypeRegistrationProvider : IProvidesRegistrations, IProvidesTypeRegistrationsFromConfiguration
  {
    public virtual IReadOnlyList<IRegistration> GetRegistrations()
    {
      var section = GetConfigSection();
      if (section?.Registrations == null)
        return Enumerable.Empty<IRegistration>().ToArray();

      return GetRegistrations(section.Registrations);
    }

    public virtual IReadOnlyList<IRegistration> GetRegistrations(ContainerRegistrationCollection registrations)
    {
      return registrations.GetElements().Select(x => GetRegistration(x)).ToArray();
    }

    IRegistration GetRegistration(ContainerRegistrationConfigElement element)
    {
      var interfaceType = Type.GetType(element.Interface, true);
      var implementationType = Type.GetType(element.Implementation, true);
      var name = String.IsNullOrEmpty(element.Name) ? null : element.Name;

      return new TypeRegistration(implementationType, new RegistrationKey(interfaceType, name));
    }

    BoDiConfigurationSection GetConfigSection()
    {
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
      return (BoDiConfigurationSection) ConfigurationManager.GetSection(ConfigConstants.ConfigSectionPath);
#else
      Debug.WriteLine("System.Configuration is not supported in this build - performing a no-op");
      return null;
#endif
    }
  }
}
