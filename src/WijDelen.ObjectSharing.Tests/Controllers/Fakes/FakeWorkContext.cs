using System;
using System.Collections.Generic;
using Orchard;

namespace WijDelen.ObjectSharing.Tests.Controllers.Fakes {
    public class FakeWorkContext : WorkContext {
        private readonly IDictionary<string, object> _state = new Dictionary<string, object>();

        public override T Resolve<T>()
        {
            throw new NotImplementedException();
        }

        public override object Resolve(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public override bool TryResolve<T>(out T service)
        {
            throw new NotImplementedException();
        }

        public override bool TryResolve(Type serviceType, out object service)
        {
            throw new NotImplementedException();
        }

        public override T GetState<T>(string name)
        {
            return (T)_state[name];
        }

        public override void SetState<T>(string name, T value)
        {
            _state[name] = value;
        }
    }
}