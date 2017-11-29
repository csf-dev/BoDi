﻿// BoDi: A very simple IoC container.
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
using System.Reflection;
using BoDi.Resolution;

namespace BoDi.Registrations
{
  public class FactoryRegistration : Registration
  {
    readonly Delegate factoryDelegate;

    public FactoryRegistration(Delegate factoryDelegate, RegistrationKey key) : base(key)
    {
      this.factoryDelegate = factoryDelegate;
    }

    public override object Resolve(IObjectContainer container, RegistrationKey keyToResolve, ResolutionPath resolutionPath)
    {
      //TODO: store result object in pool?
      var obj = container.Resolver.Resolve(factoryDelegate, resolutionPath, keyToResolve);
      return obj;
    }
  }
}
