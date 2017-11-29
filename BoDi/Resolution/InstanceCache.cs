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
using BoDi.Registrations;

namespace BoDi.Resolution
{
  public class InstanceCache : IPoolsServiceInstances, ICachesResolvedServices, IDisposable
  {
    bool isDisposed;
    readonly Dictionary<RegistrationKey,object> cache;

    public InstanceCache()
    {
      cache = new Dictionary<RegistrationKey, object>();
    }

    public void Add(RegistrationKey key, object obj)
    {
      AssertNotDisposed();
      cache.Add(key, obj);
    }

    public bool TryGet(RegistrationKey key, out object obj)
    {
      AssertNotDisposed();
      var result = cache.TryGetValue(key, out obj);

      if(!result)
      {
        obj = null;
        return false;
      }

      var wrapper = obj as NonDisposableWrapper;
      if(wrapper != null)
        obj = wrapper.Object;
      
      return true;
    }

    void AssertNotDisposed()
    {
      if(isDisposed)
        throw new InvalidOperationException("The cache must not be disposed");
    }

    void ICachesResolvedServices.Add(RegistrationKey key, object cachedObject)
    {
      Add(key, new NonDisposableWrapper(cachedObject));
    }

    void IPoolsServiceInstances.Add(RegistrationKey key, object instance)
    {
      Add(key, instance);
    }

    bool ICachesResolvedServices.TryGet(RegistrationKey key, out object cachedObject)
    {
      return TryGet(key, out cachedObject);
    }

    bool IPoolsServiceInstances.TryGet(RegistrationKey key, out object instance)
    {
      return TryGet(key, out instance);
    }

    bool ICachesResolvedServices.Contains(RegistrationKey key)
    {
      return cache.ContainsKey(key);
    }

    object IPoolsServiceInstances.GetOrReturnNull(RegistrationKey key)
    {
      object obj;
      if(TryGet(key, out obj))
        return obj;
      return null;
    }

    protected virtual void Dispose(bool disposing)
    {
      if(!isDisposed)
      {
        if(disposing)
        {
          foreach(var obj in cache.Values)
          {
            var disposable = obj as IDisposable;
            if(disposable != null)
              disposable.Dispose();
          }
        }

        isDisposed = true;
        cache.Clear();
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
  }
}
