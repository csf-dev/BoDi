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

namespace BoDi
{
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT

    public class ContainerRegistrationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ContainerRegistrationConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var registrationConfigElement = ((ContainerRegistrationConfigElement)element);
            string elementKey = registrationConfigElement.Interface;
            if (registrationConfigElement.Name != null)
                elementKey = elementKey + "/" + registrationConfigElement.Name;
            return elementKey;
        }

        public void Add(string implementationType, string interfaceType, string name = null)
        {
            BaseAdd(new ContainerRegistrationConfigElement
                        {
                            Implementation = implementationType,
                            Interface = interfaceType,
                            Name = name
                        });
        }
    }

#endif
}