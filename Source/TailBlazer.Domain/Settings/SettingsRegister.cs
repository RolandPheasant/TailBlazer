using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.Settings
{
    public class SettingsRegister : ISettingsRegister
    {
        private readonly IObjectRegister _register;
        private readonly ISettingFactory _settingFactory;

        public SettingsRegister([NotNull] IObjectRegister register, [NotNull] ISettingFactory settingFactory)
        {
            if (register == null) throw new ArgumentNullException(nameof(register));
            if (settingFactory == null) throw new ArgumentNullException(nameof(settingFactory));
            _register = register;
            _settingFactory = settingFactory;
        }

        public void Register<T>([NotNull] IConverter<T> converter, [NotNull] string key)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            if (key == null) throw new ArgumentNullException(nameof(key));
            var setting = _settingFactory.Create(converter, key);

            _register.Register<ISetting<T>>(setting);
        }
    }
}
